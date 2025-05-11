using System;
using System.Linq;

namespace GameFramework.UI // Changed namespace
{
    public class VerticalStackPanelWidget : StackPanelWidget
    {
        public VerticalStackPanelWidget(string id, int x, int y, int spacing) : base(id, x, y, spacing)
        {
        }

        public override void RecalculateLayout()
        {
            int currentY = this.Y;
            foreach (var child in _children)
            {
                child.SetPosition(this.X, currentY); // Align all children to the stack's X
                currentY += GetEffectiveHeight(child) + Spacing;
            }
        }

        public override void Draw()
        {
            if (!IsVisible) return;

            Console.WriteLine($"Drawing VerticalStackPanelWidget {Id} at ({X}, {Y}) with spacing {Spacing}");
            base.Draw(); // This will call Draw on children
        }
    }
}
