using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_Ensemble.Gameplay.Components;
using Project_Ensemble.Gameplay.Inputs;
using Project_Ensemble.Gameplay.Ui;

namespace Project_Ensemble.Gameplay.Systems {
  public class ClientRenderSystem : IGameplaySystem {
    private MouseState _lastMouseState;

    private SimpleButton? _btnSinglePlayer;
    private SimpleButton? _btnMultiplayer;
    private SimpleButton? _btnHost;
    private SimpleButton? _btnJoin;
    private SimpleButton? _btnResume;
    private SimpleButton? _btnQuit;

    public void Initialize(EnsembleSandbox game) {
      _btnSinglePlayer = new SimpleButton(540, 250, 200, 50, "Tek Oyunculu");
      _btnMultiplayer = new SimpleButton(540, 330, 200, 50, "Cok Oyunculu");

      _btnHost = new SimpleButton(540, 250, 200, 50, "Sunucu Olustur");
      _btnJoin = new SimpleButton(540, 330, 200, 50, "Sunucuya Katil");

      _btnResume = new SimpleButton(540, 250, 200, 50, "Devam Et (ESC)");
      _btnQuit = new SimpleButton(540, 330, 200, 50, "Ana Menuye Don");
    }

    public void Update(GameTime gameTime, EnsembleSandbox game) {
      var mouseState = Mouse.GetState();
      var keyboard = Keyboard.GetState();

      if (game.CurrentState == SandboxState.MainMenu) {
        _btnSinglePlayer?.Update(mouseState);
        _btnMultiplayer?.Update(mouseState);

        if (_btnSinglePlayer!.IsClicked(mouseState, _lastMouseState)) {
          game.IsSinglePlayer = true;
          game.StartGameNetwork(isHost: true);
          game.CurrentState = SandboxState.InGame;
        } else if (_btnMultiplayer!.IsClicked(mouseState, _lastMouseState)) {
          game.IsSinglePlayer = false;
          game.CurrentState = SandboxState.LobbyMenu;
        }
      } else if (game.CurrentState == SandboxState.LobbyMenu) {
        _btnHost?.Update(mouseState);
        _btnJoin?.Update(mouseState);

        if (_btnHost!.IsClicked(mouseState, _lastMouseState)) {
          game.StartGameNetwork(isHost: true);
          game.CurrentState = SandboxState.InGame;
        } else if (_btnJoin!.IsClicked(mouseState, _lastMouseState)) {
          game.StartGameNetwork(isHost: false);
          game.CurrentState = SandboxState.InGame;
        }
      } else if (game.CurrentState == SandboxState.InGame) {
        if (game.ClientEngineRef == null) return;

        Vector2 dir = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.W)) dir.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) dir.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) dir.X -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) dir.X += 1;

        if (dir != Vector2.Zero) {
          dir.Normalize();
          game.ClientEngineRef.SendInput(new MovementInput { Direction = dir });
        }
      } else if (game.CurrentState == SandboxState.Paused) {
        _btnResume?.Update(mouseState);
        _btnQuit?.Update(mouseState);

        if (_btnResume!.IsClicked(mouseState, _lastMouseState)) {
          game.CurrentState = SandboxState.InGame;
        } else if (_btnQuit!.IsClicked(mouseState, _lastMouseState)) {
          game.StopGameNetwork();
          game.CurrentState = SandboxState.MainMenu;
        }
      }

      _lastMouseState = mouseState;
    }

    public void Draw(SpriteBatch spriteBatch, EnsembleSandbox game) {
      if (game.CurrentState == SandboxState.MainMenu) {
        _btnSinglePlayer?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
        _btnMultiplayer?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
      } else if (game.CurrentState == SandboxState.LobbyMenu) {
        _btnHost?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
        _btnJoin?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
      } else if (game.CurrentState == SandboxState.InGame || game.CurrentState == SandboxState.Paused) {
        if (game.ClientEngineRef != null) {
          foreach (var kvp in game.ClientEngineRef.LocalWorldState) {
            foreach (var comp in kvp.Value) {
              if (comp is NetPosition netPos) {
                spriteBatch.Draw(game.WhitePixelRef!, new Rectangle((int)netPos.Position.X, (int)netPos.Position.Y, 50, 50), Color.CornflowerBlue);
              }
            }
          }
        }

        if (game.CurrentState == SandboxState.Paused) {
          spriteBatch.Draw(game.WhitePixelRef!, new Rectangle(0, 0, 1280, 720), new Color(0, 0, 0, 150));
          _btnResume?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
          _btnQuit?.Draw(spriteBatch, game.WhitePixelRef!, game.GameFontRef);
        }
      }
    }
  }
}
