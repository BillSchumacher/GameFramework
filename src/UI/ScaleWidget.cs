using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Assuming Vector2 for MinValue, MaxValue, CurrentValue if needed for float values

namespace GameFramework.UI
{
    /// <summary>
    /// Represents a scale widget (slider) that allows selecting a value within a range.
    /// </summary>
    public class ScaleWidget : Widget
    {
        private float _minValue;
        private float _maxValue;
        private float _currentValue;
        private Orientation _orientation;

        /// <summary>
        /// Gets or sets the minimum value of the scale.
        /// </summary>
        public float MinValue
        {
            get => _minValue;
            set
            {
                if (value >= _maxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "MinValue cannot be greater than or equal to MaxValue.");
                }
                _minValue = value;
                CurrentValue = Math.Clamp(CurrentValue, _minValue, _maxValue); // Ensure current value is still valid
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the scale.
        /// </summary>
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                if (value <= _minValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxValue cannot be less than or equal to MinValue.");
                }
                _maxValue = value;
                CurrentValue = Math.Clamp(CurrentValue, _minValue, _maxValue); // Ensure current value is still valid
            }
        }

        /// <summary>
        /// Gets or sets the current value of the scale.
        /// </summary>
        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = Math.Clamp(value, _minValue, _maxValue);
                OnValueChanged?.Invoke(_currentValue);
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the scale (Horizontal or Vertical).
        /// </summary>
        public Orientation Orientation
        {
            get => _orientation;
            set => _orientation = value;
        }

        /// <summary>
        /// Event triggered when the value of the scale changes.
        /// </summary>
        public event Action<float>? OnValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleWidget"/> class with default values.
        /// </summary>
        public ScaleWidget() : this("default_scale", 0, 100, 50, Orientation.Horizontal, 0, 0, 100, 20)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleWidget"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the widget.</param>
        /// <param name="minValue">The minimum value of the scale.</param>
        /// <param name="maxValue">The maximum value of the scale.</param>
        /// <param name="initialValue">The initial value of the scale.</param>
        /// <param name="orientation">The orientation of the scale.</param>
        /// <param name="x">The x-coordinate of the widget's position.</param>
        /// <param name="y">The y-coordinate of the widget's position.</param>
        /// <param name="width">The width of the widget.</param>
        /// <param name="height">The height of the widget.</param>
        /// <param name="anchor">The anchor point for the widget.</param>
        /// <param name="offsetX">The offset from the anchor point on the x-axis.</param>
        /// <param name="offsetY">The offset from the anchor point on the y-axis.</param>
        [JsonConstructor]
        public ScaleWidget(string id, float minValue, float maxValue, float initialValue, Orientation orientation, int x, int y, int width, int height, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
            : base(id, x, y, anchor, offsetX, offsetY)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "MinValue must be less than MaxValue.");
            }

            _minValue = minValue;
            _maxValue = maxValue;
            Orientation = orientation;
            CurrentValue = Math.Clamp(initialValue, _minValue, _maxValue); // Set initial value, clamped

            WidgetWidth = width;
            WidgetHeight = height;
        }

        public override void Draw()
        {
            base.Draw();
            if (!IsVisible) return;

            // Placeholder for actual drawing logic
            // This would involve drawing the scale track and the thumb (handle)
            // For example:
            // DrawTrack();
            // DrawThumb();
            // Console.WriteLine($"Drawing ScaleWidget: {Id} at ({X}, {Y}) with value {CurrentValue}");
        }

        // Placeholder for interaction logic (e.g., OnMouseDown to drag the thumb)
        public override bool OnMouseDown(float mouseX, float mouseY, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button)
        {
            if (!IsVisible) return false;

            // Basic interaction: if clicked, set value based on click position (simplified)
            // This is a very simplified example and would need proper hit detection and value calculation
            // based on orientation and click position relative to the widget's bounds.
            if (IsMouseOver(mouseX, mouseY))
            {
                float newValue;
                if (Orientation == Orientation.Horizontal)
                {
                    float relativeX = mouseX - X;
                    newValue = MinValue + (relativeX / WidgetWidth) * (MaxValue - MinValue);
                }
                else // Vertical
                {
                    float relativeY = mouseY - Y;
                    newValue = MinValue + (relativeY / WidgetHeight) * (MaxValue - MinValue);
                }
                CurrentValue = newValue;
                return true; // Event handled
            }
            return false; // Event not handled
        }
        
        /// <summary>
        /// Helper method to check if mouse coordinates are over the widget.
        /// </summary>
        protected bool IsMouseOver(float mouseX, float mouseY)
        {
            return mouseX >= X && mouseX <= X + WidgetWidth &&
                   mouseY >= Y && mouseY <= Y + WidgetHeight;
        }
    }

    /// <summary>
    /// Defines the orientation for UI elements like ScaleWidget or StackPanel.
    /// </summary>
    public enum Orientation
    {
        Horizontal,
        Vertical
    }
}
