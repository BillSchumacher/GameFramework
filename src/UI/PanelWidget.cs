using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GameFramework.UI
{
    public class PanelWidget : Widget
    {
        public int Width { get; set; } // Made settable
        public int Height { get; set; } // Made settable
        public List<Widget> Children { get; set; } // Made settable for serialization

        // Parameterless constructor for JSON deserialization
        public PanelWidget() : this("default_panel_id", 0, 0, 100, 100) // Default dimensions
        {
        }

        [JsonConstructor]
        public PanelWidget(string id, int x, int y, int width, int height) : base(id, x, y)
        {
            if (width <= 0 && GetType() == typeof(PanelWidget))
            {
                // throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
            }
            if (height <= 0 && GetType() == typeof(PanelWidget))
            {
                // throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
            }
            Width = width > 0 ? width : 100; // Ensure positive dimensions
            Height = height > 0 ? height : 100;
            Children = new List<Widget>();
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

        public override void Draw() // Added override
        {
            base.Draw();
            if (IsVisible)
            {
                Console.WriteLine($"Drawing PanelWidget {Id} at ({X}, {Y}) with size ({Width}x{Height})");
                foreach (var child in Children.Where(c => c.IsVisible))
                {
                    child.Draw(); // Assuming child has a Draw method.
                }
            }
        }
    }
}
