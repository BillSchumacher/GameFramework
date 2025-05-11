using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GameFramework.UI
{
    public abstract class StackPanelWidget : Widget
    {
        public int Spacing { get; set; } // Made settable
        public List<Widget> Children { get; set; } // Made settable for serialization

        // Parameterless constructor for JSON deserialization
        protected StackPanelWidget() : this("default_stackpanel_id", 0, 0, 5) // Default spacing
        {
        }

        [JsonConstructor]
        protected StackPanelWidget(string id, int x, int y, int spacing) : base(id, x, y)
        {
            Spacing = spacing;
            Children = new List<Widget>(); // Initialize the public list
        }

        public virtual void AddChild(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }
            if (Children.Contains(widget))
            {
                throw new InvalidOperationException($"Widget '{widget.Id}' is already a child of '{this.Id}'.");
            }
            Children.Add(widget);
            RecalculateLayout();
        }

        public virtual bool RemoveChild(Widget widget)
        {
            if (widget != null && Children.Remove(widget))
            {
                RecalculateLayout();
                return true;
            }
            return false;
        }

        public abstract void RecalculateLayout();

        public override void Draw() // Added override
        {
            base.Draw();
            if (IsVisible)
            {
                // Placeholder for drawing the panel itself, if it has its own visuals
                // Console.WriteLine($"Drawing StackPanel: {Id} at ({X}, {Y})");
                foreach (var child in Children)
                {
                    child.Draw(); // Children will also respect their IsVisible property
                }
            }
        }

        protected int GetEffectiveWidth(Widget widget)
        {
            if (widget is PanelWidget panel)
                return panel.Width;
            return 0; // Default for widgets without explicit width like Button, Label etc.
        }

        protected int GetEffectiveHeight(Widget widget)
        {
            if (widget is PanelWidget panel)
                return panel.Height;
            if (widget is ButtonWidget || widget is TextFieldWidget || widget is CheckboxWidget)
                return 20; // Arbitrary default height for simple widgets
            return 0; // Default for others
        }
    }
}
