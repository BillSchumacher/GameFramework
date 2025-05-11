using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GameFramework.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework; // Ensure this is present
using System.IO; // Added for Path operations
using System.Reflection; // Added for Assembly operations
using System.Collections.Generic; // Added for List
using System.Linq; // Added for Linq operations

namespace GameFramework.UI
{
    public class ButtonWidget : Widget
    {
        private string _text;
        private Vector3 _textColor;
        private LabelWidget _label;
        private string _fontName;
        private int _fontSize;

        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                if (_label != null)
                {
                    _label.Text = _text;
                    // Consider triggering a UI update if text change affects layout significantly
                }
            }
        }

        public event Action? OnClick;

        // Width and Height properties now correctly use WidgetWidth and WidgetHeight from the base class
        public int Width
        {
            get => WidgetWidth;
            set { WidgetWidth = value; /* Potentially trigger UI update */ }
        }

        public int Height
        {
            get => WidgetHeight;
            set { WidgetHeight = value; /* Potentially trigger UI update */ }
        }

        public override Vector4 BackgroundColor { get; set; }

        public Vector3 TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                if (_label != null)
                {
                    _label.TextColor = _textColor;
                }
            }
        }

        private static int _colorShaderProgram = -1;
        private static int _vao = -1;
        private static int _vbo = -1;
        private static int _projectionMatrixLocation = -1;
        private static int _modelMatrixLocation = -1;
        private static int _objectColorLocation = -1;

        public static int ScreenWidth { get; set; } = 800;
        public static int ScreenHeight { get; set; } = 600;
        private static Matrix4 _projectionMatrix;

        static ButtonWidget()
        {
            InitializeRendering();
        }

        public ButtonWidget(
            string id,
            string fontName,
            int fontSize,
            int width,
            int height,
            string text,
            AnchorPoint anchor = AnchorPoint.Manual, // Default to manual if not specified
            int offsetX = 0, // Corrected type to int to match Widget constructor
            int offsetY = 0) // Corrected type to int to match Widget constructor
            : base(id, 0, 0, anchor, offsetX, offsetY) // Pass relevant parameters to Widget base. Initial X,Y are placeholders if anchored.
        {
            WidgetWidth = width;   // Set the button's own width
            WidgetHeight = height; // Set the button's own height
            _fontName = fontName;
            _fontSize = fontSize;
            _text = text ?? string.Empty;
            BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f); // Default background (now Vector4 with alpha)
            TextColor = new Vector3(0.0f, 0.0f, 0.0f);     // Default text color

            // Label is centered within the button, with no further offset from that center point.
            // Its width/height are determined by its content.
            // The LabelWidget ID is derived from the button's ID for uniqueness.
            _label = new LabelWidget(_fontName, _fontSize, _text, _textColor, AnchorPoint.MiddleCenter, 0f, 0f, id + "_label");
            _label.Text = _text; // Ensure text is set
            _label.TextColor = _textColor; // Ensure color is set
        }

        private static string? ResolveShaderPath(string shaderFileName)
        {
            string? assemblyDir = Path.GetDirectoryName(typeof(ButtonWidget).Assembly.Location);
            if (string.IsNullOrEmpty(assemblyDir))
            {
                assemblyDir = AppContext.BaseDirectory; // Fallback
            }

            var searchBaseDirs = new List<string>();
            if (!string.IsNullOrEmpty(assemblyDir)) {
                searchBaseDirs.Add(assemblyDir);
            }
            searchBaseDirs.Add(AppContext.BaseDirectory); // Add execution directory of the app

            // Define relative paths to the Shaders folder
            string[] shaderRelativeFolders = {
                Path.Combine("src", "Shaders"), // As per GameFramework.csproj
                "Shaders"                       // A common alternative
            };

            List<string> attemptedPaths = new List<string>();

            foreach (var baseDir in searchBaseDirs.Distinct().Where(d => !string.IsNullOrEmpty(d)))
            {
                foreach (var relativeFolder in shaderRelativeFolders)
                {
                    string potentialPath = Path.Combine(baseDir!, relativeFolder, shaderFileName);
                    attemptedPaths.Add(Path.GetFullPath(potentialPath));
                    if (File.Exists(potentialPath))
                    {
                        return Path.GetFullPath(potentialPath);
                    }
                }
            }
            Console.WriteLine($"Shader file '{shaderFileName}' not found. Searched in paths derived from assembly and base directories:\n{string.Join("\n", attemptedPaths.Distinct())}");
            return null;
        }

        private static void InitializeRendering(string vertexShaderName = "ui_vertex.glsl", string fragmentShaderName = "ui_fragment_color.glsl")
        {
            try
            {
                string? resolvedVertexShaderPath = ResolveShaderPath(vertexShaderName);
                string? resolvedFragmentShaderPath = ResolveShaderPath(fragmentShaderName);

                if (string.IsNullOrEmpty(resolvedVertexShaderPath) || string.IsNullOrEmpty(resolvedFragmentShaderPath))
                {
                    // Errors already logged by ResolveShaderPath if a shader is not found.
                    // We can throw a specific exception here or let the ShaderHelper.CreateProgram handle it if paths are null/empty.
                    // For now, let's ensure ShaderHelper.CreateProgram gets valid paths or it will throw.
                    if (string.IsNullOrEmpty(resolvedVertexShaderPath))
                        throw new FileNotFoundException($"Vertex shader '{vertexShaderName}' could not be resolved.", vertexShaderName);
                    if (string.IsNullOrEmpty(resolvedFragmentShaderPath))
                        throw new FileNotFoundException($"Fragment shader '{fragmentShaderName}' could not be resolved.", fragmentShaderName);
                }

                _colorShaderProgram = ShaderHelper.CreateProgram(resolvedVertexShaderPath!, resolvedFragmentShaderPath!);
                _projectionMatrixLocation = GL.GetUniformLocation(_colorShaderProgram, "projection");
                _modelMatrixLocation = GL.GetUniformLocation(_colorShaderProgram, "model");
                _objectColorLocation = GL.GetUniformLocation(_colorShaderProgram, "objectColor");

                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();
                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                float[] quadVertices = { 0,0,  1,0,  0,1,  1,0,  1,1,  0,1 };
                GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                UpdateProjectionMatrix();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ButtonWidget rendering: {ex.Message}");
                if (_colorShaderProgram > 0) GL.DeleteProgram(_colorShaderProgram); _colorShaderProgram = -1;
                if (_vao > 0) GL.DeleteVertexArray(_vao); _vao = -1;
                if (_vbo > 0) GL.DeleteBuffer(_vbo); _vbo = -1;
            }
        }

        public static void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ScreenWidth, ScreenHeight, 0, -1.0f, 1.0f);
            FontRenderer.ScreenWidth = ScreenWidth;
            FontRenderer.ScreenHeight = ScreenHeight;
            FontRenderer.UpdateProjectionMatrix();
        }

        public override void UpdateActualPosition(int parentWidth, int parentHeight)
        {
            // 1. Calculate and set this button's own screen position (X, Y in Widget base)
            base.UpdateActualPosition(parentWidth, parentHeight);

            if (_label != null)
            {
                // 2. The label is a child of this button. Its position is relative to this button.
                //    Call the label's UpdateActualPosition, passing the button's dimensions as the "parent" container for the label.
                //    This will calculate _label.X and _label.Y relative to the button's top-left corner.
                _label.UpdateActualPosition(this.WidgetWidth, this.WidgetHeight);
            }
        }

        // Correctly override OnMouseDown from Widget base class and return bool
        public override bool OnMouseDown(float x, float y, MouseButton mouseButton)
        {
            // The UserInterface class is expected to call this only if HitTest was successful for this widget.
            // No need to call base.OnMouseDown(x,y,mouseButton) if base implementation is empty or not desired.
            if (mouseButton == MouseButton.Left)
            {
                OnClick?.Invoke();
                return true; // Event handled
            }
            return false; // Event not handled
        }

        // Modified Draw method to accept elapsedTime and pass it to the label
        public void Draw(float elapsedTime) // Changed from Draw() to Draw(float elapsedTime)
        {
            if (!IsVisible) return;
            if (_colorShaderProgram <= 0 || _vao <= 0 || _vbo <= 0)
            {
                Console.WriteLine("ButtonWidget rendering resources not properly initialized. Attempting to reinitialize.");
                InitializeRendering();
                if (_colorShaderProgram <= 0)
                {
                    Console.WriteLine("ButtonWidget reinitialization failed. Skipping draw call.");
                    return;
                }
            }

            // Draw Button Background
            GL.UseProgram(_colorShaderProgram);
            Matrix4 model = Matrix4.CreateScale(this.WidgetWidth, this.WidgetHeight, 1.0f) * Matrix4.CreateTranslation(this.X, this.Y, 0.0f);
            GL.UniformMatrix4(_projectionMatrixLocation, false, ref _projectionMatrix);
            GL.UniformMatrix4(_modelMatrixLocation, false, ref model);
            GL.Uniform3(_objectColorLocation, BackgroundColor.Xyz); // Use only the RGB components

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);

            // Draw Label
            if (_label != null)
            {
                _label.Draw(this.X, this.Y, elapsedTime); // Pass elapsedTime here
            }

            GL.UseProgram(0); 
        }

        // Override the parameterless Draw to call the one with elapsedTime
        public override void Draw()
        {
            Draw(0.0f); 
        }
    }
}
