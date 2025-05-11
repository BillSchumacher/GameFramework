using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework
{
    public class PanelWidget : Widget
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private readonly List<Widget> _children;

        public IEnumerable<Widget> Children => _children.AsReadOnly();

        // Keep track of the parent to prevent adding a widget to multiple containers
        // This is a simplified parent tracking. A more robust solution might involve a Parent property on Widget.
        private static readonly Dictionary<Widget, PanelWidget> _widgetParents = new Dictionary<Widget, PanelWidget>();

        public PanelWidget(string id, int x, int y, int width, int height) : base(id, x, y)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
            }
            Width = width;
            Height = height;
            _children = new List<Widget>();
        }

        public void AddChild(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }
            if (_widgetParents.ContainsKey(widget))
            {
                 throw new InvalidOperationException($"Widget '{widget.Id}' already has a parent and cannot be added to '{this.Id}'.");
            }
            if (!_children.Contains(widget))
            {
                _children.Add(widget);
                _widgetParents[widget] = this;
            }
        }

        public bool RemoveChild(Widget widget)
        {
            if (widget != null && _children.Remove(widget))
            {
                _widgetParents.Remove(widget);
                return true;
            }
            return false;
        }

        public override void Draw()
        {
            if (!IsVisible) return;

            Console.WriteLine($"Drawing PanelWidget {Id} at ({X}, {Y}) with size ({Width}x{Height})");
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                // Child widgets are drawn relative to their own X, Y which are global for now.
                // For true panel containment, child X,Y might be relative to the panel's X,Y.
                // This would require adjusting their draw positions or transforming coordinates.
                // For simplicity, current Widget X,Y are treated as absolute screen positions.
                child.Draw();
            }
        }
    }
}
