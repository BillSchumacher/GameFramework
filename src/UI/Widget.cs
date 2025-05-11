using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFramework.UI
{
    [JsonDerivedType(typeof(ButtonWidget), "ButtonWidget")]
    [JsonDerivedType(typeof(CheckboxWidget), "CheckboxWidget")]
    [JsonDerivedType(typeof(PanelWidget), "PanelWidget")]
    [JsonDerivedType(typeof(HorizontalStackPanelWidget), "HorizontalStackPanelWidget")]
    [JsonDerivedType(typeof(VerticalStackPanelWidget), "VerticalStackPanelWidget")]
    [JsonDerivedType(typeof(TextFieldWidget), "TextFieldWidget")]
    public class Widget
    {
        public string Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsVisible { get; set; }

        // Parameterless constructor for JSON deserialization
        public Widget() : this("default_widget_id", 0, 0)
        {
            // Default constructor for serializer, calls main constructor with default values
        }

        public Widget(string id, int x, int y)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }
            Id = id;
            X = x;
            Y = y;
            IsVisible = true; // Default to visible
        }

        public virtual void Draw()
        {
            // Base draw method, can be overridden by derived classes
            // For now, it does nothing, but derived classes can implement their drawing logic.
            // This method is called by UserInterface.Draw()
            if (!IsVisible)
            {
                return;
            }
            // Actual drawing logic would go here or in overrides
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public virtual string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Serialize as the base type Widget to ensure the type discriminator is included
            return JsonSerializer.Serialize<Widget>(this, options);
        }

        public static T FromJson<T>(string json) where T : Widget
        {
            var options = new JsonSerializerOptions { };
            var widget = JsonSerializer.Deserialize<T>(json, options);
            if (widget == null)
            {
                throw new JsonException($"Failed to deserialize {typeof(T).Name} from JSON: Input JSON may be invalid or not match the expected type.");
            }
            return widget;
        }

        // Overload for deserializing when the exact type isn't known at compile time but is expected to be Widget or derived
        public static Widget FromJson(string json)
        {
            var options = new JsonSerializerOptions { };
            var widget = JsonSerializer.Deserialize<Widget>(json, options); // Relies on [JsonDerivedType]
            if (widget == null)
            {
                throw new JsonException("Failed to deserialize Widget from JSON: Input JSON may be invalid or not match the expected type.");
            }
            return widget;
        }
    }
}
