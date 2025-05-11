using System;
using System.Linq;

namespace GameFramework
{
    public class HorizontalStackPanelWidget : StackPanelWidget
    {
        public HorizontalStackPanelWidget(string id, int x, int y, int spacing) : base(id, x, y, spacing)
        {
        }

        public override void RecalculateLayout()
        {
            int currentX = this.X;
            foreach (var child in _children)
            {
                child.SetPosition(currentX, this.Y); // Align all children to the stack's Y
                currentX += GetEffectiveWidth(child) + Spacing;
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
