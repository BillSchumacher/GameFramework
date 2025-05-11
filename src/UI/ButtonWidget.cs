using System;
using System.Text.Json.Serialization; // Required for JsonConstructor if used explicitly

namespace GameFramework.UI
{
    public class ButtonWidget : Widget
    {
        public string Text { get; set; } // Made settable
        public event Action? OnClick; // Made nullable

        // Parameterless constructor for JSON deserialization
        public ButtonWidget() : this("default_button_id", 0, 0, "Default Text")
        {
            // Default values, serializer will overwrite them
        }

        [JsonConstructor] // Explicitly mark the constructor for the serializer
        public ButtonWidget(string id, int x, int y, string text) : base(id, x, y)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                // For deserialization, allow null/empty and default it, or rely on property setter.
                // throw new ArgumentException("Text cannot be null or whitespace.", nameof(text));
            }
            Text = text ?? string.Empty; // Ensure Text is not null after construction
        }

        public void Click()
        {
            OnClick?.Invoke();
        }

        public override void Draw() // Added override
        {
            base.Draw(); // Important to call base.Draw() to check IsVisible
            if (IsVisible)
            {
                // Placeholder for actual drawing logic (e.g., using a graphics library)
                Console.WriteLine($"Drawing Button: {Text} at ({X}, {Y})");
            }
        }
    }
}
