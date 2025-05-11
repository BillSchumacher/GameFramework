using System;

namespace GameFramework.UI // Changed namespace
{
    public class ButtonWidget : Widget
    {
        public string Text { get; private set; }
        public event Action? OnClick; // Made nullable

        public ButtonWidget(string id, int x, int y, string text) : base(id, x, y)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Button text cannot be null or empty.", nameof(text));
            }
            Text = text;
        }

        public void Click()
        {
            OnClick?.Invoke();
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                Console.WriteLine($"Drawing ButtonWidget {Id} with text \"{Text}\" at ({X}, {Y})");
            }
        }
    }
}
