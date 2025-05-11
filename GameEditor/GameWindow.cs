using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics; // Added for Matrix4
using GameFramework.UI; // Added for FontRenderer and LabelWidget
using GameFramework.Rendering; // Added for ShaderHelper (if FontRenderer relies on it being public, though likely internal)
using System;
using System.IO; // Added for Path.Combine and AppContext.BaseDirectory

namespace GameEditor
{
    public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
    {
        private const string FontAssetPath = "Assets/ARIAL.TTF"; // Relative to application base
        private const float FontSize = 16f;
        private string? _resolvedFontPath; // Made nullable

        // Declare LabelWidgets for dynamic text
        private LabelWidget? _helloWorldLabel; // Made nullable
        private LabelWidget? _fpsLabel;      // Made nullable

        public GameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Resolve font path
            _resolvedFontPath = System.IO.Path.Combine(AppContext.BaseDirectory, FontAssetPath);
            if (!File.Exists(_resolvedFontPath))
            {
                Console.WriteLine($"FATAL ERROR: Font file not found at {_resolvedFontPath}. Ensure ARIAL.TTF is in the Assets folder and copied to output.");
                // Optionally, throw an exception or close the window if the font is critical
                // For now, we'll let FontRenderer.Initialize handle the File.Exists check and throw if it also can't find it.
            }

            // Set screen size for FontRenderer's projection matrix
            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;

            // Initialize FontRenderer
            try
            {
                FontRenderer.Initialize(_resolvedFontPath, FontSize);
                FontRenderer.UpdateProjectionMatrix();

                // Initialize LabelWidgets
                _helloWorldLabel = new LabelWidget("helloLabel", 10, 10, "Hello World! Test 123 - GameEditor", new Vector3(1.0f, 1.0f, 1.0f));
                _fpsLabel = new LabelWidget("fpsLabel", 10, 30, "FPS: 0", new Vector3(1.0f, 1.0f, 1.0f));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing FontRenderer: {ex.Message}");
                // Handle error, perhaps close window or show an error message
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Update FPS label text
            if (_fpsLabel != null)
            {
                _fpsLabel.Text = $"FPS: {1f / e.Time:F0}";
            }

            // Draw the labels
            _helloWorldLabel?.Draw(); // Use null-conditional operator
            _fpsLabel?.Draw();        // Use null-conditional operator

            // TODO: Add editor rendering logic here

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y); // Use ClientSize here

            // Update FontRenderer's projection matrix on resize
            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;
            FontRenderer.UpdateProjectionMatrix();
        }

        protected override void OnUnload()
        {
            FontRenderer.Dispose(); // Clean up FontRenderer resources
            base.OnUnload();
        }
    }
}
