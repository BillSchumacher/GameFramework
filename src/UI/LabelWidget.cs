using OpenTK.Mathematics;
using System.Text.Json.Serialization;

namespace GameFramework.UI
{
    /// <summary>
    /// Represents a simple text label widget in the UI.
    /// </summary>
    public class LabelWidget : Widget
    {
        /// <summary>
        /// Gets or sets the text displayed by the label.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the text color of the label.
        /// </summary>
        public Vector3 TextColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelWidget"/> class with default values.
        /// </summary>
        public LabelWidget() : this(
            "default_label_id",
            0, 0,
            "Default Text",
            new Vector3(1.0f, 1.0f, 1.0f) // Default text color (white)
            )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelWidget"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the widget.</param>
        /// <param name="x">The x-coordinate of the widget's position.</param>
        /// <param name="y">The y-coordinate of the widget's position.</param>
        /// <param name="text">The text to display on the label.</param>
        /// <param name="textColor">The text color of the label. Defaults to white if null.</param>
        [JsonConstructor]
        public LabelWidget(string id, int x, int y, string text, Vector3? textColor = null) : base(id, x, y)
        {
            Text = text ?? string.Empty;
            TextColor = textColor ?? new Vector3(1.0f, 1.0f, 1.0f); // Default white
        }

        /// <summary>
        /// Draws the label on the screen using FontRenderer.
        /// </summary>
        public override void Draw()
        {
            base.Draw(); // Call base.Draw if it has any common drawing logic or visibility checks
            if (!IsVisible || string.IsNullOrEmpty(Text))
            {
                return;
            }

            FontRenderer.SetColor(TextColor.X, TextColor.Y, TextColor.Z);
            FontRenderer.DrawText(Text, X, Y);
        }

        // No specific rendering resources to dispose for LabelWidget itself,
        // as FontRenderer manages its own resources.
        // If LabelWidget had its own VAO/VBO/Shader, it would need a DisposeRendering method.
    }
}
