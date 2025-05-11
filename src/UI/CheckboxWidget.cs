using System;
using System.Text.Json.Serialization;

namespace GameFramework.UI
{
    public class CheckboxWidget : Widget
    {
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public Action<bool>? OnCheckedChanged { get; set; }

        // Parameterless constructor for JSON deserialization
        public CheckboxWidget() : this("default_checkbox", "Checkbox", false, 0, 0, null)
        {
        }

        [JsonConstructor]
        public CheckboxWidget(string id, string text, bool isChecked, int x, int y, Action<bool>? onCheckedChanged) : base(id, x, y)
        {
            Text = text;
            IsChecked = isChecked;
            OnCheckedChanged = onCheckedChanged;
        }

        public void Toggle()
        {
            IsChecked = !IsChecked;
            OnCheckedChanged?.Invoke(IsChecked);
        }

        // Example of a custom Draw method for a CheckboxWidget
        public override void Draw() // Added override
        {
            base.Draw();
            if (IsVisible)
            {
                // Placeholder for actual drawing logic
                Console.WriteLine($"Drawing Checkbox: {Text} [{(IsChecked ? "X" : " ")}] at ({X}, {Y})");
            }
        }
    }
}
