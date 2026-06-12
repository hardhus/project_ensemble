using LiteNetLib.Utils;

namespace Project_Ensemble.Shared {
  public static class NetworkRegistry {
    private static readonly Dictionary<byte, Func<INetworkComponent>> _componentFactories = new();

    private static readonly Dictionary<byte, Func<IClientInput>> _inputFactories = new();

    public static void RegisterComponent<T>() where T : INetworkComponent, new() {
      var temp = new T();
      byte id = temp.ComponentId;

      if (_componentFactories.ContainsKey(id)) {
        throw new InvalidOperationException($"[NetworkRegistry] ID {id} zaten başka bir bileşen tarafından kullanılıyor!");
      }

      _componentFactories[id] = () => new T();
      Console.WriteLine($"[NetworkRegistry] Bileşen Kaydedildi -> ID: {id}, Tür: {typeof(T).Name}");
    }

    public static INetworkComponent? DeserializeComponent(byte componentId, NetDataReader reader) {
      if (!_componentFactories.TryGetValue(componentId, out var factory)) {
        Console.WriteLine($"[NetworkRegistry] [HATA] Bilinmeyen Bileşen ID'si: {componentId}");
        return null;
      }

      INetworkComponent component = factory();
      component.Deserialize(reader);
      return component;
    }

    public static void RegisterInput<T>() where T : IClientInput, new() {
      var temp = new T();
      byte id = temp.InputTypeId;

      if (_inputFactories.ContainsKey(id)) {
        throw new InvalidOperationException($"[NetworkRegistry] Girdi ID {id} zaten başka bir paket tarafından kullanılıyor!");
      }

      _inputFactories[id] = () => new T();
      Console.WriteLine($"[NetworkRegistry] Girdi Paketi Kaydedildi -> ID: {id}, Tür: {typeof(T).Name}");
    }

    public static IClientInput? DeserializeInput(byte inputTypeId, NetDataReader reader) {
      if (!_inputFactories.TryGetValue(inputTypeId, out var factory)) {
        Console.WriteLine($"[NetworkRegistry] Bilinmeyen Girdi Tipi ID'si: {inputTypeId}");
        return null;
      }

      IClientInput input = factory();
      input.Deserialize(reader);
      return input;
    }
  }
}
