using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_Ensemble.Core {
  public class GameCore : Game {
    private GraphicsDeviceManager _graphics;
    private readonly bool _isHeadless;

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
      base.Initialize();
    }

    protected override void Update(GameTime gameTime) {
      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
      if (_isHeadless) return;

      GraphicsDevice.Clear(Color.Black);
      base.Draw(gameTime);
    }
  }
}
