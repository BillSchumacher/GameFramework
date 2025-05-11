using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameFramework.UI
{
    [JsonDerivedType(typeof(ButtonWidget), "ButtonWidget")]
    [JsonDerivedType(typeof(CheckboxWidget), "CheckboxWidget")]
    [JsonDerivedType(typeof(PanelWidget), "PanelWidget")]
    [JsonDerivedType(typeof(HorizontalStackPanelWidget), "HorizontalStackPanelWidget")]
    [JsonDerivedType(typeof(VerticalStackPanelWidget), "VerticalStackPanelWidget")]
    [JsonDerivedType(typeof(TextFieldWidget), "TextFieldWidget")]
    [JsonDerivedType(typeof(ScaleWidget), "ScaleWidget")]
    public class Widget
    {
        public string Id { get; set; }
        public int X { get; protected set; } // Made protected set
        public int Y { get; protected set; } // Made protected set
        public bool IsVisible { get; set; } = true; // Added IsVisible property

        public AnchorPoint Anchor { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        // Dimensions of the widget, to be set by derived classes or after content is known
        public virtual int WidgetWidth { get; protected set; }
        public virtual int WidgetHeight { get; protected set; }

        // Parameterless constructor for JSON deserialization
        public Widget() : this("default_widget_id", 0, 0)
        {
            // Default constructor for serializer, calls main constructor with default values
        }

        // Constructor with optional anchor and offset parameters
        public Widget(string id, int x, int y, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }
            Id = id;
            IsVisible = true; // Default to visible

            Anchor = anchor;
            OffsetX = offsetX;
            OffsetY = offsetY;

            if (Anchor == AnchorPoint.Manual)
            {
                X = x;
                Y = y;
            }
            else
            {
                // X and Y will be calculated by UpdateActualPosition
                // For now, can initialize to 0 or the provided x,y as a fallback if UpdateActualPosition isn't called immediately
                X = x; // Or 0
                Y = y; // Or 0
            }
            // WidgetWidth and WidgetHeight should be set by derived classes
        }

        public virtual void UpdateActualPosition(int parentWidth, int parentHeight)
        {
            if (Anchor == AnchorPoint.Manual)
            {
                // Position is already set directly or via SetPosition
                return;
            }

            int newX = X;
            int newY = Y;

            // Calculate base position based on anchor
            switch (Anchor)
            {
                case AnchorPoint.TopLeft:
                    newX = 0;
                    newY = 0;
                    break;
                case AnchorPoint.TopCenter:
                    newX = parentWidth / 2 - WidgetWidth / 2;
                    newY = 0;
                    break;
                case AnchorPoint.TopRight:
                    newX = parentWidth - WidgetWidth;
                    newY = 0;
                    break;
                case AnchorPoint.MiddleLeft:
                    newX = 0;
                    newY = parentHeight / 2 - WidgetHeight / 2;
                    break;
                case AnchorPoint.MiddleCenter:
                    newX = parentWidth / 2 - WidgetWidth / 2;
                    newY = parentHeight / 2 - WidgetHeight / 2;
                    break;
                case AnchorPoint.MiddleRight:
                    newX = parentWidth - WidgetWidth;
                    newY = parentHeight / 2 - WidgetHeight / 2;
                    break;
                case AnchorPoint.BottomLeft:
                    newX = 0;
                    newY = parentHeight - WidgetHeight;
                    break;
                case AnchorPoint.BottomCenter:
                    newX = parentWidth / 2 - WidgetWidth / 2;
                    newY = parentHeight - WidgetHeight;
                    break;
                case AnchorPoint.BottomRight:
                    newX = parentWidth - WidgetWidth;
                    newY = parentHeight - WidgetHeight;
                    break;
            }

            // Apply offset
            X = newX + OffsetX;
            Y = newY + OffsetY;
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

        // Changed to return bool to indicate if the event was handled
        public virtual bool OnMouseDown(float x, float y, MouseButton mouseButton)
        {
            // Base implementation does nothing and does not handle the event
            return false;
        }

        public virtual void SetPosition(int x, int y)
        {
            Anchor = AnchorPoint.Manual;
            X = x;
            Y = y;
            OffsetX = 0; // Reset offsets when position is set manually
            OffsetY = 0;
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
