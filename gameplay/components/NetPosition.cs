using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Project_Ensemble.Shared;

namespace Project_Ensemble.Gameplay.Components {
  public struct NetPosition : INetworkComponent {
    public byte ComponentId => 1;
    public Vector2 Position;

    public void Serialize(NetDataWriter writer) {
      writer.Put(Position.X);
      writer.Put(Position.Y);
    }

    public void Deserialize(NetDataReader reader) {
      Position = new Vector2(reader.GetFloat(), reader.GetFloat());
    }
  }
}
