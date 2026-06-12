using System;
using System.Linq;

namespace Project_Ensemble {
  internal class Program {
    private static void Main(string[] args) {
      bool isServer = args.Contains("--server");

      using (var game = new Core.GameCore(isServer)) {
        if (!isServer) {
          Console.WriteLine("[BOOT] İstemci Modu Başlatıldı (Görsel Arayüz)");
        }

        game.Run();
      }
    }
  }
}
