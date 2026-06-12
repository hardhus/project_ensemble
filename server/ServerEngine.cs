using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Project_Ensemble.Shared;
using Open.Nat;

namespace Project_Ensemble.Server {
  public class ServerEngine {
    private readonly NetManager _netManager;
    private readonly EventBasedNetListener _listener;
    private Mapping? _portMapping;

    public Dictionary<uint, List<INetworkComponent>> WorldState { get; } = new();
    public Dictionary<int, uint> ClientRegistry { get; } = new();

    private uint _nextEntityId = 1;

    public event Action<uint, IClientInput>? OnInputReceived;

    public ServerEngine() {
      _listener = new EventBasedNetListener();
      _netManager = new NetManager(_listener) {
        AutoRecycle = true
      };
    }

    public void Start(int port) {
      _netManager.Start(port);

      Task.Run(async () => {
        try {
          var discoverer = new NatDiscoverer();

          using (var cts = new CancellationTokenSource(5000)) {
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

            if (device != null) {
              _portMapping = new Mapping(Protocol.Udp, port, port, "Project Ensemble Server");
              await device.CreatePortMapAsync(_portMapping);

              var externalIp = await device.GetExternalIPAsync();
              Console.WriteLine($"[SERVER] UPnP basarili! {port} UDP portu modemde otomatik acildi.");
              Console.WriteLine($"[SERVER] Arkadasinin baglanacagi Dis (Internet) IP: {externalIp}");
            }
          }
        } catch (Exception ex) {
          Console.WriteLine($"[SERVER] UPnP port acilamadi (Modemde UPnP kapali veya desteklemiyor): {ex.Message}");
        }
      });

      _listener.ConnectionRequestEvent += request => {
        request.AcceptIfKey("EnsembleSecretKey");
      };

      _listener.PeerConnectedEvent += peer => {
        Console.WriteLine($"[SERVER] Yeni istemci bağlandı! ID: {peer.Id}");
        uint entityId = _nextEntityId++;
        var initialPosition = new Gameplay.Components.NetPosition {
          Position = new Vector2(640, 360)
        };

        WorldState[entityId] = new List<INetworkComponent> { initialPosition };
        ClientRegistry[peer.Id] = entityId;
      };

      _listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
        if (ClientRegistry.TryGetValue(peer.Id, out uint entityId)) {
          WorldState.Remove(entityId);
          ClientRegistry.Remove(peer.Id);
        }
      };

      _listener.NetworkReceiveEvent += (fromPeer, reader, channel, deliveryMethod) => {
        try {
          byte inputTypeId = reader.GetByte();
          IClientInput? input = NetworkRegistry.DeserializeInput(inputTypeId, reader);

          if (input != null) {
            if (ClientRegistry.TryGetValue(fromPeer.Id, out uint entityId)) {
              OnInputReceived?.Invoke(entityId, input);
            }
          }
        } catch (Exception ex) {
          Console.WriteLine($"[SERVER] Girdi paketi okuma hatası: {ex.Message}");
        }
      };

      Console.WriteLine($"[SERVER] UDP Sunucusu {port} portunda başlatıldı.");
    }

    public void Update() {
      _netManager.PollEvents();
    }

    public void BroadcastWorldState() {
      if (_netManager.ConnectedPeersCount == 0) return;

      var packet = new WorldStatePacket { WorldData = WorldState };
      NetDataWriter writer = new NetDataWriter();
      packet.Serialize(writer);

      _netManager.SendToAll(writer, DeliveryMethod.Sequenced);
    }

    public void Stop() {
      if (_portMapping != null) {
        try {
          var discoverer = new NatDiscoverer();

          using (var cts = new CancellationTokenSource(2000)) {
            var device = discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts).Result;
            if (device != null) {
              device.DeletePortMapAsync(_portMapping).Wait();
              Console.WriteLine("[SERVER] UPnP port haritasi modemden temizlendi.");
            }
          }
        } catch { }
      }
      _netManager.Stop();
    }
  }
}
