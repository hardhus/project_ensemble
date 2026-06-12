using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Project_Ensemble.Core;
using Project_Ensemble.Server;
using Project_Ensemble.Client;
using Project_Ensemble.Shared;
using Project_Ensemble.Gameplay.Systems;
using Project_Ensemble.Gameplay.Components;
using Project_Ensemble.Gameplay.Inputs;
using SpriteFontPlus;

namespace Project_Ensemble.Gameplay {
  public enum SandboxState { MainMenu, LobbyMenu, InGame, Paused }

  public class EnsembleSandbox : GameCore {
    private SpriteBatch? _spriteBatch;
    private readonly List<IGameplaySystem> _systems = new();
    private KeyboardState _lastKeyboardState;

    public SandboxState CurrentState { get; set; } = SandboxState.MainMenu;
    public bool IsSinglePlayer { get; set; } = false;
    public Texture2D? WhitePixelRef { get; private set; }
    public SpriteFont? GameFontRef { get; private set; }

    public ServerEngine? ServerEngineRef => Server;
    public ClientEngine? ClientEngineRef => Client;

    public event Action<uint, IClientInput, EnsembleSandbox>? OnServerInputReceived;

    public EnsembleSandbox(bool isHeadless) : base(isHeadless) { }

    protected override void Initialize() {
      NetworkRegistry.RegisterComponent<NetPosition>();
      NetworkRegistry.RegisterInput<MovementInput>();

      _systems.Add(new ServerSimulationSystem());
      _systems.Add(new ClientRenderSystem());

      foreach (var sys in _systems) sys.Initialize(this);

      if (_isHeadless) CurrentState = SandboxState.InGame;

      base.Initialize();
    }

    public void StartGameNetwork(bool isHost) {
      if (isHost) {
        StartServer(65432);
        Server!.OnInputReceived += (entityId, input) => OnServerInputReceived?.Invoke(entityId, input, this);
      }
      ConnectClient("127.0.0.1", 65432);
    }

    public void StopGameNetwork() {
      Client?.Disconnect();
      Server?.Stop();
    }

    protected override void LoadContent() {
      if (!_isHeadless) {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        WhitePixelRef = new Texture2D(GraphicsDevice, 1, 1);
        WhitePixelRef.SetData(new[] { Color.White });

        try {
          byte[] fontData = System.IO.File.ReadAllBytes("assets/fonts/AKDPixel.ttf");

          var bakerResult = TtfFontBaker.Bake(fontData, 24, 1024, 1024, new[] {
            CharacterRange.BasicLatin,
            CharacterRange.Latin1Supplement
          });

          GameFontRef = bakerResult.CreateSpriteFont(GraphicsDevice);
          Console.WriteLine("[BOOT] Pixel Font basariyla hafizaya pisirildi ve MonoGame'e aktarildi!");
        } catch (Exception ex) {
          Console.WriteLine($"[WARNING] Font yukleme hatasi: {ex.Message}. Butonlar fontsuz kalabilir.");
        }
      }
      base.LoadContent();
    }

    protected override void Update(GameTime gameTime) {
      var keyboard = Keyboard.GetState();

      if (keyboard.IsKeyDown(Keys.Escape) && _lastKeyboardState.IsKeyUp(Keys.Escape)) {
        if (CurrentState == SandboxState.InGame) CurrentState = SandboxState.Paused;
        else if (CurrentState == SandboxState.Paused) CurrentState = SandboxState.InGame;
      }

      if (!(IsSinglePlayer && CurrentState == SandboxState.Paused)) {
        base.Update(gameTime);
      }

      foreach (var sys in _systems) sys.Update(gameTime, this);

      _lastKeyboardState = keyboard;
    }

    protected override void Draw(GameTime gameTime) {
      if (_isHeadless || _spriteBatch == null) return;

      GraphicsDevice.Clear(Color.Black);
      _spriteBatch.Begin();

      foreach (var sys in _systems) sys.Draw(_spriteBatch, this);

      _spriteBatch.End();
      base.Draw(gameTime);
    }
  }
}
