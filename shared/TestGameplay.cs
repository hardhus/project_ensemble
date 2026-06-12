using LiteNetLib.Utils;
using Microsoft.Xna.Framework;

namespace Project_Ensemble.Shared {
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

  public struct MovementInput : IClientInput {
    public byte InputTypeId => 1;
    public Vector2 Direction;

    public void Serialize(NetDataWriter writer) {
      writer.Put(Direction.X);
      writer.Put(Direction.Y);
    }

    public void Deserialize(NetDataReader reader) {
      Direction = new Vector2(reader.GetFloat(), reader.GetFloat());
    }
  }
}
