using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using OpenTK.Mathematics; // Added for Vector4

namespace GameFramework.UI
{
    public class PanelWidget : Widget
    {
        // Removed Width and Height properties, will use base.WidgetWidth and base.WidgetHeight
        public List<Widget> Children { get; set; }

        // Parameterless constructor for JSON deserialization
        public PanelWidget() : this("default_panel_id", 0, 0, 100, 100) 
        {
        }

        [JsonConstructor]
        public PanelWidget(string id, int x, int y, int width, int height, Vector4? backgroundColor = null, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0) 
            : base(id, x, y, anchor, offsetX, offsetY)
        {
            WidgetWidth = width > 0 ? width : 100; // Use base class property
            WidgetHeight = height > 0 ? height : 100; // Use base class property
            Children = new List<Widget>();
            if (backgroundColor.HasValue)
            {
                BackgroundColor = backgroundColor.Value;
            }
        }

        public void AddChild(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }
            if (!Children.Contains(widget))
            {
                Children.Add(widget);
                // Optional: Trigger an update if needed, e.g., this.UpdateActualPosition(parentWidth, parentHeight)
                // but parentWidth/Height are not known here. The container should do it.
            }
        }

        public bool RemoveChild(Widget widget)
        {
            if (widget != null && Children.Remove(widget))
            {
                return true;
            }
            return false;
        }

        public override void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            // Update PanelWidget's own position first
            base.UpdateActualPosition(parentActualX, parentActualY, containerWidth, containerHeight);

            // Then update children, they are positioned relative to this panel's ActualX, ActualY
            // The container for child anchoring is this panel itself.
            foreach (var child in Children)
            {
                child.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
            }
        }

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible) return;

            base.Draw(elapsedTime, projectionMatrix); // Draw panel background

            // Draw children
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                // Ensure child's actual position is updated relative to this panel
                // This might be redundant if UpdateActualPosition is called systematically elsewhere (e.g., before Draw pass)
                // child.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight); 
                child.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix
            }
        }
    }
}
