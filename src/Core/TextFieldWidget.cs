using System;

namespace GameFramework
{
    public class TextFieldWidget : Widget
    {
        public string Text { get; private set; }
        public int MaxLength { get; private set; }
        public bool IsReadOnly { get; set; }
        public event Action<string>? OnTextChanged; // Made nullable

        public TextFieldWidget(string id, int x, int y, string initialText = "", int maxLength = 255) : base(id, x, y)
        {
            if (maxLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be greater than zero.");
            }
            MaxLength = maxLength;
            Text = initialText ?? string.Empty;
            if (Text.Length > MaxLength)
            {
                Text = Text.Substring(0, MaxLength);
            }
            IsReadOnly = false;
        }

        public void SetText(string newText)
        {
            if (IsReadOnly) return;

            string processedText = newText ?? string.Empty;
            if (processedText.Length > MaxLength)
            {
                processedText = processedText.Substring(0, MaxLength);
            }

            if (Text != processedText)
            {
                Text = processedText;
                OnTextChanged?.Invoke(Text);
            }
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                Console.WriteLine($"Drawing TextFieldWidget {Id} with text \"{Text}\" at ({X}, {Y})");
            }
        }
    }
}
