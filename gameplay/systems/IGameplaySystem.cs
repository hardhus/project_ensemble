using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_Ensemble.Gameplay.Systems {
  public interface IGameplaySystem {
    void Initialize(EnsembleSandbox game);
    void Update(GameTime gameTime, EnsembleSandbox game);
    void Draw(SpriteBatch spriteBatch, EnsembleSandbox game);
  }
}
