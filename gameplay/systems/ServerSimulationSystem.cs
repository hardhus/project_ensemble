using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project_Ensemble.Shared;
using Project_Ensemble.Gameplay.Components;
using Project_Ensemble.Gameplay.Inputs;

namespace Project_Ensemble.Gameplay.Systems {
  public class ServerSimulationSystem : IGameplaySystem {
    public void Initialize(EnsembleSandbox game) {
      game.OnServerInputReceived += HandleClientInput;
    }

    public void Update(GameTime gameTime, EnsembleSandbox game) {
      if (game.IsSinglePlayer && game.CurrentState == SandboxState.Paused)
        return;

      if (game.ServerEngineRef == null) return;

    }

    private void HandleClientInput(uint entityId, IClientInput input, EnsembleSandbox game) {
      if (game.ServerEngineRef == null) return;

      if (input is MovementInput moveInput) {
        if (game.ServerEngineRef.WorldState.TryGetValue(entityId, out var components)) {
          NetPosition netPos = new NetPosition { Position = new Vector2(400, 300) };

          for (int i = 0; i < components.Count; i++) {
            if (components[i] is NetPosition p) {
              netPos = p;
              components.RemoveAt(i);
              break;
            }
          }

          float speed = 8.0f;
          netPos.Position += moveInput.Direction * speed;

          components.Add(netPos);
        }
      }
    }

    public void Draw(SpriteBatch spriteBatch, EnsembleSandbox game) {
    }
  }
}
