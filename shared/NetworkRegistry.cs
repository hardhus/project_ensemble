using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Project_Ensemble.Shared {
  public static class NetworkRegistry {
    private static readonly Dictionary<byte, Func<INetworkComponent>> _componentFactories = new();

    public static void RegisterComponent<T>() where T : INetworkComponent, new() {
      var temp = new T();
      byte id = temp.ComponentId;

      if (_componentFactories.ContainsKey(id)) {
        throw new InvalidOperationException($"[NetworkRegistry] ID {id} zaten başka bir bileşen tarafından kullanılıyor!");
      }

      _componentFactories[id] = () => new T();
      Console.WriteLine($"[NetworkRegistry] Bileşen Kaydedildi -> ID: {id}, Tür: {typeof(T).Name}");
    }

    public static INetworkComponent DeserializeComponent(byte componentId, NetDataReader reader) {
      if (!_componentFactories.TryGetValue(componentId, out var factory)) {
        Console.WriteLine($"[NetworkRegistry] [HATA] Bilinmeyen Bileşen ID'si: {componentId}");
        return null;
      }

      INetworkComponent component = factory();
      component.Deserialize(reader);
      return component;
    }
  }
}
