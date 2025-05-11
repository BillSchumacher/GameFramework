using System;
using System.Linq;
using OpenTK.Mathematics; // Added for Matrix4

namespace GameFramework.UI
{
    public class HorizontalStackPanelWidget : StackPanelWidget
    {
        public HorizontalStackPanelWidget(string id, int x, int y, int width, int height, int spacing, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
            : base(id, x, y, width, height, Orientation.Horizontal, anchor, offsetX, offsetY)
        {
            Spacing = spacing;
        }

        public override void RecalculateLayout()
        {
            int currentX = 0;
            int maxHeight = 0;
            foreach (var child in Children)
            {
                child.X = currentX; // Position child relative to stack panel
                child.Y = 0;      // Align to top of stack panel
                currentX += child.WidgetWidth + Spacing;
                if (child.WidgetHeight > maxHeight)
                {
                    maxHeight = child.WidgetHeight;
                }
            }
            // Optional: Adjust StackPanel size based on children
            // WidgetWidth = currentX - Spacing; // if Spacing is added at the end
            // WidgetHeight = maxHeight;
        }

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            base.Draw(elapsedTime, projectionMatrix); // Call base to draw background if any, then draw children
        }
    }
}
