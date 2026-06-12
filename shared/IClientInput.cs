using LiteNetLib.Utils;

namespace Project_Ensemble.Shared {
  public interface IClientInput : INetSerializable {
    byte InputTypeId { get; }
  }
}
