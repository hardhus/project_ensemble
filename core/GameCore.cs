using Microsoft.Xna.Framework;
using Project_Ensemble.Server;
using Project_Ensemble.Client;

namespace Project_Ensemble.Core {
  public class GameCore : Game {
    protected GraphicsDeviceManager? _graphics;

    public ServerEngine? Server { get; private set; }
    public ClientEngine? Client { get; private set; }

    protected readonly bool _isHeadless;
    private double _serverTickInterval = 1.0 / 30.0;
    private double _serverTickTimer = 0.0;

    public GameCore(bool isHeadless) {
      _isHeadless = isHeadless;

      if (!_isHeadless) {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Window.Title = "Project Ensemble";
        IsMouseVisible = true;
      } else {
        Console.WriteLine("[BOOT] Server mode started (Headless Mode)");
      }
    }

    protected void StartServer(int port) {
      Server = new ServerEngine();
      Server.Start(port);
    }

    protected void ConnectClient(string host, int port) {
      Client = new ClientEngine();
      Client.Connect(host, port);
    }

    protected override void Initialize() {
      base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
      double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

      if (Server != null) {
        Server.Update();

        _serverTickTimer += deltaTime;
        if (_serverTickTimer >= _serverTickInterval) {
          _serverTickTimer -= _serverTickInterval;
          Server.BroadcastWorldState();
        }
      }

      if (Client != null) {
        Client.Update();
      }

      if (_isHeadless) {
        System.Threading.Thread.Sleep(1);
      }

      base.Update(gameTime);
    }

    protected override void OnExiting(object sender, ExitingEventArgs args) {
      Server?.Stop();
      Client?.Disconnect();
      base.OnExiting(sender, args);
    }
  }
}
