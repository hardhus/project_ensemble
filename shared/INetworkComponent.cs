using LiteNetLib.Utils;

namespace Project_Ensemble.Shared {
  public interface INetworkComponent : INetSerializable {
    byte ComponentId { get; }
  }
}
