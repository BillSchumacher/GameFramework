using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.UI // Changed namespace
{
    public abstract class StackPanelWidget : Widget
    {
        public int Spacing { get; protected set; }
        protected readonly List<Widget> _children;
        public IEnumerable<Widget> Children => _children.AsReadOnly();

        // Basic parent tracking to prevent a widget from being in multiple containers easily.
        // A more robust system might involve a Parent property on Widget itself.
        protected static readonly Dictionary<Widget, Widget> _widgetParents = new Dictionary<Widget, Widget>();

        protected StackPanelWidget(string id, int x, int y, int spacing) : base(id, x, y)
        {
            Spacing = spacing;
            _children = new List<Widget>();
        }

        public virtual void AddChild(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }
            if (_widgetParents.ContainsKey(widget) && _widgetParents[widget] != this) // Allow re-adding to same parent if logic permits
            {
                 throw new InvalidOperationException($"Widget '{widget.Id}' already has a parent '{(_widgetParents[widget] as Widget)?.Id ?? "unknown"}' and cannot be added to '{this.Id}'.");
            }
            if (!_children.Contains(widget))
            {
                _children.Add(widget);
                _widgetParents[widget] = this;
                RecalculateLayout();
            }
        }

        public virtual bool RemoveChild(Widget widget)
        {
            if (widget != null && _children.Remove(widget))
            {
                _widgetParents.Remove(widget);
                RecalculateLayout();
                return true;
            }
            return false;
        }

        public abstract void RecalculateLayout();

        public override void Draw()
        {
            if (!IsVisible) return;
            // Base stack panel might draw a border or background if desired
            // Console.WriteLine($"Drawing StackPanel {Id} at ({X}, {Y})"); 
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                child.Draw();
            }
        }

        protected int GetEffectiveWidth(Widget widget)
        {
            if (widget is PanelWidget panel)
                return panel.Width;
            // Add more specific checks if other widgets have explicit widths
            // For a simple ButtonWidget or TextFieldWidget, width might be based on content or a default.
            // For layout purposes in a stack panel, if not specified, could be considered 0 or a min value.
            // This is a simplification; real UI frameworks have complex size negotiation.
            return 0; // Default for widgets without explicit width like Button, Label etc.
        }

        protected int GetEffectiveHeight(Widget widget)
        {
            if (widget is PanelWidget panel)
                return panel.Height;
            // Similar to GetEffectiveWidth, this is a simplification.
            // Text-based widgets might calculate height based on font size, lines of text.
            // For now, assume non-panel widgets have a minimal or default height for stacking.
            // Let's assume a default height for simple widgets for now, e.g., 20 units.
            if (widget is ButtonWidget || widget is TextFieldWidget || widget is CheckboxWidget)
                return 20; // Arbitrary default height for simple widgets
            return 0; // Default for others
        }
    }
}
