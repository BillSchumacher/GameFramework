using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Assuming Vector2 for MinValue, MaxValue, CurrentValue if needed for float values
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Linq
using OpenTK.Windowing.GraphicsLibraryFramework;

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
        private Dictionary<string, (AnchorPoint Anchor, int OffsetX, int OffsetY, int OriginalWidth, int OriginalHeight)> _originalChildSetups = new Dictionary<string, (AnchorPoint, int, int, int, int)>();

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
                float clampedValue = Math.Clamp(value, MinValue, MaxValue);
                if (_currentValue != clampedValue)
                {
                    _currentValue = clampedValue;
                    OnValueChanged?.Invoke(_currentValue);
                    if (Parent != null) 
                    {
                        // Update this widget's position based on its parent
                        UpdateActualPosition(Parent.ActualX, Parent.ActualY, Parent.WidgetWidth, Parent.WidgetHeight);
                    }
                    else if (FontRenderer.ScreenWidth > 0 && FontRenderer.ScreenHeight > 0)
                    {
                        // Update this widget's position based on screen dimensions if no parent
                        UpdateActualPosition(0, 0, FontRenderer.ScreenWidth, FontRenderer.ScreenHeight);
                    }
                }
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
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (_children.Contains(child)) return;

            _children.Add(child);
            _originalChildSetups[child.Id] = (child.Anchor, child.OffsetX, child.OffsetY, child.WidgetWidth, child.WidgetHeight);
            
            // Ensure positions are updated after adding a child.
            // The ScaleWidget itself needs its position updated based on its parent/screen.
            // This will then trigger an update for the new child as well within UpdateActualPosition.
            if (Parent != null)
            {
                UpdateActualPosition(Parent.ActualX, Parent.ActualY, Parent.WidgetWidth, Parent.WidgetHeight); 
            }
            else if (FontRenderer.ScreenWidth > 0 && FontRenderer.ScreenHeight > 0) 
            {
                 UpdateActualPosition(0, 0, FontRenderer.ScreenWidth, FontRenderer.ScreenHeight);
            }
        }

        public void RemoveChild(Widget child)
        {
            if (child == null) return;
            _children.Remove(child);
            _originalChildSetups.Remove(child.Id);
        }

        public IEnumerable<Widget> GetChildren()
        {
            return _children.AsReadOnly();
        }

        public override void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            // Update ScaleWidget's own position first
            base.UpdateActualPosition(parentActualX, parentActualY, containerWidth, containerHeight);

            float scaleFactor = GetScaleFactor();

            foreach (var child in _children)
            {
                if (!_originalChildSetups.TryGetValue(child.Id, out var setup))
                {
                    // Fallback, though AddChild should prevent this.
                    setup = (child.Anchor, child.OffsetX, child.OffsetY, child.WidgetWidth, child.WidgetHeight);
                    _originalChildSetups[child.Id] = setup; 
                }

                // Restore child's original anchor and offsets for positioning relative to ScaleWidget.
                child.Anchor = setup.Anchor;
                child.OffsetX = setup.OffsetX;
                child.OffsetY = setup.OffsetY;

                // Calculate scaled dimensions for the child based on its original dimensions.
                int originalChildWidth = setup.OriginalWidth;
                int originalChildHeight = setup.OriginalHeight;

                if (Orientation == Orientation.Horizontal)
                {
                    child.WidgetWidth = (int)(originalChildWidth * scaleFactor);
                    // Height remains original for horizontal scaling, or could also be scaled if desired.
                    // For now, only width is scaled.
                    child.WidgetHeight = originalChildHeight; 
                }
                else // Vertical
                {
                    child.WidgetHeight = (int)(originalChildHeight * scaleFactor);
                    // Width remains original for vertical scaling.
                    child.WidgetWidth = originalChildWidth;
                }

                // Child calculates its ActualX, ActualY relative to this ScaleWidget.
                // The container for the child is this ScaleWidget.
                child.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
            }
        }

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible) return;

            // Draw background of the ScaleWidget itself (e.g., the track)
            base.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix

            // TODO: Draw the thumb/handle of the scale widget
            // This would involve calculating the thumb's position based on CurrentValue
            // and then drawing a small quad or image for the thumb.
            // Example: DrawThumb(projectionMatrix);

            // Draw children (if any)
            // Children are scaled and positioned in UpdateActualPosition
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                child.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix
            }
        }
        
        public override bool OnMouseDown(float mouseX, float mouseY, MouseButton button)
        {
            if (!IsVisible) return false;

            bool isOverInteractiveArea = this.HitTest(mouseX, mouseY); 

            if (isOverInteractiveArea)
            {
                float relativeMouseX = mouseX - this.X;
                float relativeMouseY = mouseY - this.Y;

                if (Orientation == Orientation.Horizontal)
                {
                    if (this.WidgetWidth > 0)
                    {
                        float newValueRatio = Math.Clamp(relativeMouseX / this.WidgetWidth, 0.0f, 1.0f);
                        CurrentValue = MinValue + (MaxValue - MinValue) * newValueRatio;
                    }
                }
                else
                {
                    if (this.WidgetHeight > 0)
                    {
                        float newValueRatio = Math.Clamp(relativeMouseY / this.WidgetHeight, 0.0f, 1.0f);
                        CurrentValue = MinValue + (MaxValue - MinValue) * newValueRatio;
                    }
                }
                return true;
            }

            foreach (var child in _children.Reverse<Widget>()) 
            {
                if (child.IsVisible && child.HitTest(mouseX, mouseY)) 
                {
                    if (child.OnMouseDown(mouseX, mouseY, button))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool HitTest(float mouseX, float mouseY) 
        {
            if (!IsVisible) return false;
            // Check if mouse is over the ScaleWidget itself (its bounding box)
            return mouseX >= X && mouseX <= X + WidgetWidth &&
                   mouseY >= Y && mouseY <= Y + WidgetHeight;
        }

        private float GetScaleFactor()
        {
            if (MaxValue - MinValue != 0)
            {
                return (CurrentValue - MinValue) / (MaxValue - MinValue);
            }
            else if (MinValue == MaxValue && MaxValue != 0)
            {
                return CurrentValue == MinValue ? 1.0f : 0f;
            }
            return 0;
        }

        public Widget? Parent { get; set; }
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
