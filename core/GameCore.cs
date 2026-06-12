using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_Ensemble.Server;
using Project_Ensemble.Client;

namespace Project_Ensemble.Core {
  public class GameCore : Game {
    private GraphicsDeviceManager _graphics;
    private readonly bool _isHeadless;

    private ServerEngine? _serverEngine;
    private ClientEngine? _clientEngine;

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

    protected override void Initialize() {
      if (_isHeadless) {
        _serverEngine = new ServerEngine();
        _serverEngine.Start(65432);
      } else {
        _serverEngine = new ServerEngine();
        _serverEngine.Start(65432);

        _clientEngine = new ClientEngine();
        _clientEngine.Connect("127.0.0.1", 65432);
      }

      base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
      double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

      // SERVER LOOP
      if (_serverEngine != null) {
        _serverEngine.Update();

        _serverTickTimer += deltaTime;
        if (_serverTickTimer >= _serverTickInterval) {
          _serverTickTimer -= _serverTickInterval;

          _serverEngine.BroadcastWorldState();
        }
      }

      // ClIENT LOOP
      if (_clientEngine != null) {
        _clientEngine.Update();
      }

      if (_isHeadless) {
        System.Threading.Thread.Sleep(1);
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
      if (_isHeadless) return;

      GraphicsDevice.Clear(Color.Black);

      // TODO:  render

      base.Draw(gameTime);
    }

    protected override void OnExiting(object sender, ExitingEventArgs args) {
      _serverEngine?.Stop();
      _clientEngine?.Disconnect();
      base.OnExiting(sender, args);
    }
  }
}
