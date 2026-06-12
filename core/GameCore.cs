using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Project_Ensemble {
	public class GameCore : Game {
		private GraphicsDeviceManager _graphics;

		public GameCore(){
			_graphics = new GraphicsDeviceManager(this);

			_graphics.PreferredBackBufferWidth = 1280;
			_graphics.PreferredBackBufferHeight = 720;
			_graphics.ApplyChanges();

			Window.Title = "Project Ensemble";
			IsMouseVisible = true;
		}

		protected override void Initialize() {
			base.Initialize();
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);
			base.Draw(gameTime);
		}
	}
}
