using System;
using System.Collections.Generic;
using System.Linq;
using GameFramework.UI; // Added using directive for the new UI namespace
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework; // Added for MouseButton
using OpenTK.Mathematics; // Added for Matrix4

namespace GameFramework
{
    public class UserInterface
    {
        public List<Widget> Widgets { get; set; } // Made settable

        // Parameterless constructor for JSON deserialization
        public UserInterface()
        {
            Widgets = new List<Widget>();
        }

        public void AddWidget(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget), "Widget cannot be null.");
            }
            if (Widgets.Any(w => w.Id == widget.Id))
            {
                throw new ArgumentException($"A widget with ID '{widget.Id}' already exists.", nameof(widget));
            }
            Widgets.Add(widget);
        }

        public bool RemoveWidget(string widgetId) // Changed parameter from Widget to string ID for consistency
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                throw new ArgumentException("Widget ID cannot be null or empty.", nameof(widgetId));
            }
            var widgetToRemove = Widgets.FirstOrDefault(w => w.Id == widgetId);
            if (widgetToRemove != null)
            {
                return Widgets.Remove(widgetToRemove);
            }
            return false;
        }

        public Widget? GetWidgetById(string id) // Renamed from GetWidget for clarity
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }
            return Widgets.FirstOrDefault(w => w.Id == id);
        }

        public IReadOnlyList<Widget> GetWidgets()
        {
            return Widgets.AsReadOnly();
        }

        public void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix parameter
        {
            foreach (var widget in Widgets.Where(w => w.IsVisible)) 
            {
                widget.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix to widget's Draw method
            }
        }

        public void HandleMouseDown(float mouseX, float mouseY, MouseButton button)
        {
            // Iterate in reverse order so the top-most widget gets the event first.
            for (int i = Widgets.Count - 1; i >= 0; i--)
            {
                var widget = Widgets[i];
                if (widget.IsVisible)
                {
                    // Perform hit-testing using ActualX and ActualY
                    if (mouseX >= widget.ActualX && mouseX <= widget.ActualX + widget.WidgetWidth &&
                        mouseY >= widget.ActualY && mouseY <= widget.ActualY + widget.WidgetHeight)
                    {
                        // If the widget handles the event, stop processing.
                        if (widget.OnMouseDown(mouseX, mouseY, button))
                        {
                            return; 
                        }
                    }
                }
            }
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }

        public static UserInterface? FromJson(string json)
        {
            var options = new JsonSerializerOptions { };
            return JsonSerializer.Deserialize<UserInterface>(json, options);
        }
    }
}
