using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Added for Matrix4

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
        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Corrected signature, added projectionMatrix
        {
            if (!IsVisible) return;

            // Base widget draw for background (if any)
            base.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix

            // Placeholder for actual drawing logic for the checkbox itself (e.g., box and checkmark)
            // This would likely involve its own shader or drawing primitives.
            // For now, only the background (if defined) will be drawn by the base call.
        }
    }
}
