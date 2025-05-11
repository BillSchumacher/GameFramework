using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics; // Added for Matrix4
using GameFramework.UI; // Added for FontRenderer and LabelWidget
using GameFramework.Rendering; // Added for ShaderHelper (if FontRenderer relies on it being public, though likely internal)
using System;
using System.IO; // Added for Path.Combine and AppContext.BaseDirectory
using GameFramework; // Added for UserInterface

namespace GameEditor
{
    public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
    {
        private const string FontNameForUI = "ARIAL.TTF"; // Using the constant for font name
        private const float DefaultFontSize = 16f;
        private string? _resolvedFontPath;

        private UserInterface _userInterface;
        private LabelWidget? _helloWorldLabel;
        private LabelWidget? _fpsLabel;
        private ButtonWidget? _sampleButton; // Added for the button

        public GameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _userInterface = new UserInterface();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _resolvedFontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", FontNameForUI);
            if (!File.Exists(_resolvedFontPath))
            {
                Console.WriteLine($"FATAL ERROR: Font file not found at {_resolvedFontPath}.");
                // Consider closing or throwing
                return; // Exit OnLoad if font is missing
            }

            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;

            try
            {
                FontRenderer.Initialize(_resolvedFontPath, DefaultFontSize);
                // No need to call FontRenderer.UpdateProjectionMatrix(); here, as Initialize does it.

                // Initialize LabelWidgets with anchoring
                _helloWorldLabel = new LabelWidget(
                    id: "helloLabel",
                    x: 0, y: 0, // Initial X, Y are less important when anchored
                    text: "Hello World! Anchored!",
                    fontName: FontNameForUI, 
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10, 
                    offsetY: 10
                );

                _fpsLabel = new LabelWidget(
                    id: "fpsLabel",
                    x: 0, y: 0,
                    text: "FPS: 0",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.TopRight,
                    offsetX: -10, // Negative offset to come in from the right
                    offsetY: 10
                );

                // Initialize ButtonWidget with anchoring
                _sampleButton = new ButtonWidget(
                    id: "sampleButton",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    width: 150,
                    height: 40,
                    text: "Click Me!",
                    anchor: AnchorPoint.BottomCenter,
                    offsetX: 0,
                    offsetY: -20 // Negative offset to come up from the bottom
                );

                _sampleButton.OnClick += () => {
                    Console.WriteLine("Button Clicked!");
                };

                _userInterface.AddWidget(_helloWorldLabel);
                _userInterface.AddWidget(_fpsLabel);
                _userInterface.AddWidget(_sampleButton);

                // Update positions for all widgets after adding them
                UpdateAllWidgetPositions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing UI: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void UpdateAllWidgetPositions()
        {
            foreach (var widget in _userInterface.GetWidgets())
            {
                widget.UpdateActualPosition(ClientSize.X, ClientSize.Y);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (_fpsLabel != null)
            {
                _fpsLabel.Text = $"FPS: {1f / e.Time:F0}";
                // Update the label's position after its text (and thus width) has changed.
                _fpsLabel.UpdateActualPosition(ClientSize.X, ClientSize.Y);
            }

            _userInterface.Draw();
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            // Potentially update widget states or handle input for UI elements here
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;
            FontRenderer.UpdateProjectionMatrix();
            ButtonWidget.ScreenWidth = ClientSize.X; // Also update for ButtonWidget's own projection
            ButtonWidget.ScreenHeight = ClientSize.Y;
            ButtonWidget.UpdateProjectionMatrix();

            // Update positions of all widgets on resize
            UpdateAllWidgetPositions();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            // Pass mouse events to the UserInterface to handle widget interactions
            _userInterface.HandleMouseDown(MouseState.X, MouseState.Y, e.Button);
        }

        protected override void OnUnload()
        {
            FontRenderer.Dispose();
            // If ButtonWidget had its own static GL resources that need cleanup, do it here.
            // For now, assuming FontRenderer.Dispose() covers shared font atlas, and ButtonWidget uses that.
            base.OnUnload();
        }
    }
}
