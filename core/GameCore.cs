using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_Ensemble.Server;
using Project_Ensemble.Client;
using Project_Ensemble.Shared;

namespace Project_Ensemble.Core {
  public class GameCore : Game {
    private GraphicsDeviceManager? _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _whitePixel;

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
      // === 1. ADIM: FRAMEWORK'E BİLEŞENLERİ KAYDET ===
      NetworkRegistry.RegisterComponent<NetPosition>();
      NetworkRegistry.RegisterInput<MovementInput>();

      if (_isHeadless) {
        _serverEngine = new ServerEngine();
        // Sunucu girdileri yakaladığında ne yapacağını bağlıyoruz
        _serverEngine.OnInputReceived += Server_HandleInput;
        _serverEngine.Start(65432);
      } else {
        _serverEngine = new ServerEngine();
        _serverEngine.OnInputReceived += Server_HandleInput;
        _serverEngine.Start(65432);

        _clientEngine = new ClientEngine();
        _clientEngine.Connect("127.0.0.1", 65432);
      }

      base.Initialize();
    }

    protected override void LoadContent() {
      if (!_isHeadless) {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _whitePixel = new Texture2D(GraphicsDevice, 1, 1);
        _whitePixel.SetData(new[] { Color.White });
      }
    }

    private void Server_HandleInput(uint entityId, IClientInput input) {
      if (input is MovementInput moveInput && _serverEngine != null) {
        if (_serverEngine.WorldState.TryGetValue(entityId, out var components)) {
          NetPosition netPos = new NetPosition { Position = new Vector2(400, 300) };
          bool found = false;

          for (int i = 0; i < components.Count; i++) {
            if (components[i] is NetPosition p) {
              netPos = p;
              components.RemoveAt(i);
              found = true;
              break;
            }
          }
          float speed = 8.0f;
          netPos.Position += moveInput.Direction * speed;

          components.Add(netPos);
        }
      }
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

        var keyboard = Keyboard.GetState();
        Vector2 dir = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.W)) dir.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) dir.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) dir.X -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) dir.X += 1;

        if (dir != Vector2.Zero) {
          dir.Normalize();
          _clientEngine.SendInput(new MovementInput { Direction = dir });
        }
      }

      if (_isHeadless) {
        System.Threading.Thread.Sleep(1);
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
      if (_isHeadless || _spriteBatch == null || _whitePixel == null || _clientEngine == null) return;

      GraphicsDevice.Clear(Color.Black);

      _spriteBatch.Begin();

      foreach (var kvp in _clientEngine.LocalWorldState) {
        uint entityId = kvp.Key;
        List<INetworkComponent> components = kvp.Value;

        foreach (var comp in components) {
          if (comp is NetPosition netPos) {
            _spriteBatch.Draw(_whitePixel, new Rectangle((int)netPos.Position.X, (int)netPos.Position.Y, 50, 50), Color.CornflowerBlue);
          }
        }
      }

      _spriteBatch.End();

      base.Draw(gameTime);
    }

    protected override void OnExiting(object sender, ExitingEventArgs args) {
      _serverEngine?.Stop();
      _clientEngine?.Disconnect();
      base.OnExiting(sender, args);
    }
  }
}
