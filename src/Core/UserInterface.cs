using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework
{
    public class UserInterface
    {
        private readonly List<Widget> _widgets;

        public UserInterface()
        {
            _widgets = new List<Widget>();
        }

        public void AddWidget(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget), "Widget cannot be null.");
            }
            if (_widgets.Any(w => w.Id == widget.Id))
            {
                throw new ArgumentException($"A widget with ID '{widget.Id}' already exists.", nameof(widget));
            }
            _widgets.Add(widget);
        }

        public bool RemoveWidget(string widgetId)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                throw new ArgumentException("Widget ID cannot be null or empty.", nameof(widgetId));
            }
            var widgetToRemove = _widgets.FirstOrDefault(w => w.Id == widgetId);
            if (widgetToRemove != null)
            {
                return _widgets.Remove(widgetToRemove);
            }
            return false;
        }

        public Widget? GetWidget(string widgetId)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                throw new ArgumentException("Widget ID cannot be null or empty.", nameof(widgetId));
            }
            return _widgets.FirstOrDefault(w => w.Id == widgetId);
        }

        public IReadOnlyList<Widget> GetWidgets()
        {
            return _widgets.AsReadOnly();
        }

        public void Draw()
        {
            foreach (var widget in _widgets)
            {
                widget.Draw(); // Widget's Draw method will check IsVisible
            }
        }
    }
}
