using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Project_Ensemble.Shared;

namespace Project_Ensemble.Gameplay.Inputs {
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
