using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Project_Ensemble.Shared {
  public class WorldStatePacket : INetSerializable {
    public Dictionary<uint, List<INetworkComponent>> WorldData { get; set; } = new();

    public void Serialize(NetDataWriter writer) {
      writer.Put(WorldData.Count);

      foreach (var kvp in WorldData) {
        uint entityId = kvp.Key;
        List<INetworkComponent> components = kvp.Value;

        writer.Put(entityId);

        writer.Put((byte)components.Count);

        foreach (var component in components) {
          writer.Put(component.ComponentId);
          component.Serialize(writer);
        }
      }
    }

    public void Deserialize(NetDataReader reader) {
      WorldData.Clear();

      int entityCount = reader.GetInt();

      for (int i = 0; i < entityCount; i++) {
        uint entityId = reader.GetUInt();

        byte componentCount = reader.GetByte();

        var componentsList = new List<INetworkComponent>();

        for (int j = 0; j < componentCount; j++) {
          byte componentId = reader.GetByte();

          INetworkComponent component = NetworkRegistry.DeserializeComponent(componentId, reader);

          if (component != null) {
            componentsList.Add(component);
          }
        }

        WorldData[entityId] = componentsList;
      }
    }
  }
}
