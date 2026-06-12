using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Project_Ensemble.Gameplay.Ui {
  public class SimpleTextBox {
    public Rectangle Bounds { get; set; }
    public string Text { get; private set; } = "127.0.0.1";
    public bool IsSelected { get; private set; }

    private KeyboardState _lastKeyboardState;

    public SimpleTextBox(int x, int y, int width, int height) {
      Bounds = new Rectangle(x, y, width, height);
    }

    public void Update(MouseState mouseState, KeyboardState keyboardState) {
      if (mouseState.LeftButton == ButtonState.Pressed) {
        IsSelected = Bounds.Contains(mouseState.X, mouseState.Y);
      }

      if (!IsSelected) return;

      foreach (var key in keyboardState.GetPressedKeys()) {
        if (_lastKeyboardState.IsKeyUp(key)) {
          if (key == Keys.Back && Text.Length > 0) {
            Text = Text.Substring(0, Text.Length - 1);
          } else if (Text.Length < 15) {
            string keyChar = GetCharFromKey(key);
            if (!string.IsNullOrEmpty(keyChar)) {
              Text += keyChar;
            }
          }
        }
      }
      _lastKeyboardState = keyboardState;
    }

    private string GetCharFromKey(Keys key) {
      if (key >= Keys.D0 && key <= Keys.D9) return (key - Keys.D0).ToString();
      if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return (key - Keys.NumPad0).ToString();
      if (key == Keys.OemPeriod || key == Keys.Decimal) return ".";
      return "";
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D whitePixel, SpriteFont? font) {
      spriteBatch.Draw(whitePixel, Bounds, IsSelected ? Color.Yellow : Color.Gray);

      Rectangle innerBounds = new Rectangle(Bounds.X + 2, Bounds.Y + 2, Bounds.Width - 4, Bounds.Height - 4);
      spriteBatch.Draw(whitePixel, innerBounds, Color.Black);

      if (font != null && !string.IsNullOrEmpty(Text)) {
        Vector2 textSize = font.MeasureString(Text);
        Vector2 textPos = new Vector2(Bounds.X + 10, Bounds.Y + (Bounds.Height - textSize.Y) / 2);
        spriteBatch.DrawString(font, Text, textPos, Color.GreenYellow);
      }
    }
  }
}
