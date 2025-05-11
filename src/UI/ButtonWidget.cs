using System;
using System.Text.Json.Serialization;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics; // For Vector3, Matrix4
using GameFramework.Rendering; // For ShaderHelper

namespace GameFramework.UI
{
    /// <summary>
    /// Represents a clickable button widget in the UI.
    /// </summary>
    public class ButtonWidget : Widget
    {
        private string _text; // Backing field for Text property
        private Vector3 _textColor; // Backing field for TextColor property
        private LabelWidget _label; // Internal LabelWidget for rendering text

        /// <summary>
        /// Gets or sets the text displayed on the button.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                if (_label != null)
                {
                    _label.Text = _text;
                }
                UpdateLabelPosition(); // Update label position when text changes
            }
        }

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event Action? OnClick;

        /// <summary>
        /// Gets or sets the width of the button.
        /// </summary>
        public int Width { get; set; } 

        /// <summary>
        /// Gets or sets the height of the button.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the background color of the button.
        /// </summary>
        public Vector3 BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the text color of the button.
        /// </summary>
        public Vector3 TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                if (_label != null)
                {
                    _label.TextColor = _textColor; // Changed from _label.Color
                }
            }
        }

        // Rendering resources for the button itself (background, border)
        private static int _colorShaderProgram = -1;
        private static int _vao = -1;
        private static int _vbo = -1;
        private static int _projectionMatrixLocation = -1;
        private static int _modelMatrixLocation = -1;
        private static int _objectColorLocation = -1;

        // Screen dimensions - needed for projection matrix
        /// <summary>
        /// Gets or sets the width of the screen. This is used for the projection matrix.
        /// </summary>
        public static int ScreenWidth { get; set; } = 800;
        /// <summary>
        /// Gets or sets the height of the screen. This is used for the projection matrix.
        /// </summary>
        public static int ScreenHeight { get; set; } = 600;
        private static Matrix4 _projectionMatrix;

        // Static constructor to initialize shared rendering resources once
        static ButtonWidget()
        {
            InitializeRendering();
        }

        private static void InitializeRendering(string vertexShaderPath = "src/Shaders/ui_vertex.glsl", string fragmentShaderPath = "src/Shaders/ui_fragment_color.glsl")
        {
            try
            {
                _colorShaderProgram = ShaderHelper.CreateProgram(vertexShaderPath, fragmentShaderPath);
                Console.WriteLine($"ButtonWidget Color Shader Program ID: {_colorShaderProgram}");

                _projectionMatrixLocation = GL.GetUniformLocation(_colorShaderProgram, "projection");
                _modelMatrixLocation = GL.GetUniformLocation(_colorShaderProgram, "model");
                _objectColorLocation = GL.GetUniformLocation(_colorShaderProgram, "objectColor");

                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

                float[] quadVertices = new float[] {
                    0, 0,          // Top-left
                    1, 0,          // Top-right
                    0, 1,          // Bottom-left
                    1, 0,          // Top-right
                    1, 1,          // Bottom-right
                    0, 1           // Bottom-left
                };
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

                GL.EnableVertexAttribArray(0); // Location 0 for aPosition in ui_vertex.glsl
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                UpdateProjectionMatrix();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ButtonWidget rendering: {ex.Message}");
                if (_colorShaderProgram != -1) GL.DeleteProgram(_colorShaderProgram);
                if (_vao != -1) GL.DeleteVertexArray(_vao);
                if (_vbo != -1) GL.DeleteBuffer(_vbo);
                _colorShaderProgram = _vao = _vbo = -1;
            }
        }

        /// <summary>
        /// Updates the projection matrix based on the current screen dimensions.
        /// Also updates the FontRenderer's projection matrix.
        /// </summary>
        public static void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ScreenWidth, ScreenHeight, 0, -1.0f, 1.0f);
            FontRenderer.ScreenWidth = ScreenWidth;
            FontRenderer.ScreenHeight = ScreenHeight;
            FontRenderer.UpdateProjectionMatrix();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonWidget"/> class with default values.
        /// </summary>
        public ButtonWidget() : this(
            "default_button_id", 
            0, 0, 
            "Default Text", 
            100, 30, 
            new Vector3(0.8f, 0.8f, 0.8f),  // Default background color (light gray)
            new Vector3(0.1f, 0.1f, 0.1f)   // Default text color (dark gray)
            ) 
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonWidget"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the widget.</param>
        /// <param name="x">The x-coordinate of the widget's position.</param>
        /// <param name="y">The y-coordinate of the widget's position.</param>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="backgroundColor">The background color of the button. Defaults to light gray if null.</param>
        /// <param name="textColor">The text color of the button. Defaults to dark gray if null.</param>
        [JsonConstructor]
        public ButtonWidget(string id, int x, int y, string text, int width = 100, int height = 30, Vector3? backgroundColor = null, Vector3? textColor = null) : base(id, x, y)
        {
            // Initialize private fields first to satisfy non-nullable requirements
            _text = text ?? string.Empty;
            _textColor = textColor ?? new Vector3(0.1f, 0.1f, 0.1f); // Default dark gray

            Width = width;
            Height = height;
            BackgroundColor = backgroundColor ?? new Vector3(0.8f, 0.8f, 0.8f); // Default light gray
            // TextColor and Text properties are effectively set via the private fields _text and _textColor

            // Create and configure the internal LabelWidget
            // Id from base class is used to ensure unique label ID
            _label = new LabelWidget($"{Id}_label", 0, 0, _text, _textColor); 
            _label.IsVisible = this.IsVisible; // Sync initial visibility

            // Position the label correctly within the button
            // This needs to be called after ButtonWidget's X, Y, Width, Height are set.
            UpdateLabelPosition();
        }

        /// <summary>
        /// Updates the internal label's position based on the button's dimensions and position.
        /// </summary>
        private void UpdateLabelPosition()
        {
            if (_label == null) return;

            // FontRenderer should be initialized by LabelWidget's constructor if needed
            // or ensure FontRenderer is ready before these calls.
            // Assuming FontRenderer is globally accessible and initialized.
            float labelWidth = FontRenderer.GetTextWidth(_label.Text);
            float labelHeight = FontRenderer.GetTextHeight(); // Corrected: GetTextHeight takes no arguments

            // Calculate centered position for the label within the button
            int labelX = X + (int)((Width - labelWidth) / 2);
            int labelY = Y + (int)((Height - labelHeight) / 2);

            _label.SetPosition(labelX, labelY);
        }

        /// <summary>
        /// Sets the position of the widget. Also updates the internal label's position.
        /// </summary>
        /// <param name="x">The new x-coordinate.</param>
        /// <param name="y">The new y-coordinate.</param>
        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            UpdateLabelPosition();
        }

        /// <summary>
        /// Simulates a click on the button, invoking the OnClick event.
        /// </summary>
        public void Click()
        {
            OnClick?.Invoke();
        }

        /// <summary>
        /// Draws the button on the screen.
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            if (!IsVisible) return; // Simplified visibility check

            // Draw background
            if (_colorShaderProgram != -1)
            {
                GL.UseProgram(_colorShaderProgram);
                GL.UniformMatrix4(_projectionMatrixLocation, false, ref _projectionMatrix);

                Matrix4 modelMatrix = Matrix4.CreateScale(Width, Height, 1.0f) * Matrix4.CreateTranslation(X, Y, 0f);
                GL.UniformMatrix4(_modelMatrixLocation, false, ref modelMatrix);

                GL.BindVertexArray(_vao);
                GL.Uniform3(_objectColorLocation, BackgroundColor);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.BindVertexArray(0);
                GL.UseProgram(0);
            }

            // Draw text using LabelWidget
            if (_label != null)
            {
                _label.IsVisible = this.IsVisible; // Ensure visibility is synced
                _label.Draw();
            }
        }

        /// <summary>
        /// Disposes of the rendering resources used by the ButtonWidget class.
        /// </summary>
        public static void DisposeRendering()
        {
            if (_colorShaderProgram != -1) GL.DeleteProgram(_colorShaderProgram);
            if (_vao != -1) GL.DeleteVertexArray(_vao);
            if (_vbo != -1) GL.DeleteBuffer(_vbo);
            _colorShaderProgram = _vao = _vbo = -1;
            FontRenderer.Dispose();
        }
    }
}
