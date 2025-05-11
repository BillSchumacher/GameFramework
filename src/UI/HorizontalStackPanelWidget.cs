using System;
using System.Linq;

namespace GameFramework.UI
{
    public class HorizontalStackPanelWidget : StackPanelWidget
    {
        public HorizontalStackPanelWidget(string id, int x, int y, int spacing) : base(id, x, y, spacing)
        {
        }

        public override void RecalculateLayout()
        {
            int currentX = this.X;
            foreach (var child in Children)
            {
                child.SetPosition(currentX, this.Y); // Align all children to the stack's Y
                var childWidth = 50; // Placeholder: replace with actual width logic
                if (child is PanelWidget panel) childWidth = panel.Width;
                currentX += childWidth + Spacing;
            }
        }

        public override void Draw()
        {
            if (!IsVisible) return;

            Console.WriteLine($"Drawing HorizontalStackPanelWidget {Id} at ({X}, {Y}) with spacing {Spacing}");
            base.Draw(); // This will call Draw on children
        }
    }
}
