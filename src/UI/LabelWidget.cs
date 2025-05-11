using OpenTK.Mathematics;
using System.Text.Json.Serialization;
using System.Collections.Generic; // Added for List<Vector3>

namespace GameFramework.UI
{
    /// <summary>
    /// Represents a simple text label widget in the UI.
    /// </summary>
    public class LabelWidget : Widget
    {
        private string _text = string.Empty;

        /// <summary>
        /// Gets or sets the text displayed by the label.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the text color of the label.
        /// </summary>
        public Vector3 TextColor { get; set; }

        /// <summary>
        /// Gets or sets the name of the font used by the label.
        /// </summary>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the size of the font used by the label.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Gets or sets the visual effect applied to the text.
        /// </summary>
        public TextEffect CurrentTextEffect { get; set; } = TextEffect.None;

        /// <summary>
        /// Gets or sets the speed of the text effect (e.g., cycles per second for bounce).
        /// </summary>
        public float EffectSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the strength of the text effect (e.g., max bounce height in pixels).
        /// </summary>
        public float EffectStrength { get; set; } = 5.0f;

        /// <summary>
        /// Gets or sets the list of colors for individual characters. 
        /// If null or if the count does not match the text length, the global TextColor is used.
        /// </summary>
        public List<Vector3>? CharacterColors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelWidget"/> class with default values.
        /// </summary>
        public LabelWidget() : this(
            "default_label_id",
            0, 0, // x, y
            "Default Text",
            "default_font.ttf", // Example: Expects a resolvable path or key
            16, // fontSize
            new Vector3(1.0f, 1.0f, 1.0f), // Default text color (white)
            AnchorPoint.Manual, 0, 0,
            TextEffect.None, 1.0f, 5.0f, // Default effect parameters
            null // characterColors
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelWidget"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the widget.</param>
        /// <param name="x">The x-coordinate of the widget's initial position (if anchor is Manual).</param>
        /// <param name="y">The y-coordinate of the widget's initial position (if anchor is Manual).</param>
        /// <param name="text">The text to display on the label.</param>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="textColor">The text color of the label. Defaults to white if null.</param>
        /// <param name="anchor">The anchor point for the widget's position.</param>
        /// <param name="offsetX">The X offset from the anchor point.</param>
        /// <param name="offsetY">The Y offset from the anchor point.</param>
        /// <param name="currentTextEffect">The visual effect applied to the text.</param>
        /// <param name="effectSpeed">The speed of the text effect.</param>
        /// <param name="effectStrength">The strength of the text effect.</param>
        /// <param name="characterColors">The list of colors for individual characters.</param>
        [JsonConstructor]
        public LabelWidget(string id, int x, int y, string text, string fontName, int fontSize, Vector3? textColor = null, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0, TextEffect currentTextEffect = TextEffect.None, float effectSpeed = 1.0f, float effectStrength = 5.0f, List<Vector3>? characterColors = null)
            : base(id, x, y, anchor, offsetX, offsetY)
        {
            Text = text; // Use property to ensure backing field is set
            FontName = fontName;
            FontSize = fontSize;
            TextColor = textColor ?? new Vector3(1.0f, 1.0f, 1.0f); // Default white
            CurrentTextEffect = currentTextEffect;
            EffectSpeed = effectSpeed;
            EffectStrength = effectStrength;
            CharacterColors = characterColors;
        }
        
        // Simplified constructor for internal use, e.g., by ButtonWidget
        // Assumes X, Y will be determined by anchoring within a parent
        public LabelWidget(string fontName, int fontSize, string text, Vector3? textColor = null, AnchorPoint anchor = AnchorPoint.MiddleCenter, float offsetX = 0, float offsetY = 0, string id = "generated_label_id", TextEffect currentTextEffect = TextEffect.None, float effectSpeed = 1.0f, float effectStrength = 5.0f, List<Vector3>? characterColors = null)
            : this(id, 0, 0, text, fontName, fontSize, textColor, anchor, (int)offsetX, (int)offsetY, currentTextEffect, effectSpeed, effectStrength, characterColors)
        {
        }


        /// <summary>
        /// Gets the width of the label based on its text content, font name, and font size.
        /// </summary>
        public override int WidgetWidth
        {
            get
            {
                if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(FontName) || FontSize <= 0)
                    return 0;
                // Assumes FontRenderer is already initialized with the correct font (matching this.FontName, this.FontSize)
                return (int)FontRenderer.GetTextWidth(Text);
            }
            set { /* Setter is required for override but not used directly */ }
        }

        /// <summary>
        /// Gets the height of the label based on the current font name and size.
        /// </summary>
        public override int WidgetHeight
        {
            get
            {
                if (string.IsNullOrEmpty(FontName) || FontSize <= 0) // Text content doesn't determine general line height
                    return 0;
                // Assumes FontRenderer is already initialized with the correct font (matching this.FontName, this.FontSize)
                return (int)FontRenderer.GetTextHeight(); // Uses current font for general line height
            }
            set { /* Setter is required for override but not used directly */ }
        }

        /// <summary>
        /// Draws the label on the screen using FontRenderer.
        /// Assumes X and Y are relative to a parent if not drawn at top-level.
        /// </summary>
        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible || string.IsNullOrEmpty(Text)) return;

            // A LabelWidget typically doesn't have its own background, so base.Draw(elapsedTime) is usually not called.
            // If a background were desired, it would be: base.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix

            // Set the color for the FontRenderer before drawing text
            FontRenderer.SetColor(TextColor.X, TextColor.Y, TextColor.Z);

            FontRenderer.DrawText(
                Text,
                ActualX, 
                ActualY, 
                CurrentTextEffect, 
                EffectStrength, 
                EffectSpeed, 
                elapsedTime, 
                CharacterColors 
            );
        }

        /// <summary>
        /// Draws the label on the screen at its relative X, Y offset by parent's absolute coordinates.
        /// This overload might be redundant if UpdateActualPosition and the main Draw(elapsedTime) are used correctly.
        /// </summary>
        /// <param name="parentAbsoluteX">The absolute X coordinate of the parent container.</param>
        /// <param name="parentAbsoluteY">The absolute Y coordinate of the parent container.</param>
        /// <param name="elapsedTime">The elapsed time for animations.</param>
        /// <param name="projectionMatrix">The projection matrix for rendering.</param>
        public void Draw(int parentAbsoluteX, int parentAbsoluteY, float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible || string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(FontName) || FontSize <= 0)
            {
                return;
            }

            float screenX = parentAbsoluteX + X;
            float screenY = parentAbsoluteY + Y;

            // Set the color for the FontRenderer before drawing text
            FontRenderer.SetColor(TextColor.X, TextColor.Y, TextColor.Z);

            FontRenderer.DrawText(
                Text, 
                screenX,
                screenY,
                CurrentTextEffect,
                EffectStrength,
                EffectSpeed,
                elapsedTime,
                CharacterColors
            );
        }
    }
}
