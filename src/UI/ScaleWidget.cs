using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Assuming Vector2 for MinValue, MaxValue, CurrentValue if needed for float values
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Linq

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
        private List<Widget> _children = new List<Widget>();
        private Dictionary<Widget, (int originalWidth, int originalHeight)> _originalChildDimensions = new Dictionary<Widget, (int, int)>();

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
                ApplyScaleToChildren();
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

        public void AddChild(Widget child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                _originalChildDimensions[child] = (child.WidgetWidth, child.WidgetHeight);
                ApplyScaleToChild(child);
            }
        }

        public void RemoveChild(Widget child)
        {
            if (_children.Remove(child))
            {
                _originalChildDimensions.Remove(child);
            }
        }

        private void ApplyScaleToChildren()
        {
            foreach (var child in _children)
            {
                ApplyScaleToChild(child);
            }
        }

        private void ApplyScaleToChild(Widget child)
        {
            if (_originalChildDimensions.TryGetValue(child, out var originalDimensions))
            {
                float scaleFactor = 0;
                if (MaxValue - MinValue != 0)
                {
                    scaleFactor = (CurrentValue - MinValue) / (MaxValue - MinValue);
                }
                else if (MinValue == MaxValue && MaxValue != 0)
                {
                    scaleFactor = CurrentValue == MinValue ? 1.0f : 0f;
                }
                scaleFactor = Math.Max(0, scaleFactor);

                child.WidgetWidth = (int)(originalDimensions.originalWidth * scaleFactor);
                child.WidgetHeight = (int)(originalDimensions.originalHeight * scaleFactor);
                child.UpdateActualPosition(this.WidgetWidth, this.WidgetHeight);
            }
        }

        public override void UpdateActualPosition(int parentWidth, int parentHeight)
        {
            base.UpdateActualPosition(parentWidth, parentHeight);
            foreach (var child in _children)
            {
                child.UpdateActualPosition(this.WidgetWidth, this.WidgetHeight);
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (!IsVisible) return;

            foreach (var child in _children)
            {
                if (child.IsVisible)
                {
                    child.Draw();
                }
            }
        }

        public override bool OnMouseDown(float mouseX, float mouseY, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton button)
        {
            if (!IsVisible) return false;

            bool selfInteracted = false;
            if (IsMouseOver(mouseX, mouseY))
            {
                float newValue;
                if (Orientation == Orientation.Horizontal)
                {
                    float relativeX = mouseX - this.X;
                    newValue = MinValue + (relativeX / WidgetWidth) * (MaxValue - MinValue);
                }
                else
                {
                    float relativeY = mouseY - this.Y;
                    newValue = MinValue + (relativeY / WidgetHeight) * (MaxValue - MinValue);
                }
                CurrentValue = newValue;
                selfInteracted = true;
            }

            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                if (child.IsVisible && child.OnMouseDown(mouseX, mouseY, button))
                {
                    return true;
                }
            }

            return selfInteracted;
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
