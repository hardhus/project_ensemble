using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project_Ensemble.Gameplay.Ui {
  public class SimpleButton {
    public Rectangle Bounds { get; set; }
    public string Text { get; set; }
    public Color NormalColor { get; set; } = Color.DarkSlateGray;
    public Color HoverColor { get; set; } = Color.SlateGray;

    private bool _isHovered;

    public SimpleButton(int x, int y, int width, int height, string text) {
      Bounds = new Rectangle(x, y, width, height);
      Text = text;
    }

    public void Update(MouseState mouseState) {
      _isHovered = Bounds.Contains(mouseState.X, mouseState.Y);
    }

    public bool IsClicked(MouseState mouseState, MouseState lastMouseState) {
      return _isHovered &&
             mouseState.LeftButton == ButtonState.Pressed &&
             lastMouseState.LeftButton == ButtonState.Released;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel, SpriteFont? font) {
      spriteBatch.Draw(whitePixel, Bounds, _isHovered ? HoverColor : NormalColor);

      if (font != null) {
        Vector2 textSize = font.MeasureString(Text);
        Vector2 textPos = new Vector2(
          Bounds.X + (Bounds.Width - textSize.X) / 2,
          Bounds.Y + (Bounds.Height - textSize.Y) / 2
        );
        spriteBatch.DrawString(font, Text, textPos, Color.White);
      }
    }
  }
}
