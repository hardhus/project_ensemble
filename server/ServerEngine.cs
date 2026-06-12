using LiteNetLib;
using LiteNetLib.Utils;
using Project_Ensemble.Shared;

namespace Project_Ensemble.Server {
  public class ServerEngine {
    private readonly NetManager _netManager;
    private readonly EventBasedNetListener _listener;

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

      _listener.ConnectionRequestEvent += request => {
        request.AcceptIfKey("EnsembleSecretKey");
      };

      _listener.PeerConnectedEvent += peer => {
        Console.WriteLine($"[SERVER] Yeni istemci bağlandı! ID: {peer.Id}");
        uint entityId = _nextEntityId++;
        WorldState[entityId] = new List<INetworkComponent>();
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
      _netManager.Stop();
    }
  }
}
