using LiteNetLib;
using LiteNetLib.Utils;
using Project_Ensemble.Shared;

namespace Project_Ensemble.Client {
  public class ClientEngine {
    private readonly NetManager _netManager;
    private readonly EventBasedNetListener _listener;
    private NetPeer? _serverPeer;

    public Dictionary<uint, List<INetworkComponent>> LocalWorldState { get; private set; } = new();

    public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;

    public ClientEngine() {
      _listener = new EventBasedNetListener();
      _netManager = new NetManager(_listener) {
        AutoRecycle = true
      };
    }

    public void Connect(string host, int port) {
      _netManager.Start();
      _serverPeer = _netManager.Connect(host, port, "EnsembleSecretKey");

      _listener.PeerConnectedEvent += peer => {
        Console.WriteLine($"[CLIENT] Sunucuya basariyla baglanildi! Sunucu ID: {peer.Id}");
      };

      _listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
        Console.WriteLine($"[CLIENT] Sunucu ile baglanti koptu: {disconnectInfo.Reason}");
        _serverPeer = null;
        LocalWorldState.Clear();
      };

      _listener.NetworkReceiveEvent += (fromPeer, reader, channel, deliveryMethod) => {
        try {
          var packet = new WorldStatePacket();
          packet.Deserialize(reader);
          LocalWorldState = packet.WorldData;
        } catch (Exception ex) {
          Console.WriteLine($"[CLIENT] Paket okuma hatasi: {ex.Message}");
        }
      };
    }

    public void Update() {
      _netManager.PollEvents();
    }

    public void Disconnect() {
      _netManager.Stop();
      _serverPeer = null;
    }

    public void SendInput(IClientInput input) {
      if (!IsConnected || _serverPeer == null) return;

      NetDataWriter writer = new NetDataWriter();
      writer.Put(input.InputTypeId);
      input.Serialize(writer);

      _serverPeer.Send(writer, DeliveryMethod.Sequenced);
    }
  }
}
