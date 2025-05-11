using System;
using System.Linq;
using OpenTK.Mathematics; // Added for Matrix4

namespace GameFramework.UI
{
    public class VerticalStackPanelWidget : StackPanelWidget
    {
        public VerticalStackPanelWidget(string id, int x, int y, int width, int height, int spacing, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
            : base(id, x, y, width, height, Orientation.Vertical, anchor, offsetX, offsetY)
        {
            Spacing = spacing;
        }

        public override void RecalculateLayout()
        {
            int currentY = 0;
            int maxWidth = 0;
            foreach (var child in Children)
            {
                child.X = 0;      // Align to left of stack panel
                child.Y = currentY; // Position child relative to stack panel
                currentY += child.WidgetHeight + Spacing;
                if (child.WidgetWidth > maxWidth)
                {
                    maxWidth = child.WidgetWidth;
                }
            }
            // Optional: Adjust StackPanel size based on children
            // WidgetHeight = currentY - Spacing; // if Spacing is added at the end
            // WidgetWidth = maxWidth;
        }

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            base.Draw(elapsedTime, projectionMatrix); // Call base to draw background if any, then draw children
        }
    }
}
