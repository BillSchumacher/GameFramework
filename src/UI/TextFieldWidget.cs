using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Added for Matrix4

namespace GameFramework.UI
{
    public class TextFieldWidget : Widget
    {
        public string Text { get; set; } // Made settable
        public int MaxLength { get; set; } // Made settable
        public bool IsReadOnly { get; set; }
        public string Placeholder { get; set; } // Added Placeholder property
        public event Action<string>? OnTextChanged;

        // Parameterless constructor for JSON deserialization
        public TextFieldWidget() : this("default_textfield_id", 0, 0, string.Empty, 100, "Enter text...") // Default MaxLength to a positive value
        {
        }

        [JsonConstructor]
        public TextFieldWidget(string id, int x, int y, string? initialText = null, int maxLength = 0, string placeholder = "Enter text...") : base(id, x, y)
        {
            if (maxLength <= 0 && GetType() == typeof(TextFieldWidget)) // Allow derived types to bypass, or set a default positive value
            {
                // throw new ArgumentOutOfRangeException(nameof(maxLength), "MaxLength must be positive.");
                // For deserialization, allow 0 or negative, or ensure a valid default is set.
                // Here, the parameterless constructor sets a valid default.
            }
            Text = initialText ?? string.Empty;
            MaxLength = maxLength > 0 ? maxLength : 100; // Ensure MaxLength is positive after construction
            Placeholder = placeholder; // Initialize Placeholder
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

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible) return;

            // Draw background using base class logic
            base.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix

            // Placeholder for text field specific drawing (text, cursor, border)
            // FontRenderer.Instance.DrawText(...);
        }
    }
}
