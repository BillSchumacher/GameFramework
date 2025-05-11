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
using System.Collections.Generic; // Added for List<Vector3>

namespace GameEditor
{
    public class GameWindow : OpenTK.Windowing.Desktop.GameWindow
    {
        private const string FontNameForUI = "ARIAL.TTF"; // Using the constant for font name
        private const float DefaultFontSize = 16f;
        private string? _resolvedFontPath;

        private UserInterface _userInterface;
        private LabelWidget? _helloWorldLabel;
        private LabelWidget? _bounceEffectLabel;
        private LabelWidget? _randomBounceEffectLabel;
        private LabelWidget? _jitterEffectLabel;
        private LabelWidget? _typewriterEffectLabel;
        private LabelWidget? _rainbowEffectLabel; // Added for character colors
        private LabelWidget? _fpsLabel;
        private ButtonWidget? _sampleButton; // Added for the button
        private ScaleWidget? _horizontalScale;
        private ScaleWidget? _verticalScale;
        private ScaleWidget? _controlledScale;
        private LabelWidget? _controlledScaleLabel;
        private ScaleWidget? _scaleWithChildrenHorizontal;
        private LabelWidget? _childLabelForHorizontalScale;
        private ButtonWidget? _childButtonForHorizontalScale;
        private ScaleWidget? _scaleWithChildrenVertical;
        private PanelWidget? _childPanelForVerticalScale;
        private LabelWidget? _childLabelInPanelForVerticalScale;
        private double _totalElapsedTime = 0.0; // Field to accumulate total elapsed time

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
                    text: "Plain Text Example",
                    fontName: FontNameForUI, 
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10, 
                    offsetY: 10,
                    currentTextEffect: TextEffect.None // Or any specific effect you want as the main one
                );

                _bounceEffectLabel = new LabelWidget(
                    id: "bounceLabel",
                    x: 0, y: 0,
                    text: "Bouncing Text!",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(0.8f, 1.0f, 0.8f), // Light green
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10,
                    offsetY: 40, // Position below the previous label
                    currentTextEffect: TextEffect.Bounce,
                    effectSpeed: 2.0f,
                    effectStrength: 8.0f
                );

                _randomBounceEffectLabel = new LabelWidget(
                    id: "randomBounceLabel",
                    x: 0, y: 0,
                    text: "Random Bounce Madness!",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(0.8f, 0.8f, 1.0f), // Light blue
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10,
                    offsetY: 70, // Position below the previous label
                    currentTextEffect: TextEffect.RandomBounce,
                    effectSpeed: 1.5f,
                    effectStrength: 10.0f
                );

                _jitterEffectLabel = new LabelWidget(
                    id: "jitterLabel",
                    x: 0, y: 0,
                    text: "Jittery Jitter Jitter!",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 0.7f), // Light yellow
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10,
                    offsetY: 100, // Position below the previous label
                    currentTextEffect: TextEffect.Jitter,
                    effectSpeed: 0.0f, // Not used by current Jitter impl.
                    effectStrength: 1.0f // Max pixel displacement
                );

                _typewriterEffectLabel = new LabelWidget(
                    id: "typewriterLabel",
                    x: 0, y: 0,
                    text: "Typewriter effect is typing...",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 0.7f, 0.7f), // Light red
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10,
                    offsetY: 130, // Position below the previous label
                    currentTextEffect: TextEffect.Typewriter,
                    effectSpeed: 8.0f, // Characters per second
                    effectStrength: 0.0f // Not used by Typewriter
                );

                // Rainbow Effect Label (per-character colors)
                string rainbowText = "Rainbow!";
                var rainbowCharColors = new List<Vector3>();
                Vector3[] colors = new Vector3[]
                {
                    new Vector3(1.0f, 0.0f, 0.0f), // Red
                    new Vector3(1.0f, 0.5f, 0.0f), // Orange
                    new Vector3(1.0f, 1.0f, 0.0f), // Yellow
                    new Vector3(0.0f, 1.0f, 0.0f), // Green
                    new Vector3(0.0f, 0.5f, 1.0f), // Blue
                    new Vector3(0.5f, 0.0f, 1.0f), // Indigo
                    new Vector3(1.0f, 0.0f, 1.0f)  // Violet
                };
                for (int i = 0; i < rainbowText.Length; i++)
                {
                    rainbowCharColors.Add(colors[i % colors.Length]);
                }

                _rainbowEffectLabel = new LabelWidget(
                    id: "rainbowLabel",
                    x: 0, y: 0,
                    text: rainbowText,
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f), // Fallback if CharacterColors is null/mismatched
                    anchor: AnchorPoint.TopLeft,
                    offsetX: 10,
                    offsetY: 160, // Position below the typewriter label
                    currentTextEffect: TextEffect.None, // Or any other effect combined with rainbow
                    characterColors: rainbowCharColors
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
                    if (_controlledScale != null)
                    {
                        _controlledScale.CurrentValue += 10;
                    }
                };

                // Horizontal ScaleWidget
                _horizontalScale = new ScaleWidget(
                    id: "horizontalScale",
                    minValue: 0,
                    maxValue: 100,
                    initialValue: 25,
                    orientation: GameFramework.UI.Orientation.Horizontal,
                    x: 0, y: 0, // Anchored
                    width: 200,
                    height: 20,
                    anchor: AnchorPoint.BottomLeft,
                    offsetX: 10,
                    offsetY: -80
                );
                _horizontalScale.OnValueChanged += (value) => Console.WriteLine($"Horizontal Scale Value: {value}");

                // Vertical ScaleWidget
                _verticalScale = new ScaleWidget(
                    id: "verticalScale",
                    minValue: -50,
                    maxValue: 50,
                    initialValue: 0,
                    orientation: GameFramework.UI.Orientation.Vertical,
                    x: 0, y: 0, // Anchored
                    width: 20,
                    height: 150,
                    anchor: AnchorPoint.MiddleRight,
                    offsetX: -50,
                    offsetY: 0
                );
                _verticalScale.OnValueChanged += (value) => Console.WriteLine($"Vertical Scale Value: {value}");

                // Controlled ScaleWidget and its Label
                _controlledScaleLabel = new LabelWidget(
                    id: "controlledScaleLabel",
                    x: 0, y: 0, // Anchored
                    text: "Controlled Scale: 50",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.BottomLeft,
                    offsetX: 10,
                    offsetY: -140 // Position above the horizontal scale
                );

                _controlledScale = new ScaleWidget(
                    id: "controlledScale",
                    minValue: 0,
                    maxValue: 200,
                    initialValue: 50,
                    orientation: GameFramework.UI.Orientation.Horizontal,
                    x: 0, y: 0, // Anchored
                    width: 200,
                    height: 20,
                    anchor: AnchorPoint.BottomLeft,
                    offsetX: 10,
                    offsetY: -110 // Position below the label, above horizontal scale
                );
                _controlledScale.OnValueChanged += (value) => {
                    if (_controlledScaleLabel != null)
                    {
                        _controlledScaleLabel.Text = $"Controlled Scale: {value:F0}";
                        _controlledScaleLabel.UpdateActualPosition(ClientSize.X, ClientSize.Y); // Update label due to potential text width change
                    }
                    Console.WriteLine($"Controlled Scale Value: {value}");
                };

                // Horizontal ScaleWidget with Children
                _scaleWithChildrenHorizontal = new ScaleWidget(
                    id: "scaleWithChildrenHorizontal",
                    minValue: 0,
                    maxValue: 100,
                    initialValue: 50,
                    orientation: GameFramework.UI.Orientation.Horizontal,
                    x: 0, y: 0, // Anchored
                    width: 250,
                    height: 80, // Made taller to accommodate children
                    anchor: AnchorPoint.MiddleLeft,
                    offsetX: 20,
                    offsetY: -50 // Adjusted Y to be above bottom elements
                );
                _scaleWithChildrenHorizontal.BackgroundColor = new Vector4(0.3f, 0.3f, 0.4f, 1.0f); // Slightly different background

                _childLabelForHorizontalScale = new LabelWidget(
                    id: "childLabelH",
                    x: 10, y: 10, // Relative to parent ScaleWidget
                    text: "Child Lbl",
                    fontName: FontNameForUI,
                    fontSize: (int)(DefaultFontSize * 0.8), // Smaller font
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.TopLeft // Anchored within parent
                );

                _childButtonForHorizontalScale = new ButtonWidget(
                    id: "childButtonH",
                    fontName: FontNameForUI,
                    fontSize: (int)(DefaultFontSize * 0.8),
                    width: 80, // Smaller button
                    height: 25,
                    text: "Child Btn",
                    anchor: AnchorPoint.BottomRight, // Anchored within parent
                    offsetX: -10,
                    offsetY: -10
                );
                _childButtonForHorizontalScale.OnClick += () => Console.WriteLine("Child Button in Horizontal Scale Clicked!");

                _scaleWithChildrenHorizontal.AddChild(_childLabelForHorizontalScale);
                _scaleWithChildrenHorizontal.AddChild(_childButtonForHorizontalScale);
                _scaleWithChildrenHorizontal.OnValueChanged += (value) => Console.WriteLine($"Horizontal Scale w/ Children Value: {value}");

                // Vertical ScaleWidget with Children
                _scaleWithChildrenVertical = new ScaleWidget(
                    id: "scaleWithChildrenVertical",
                    minValue: 0,
                    maxValue: 100,
                    initialValue: 75,
                    orientation: GameFramework.UI.Orientation.Vertical,
                    x: 0, y: 0, // Anchored
                    width: 80, // Made wider to accommodate child panel
                    height: 250,
                    anchor: AnchorPoint.MiddleRight,
                    offsetX: -100, // Further from the right edge
                    offsetY: 0
                );
                _scaleWithChildrenVertical.BackgroundColor = new Vector4(0.4f, 0.3f, 0.3f, 1.0f); // Slightly different background

                _childPanelForVerticalScale = new PanelWidget(
                    id: "childPanelV",
                    x: 5, y: 5, // Relative to parent ScaleWidget
                    width: 60, // Smaller than parent
                    height: 100,
                    backgroundColor: new Vector4(0.2f, 0.2f, 0.2f, 0.8f),
                    anchor: AnchorPoint.TopCenter, // Anchored within parent
                    offsetX: 0,
                    offsetY: 10
                );

                _childLabelInPanelForVerticalScale = new LabelWidget(
                    id: "childLabelInPanelV",
                    x: 0, y: 0, // Will be centered in panel
                    text: "Nested",
                    fontName: FontNameForUI,
                    fontSize: (int)(DefaultFontSize * 0.7),
                    textColor: new Vector3(0.9f, 0.9f, 0.9f),
                    anchor: AnchorPoint.MiddleCenter // Centered within its parent panel
                );
                _childPanelForVerticalScale.AddChild(_childLabelInPanelForVerticalScale); // Add label to panel

                _scaleWithChildrenVertical.AddChild(_childPanelForVerticalScale); // Add panel to scale widget
                _scaleWithChildrenVertical.OnValueChanged += (value) => Console.WriteLine($"Vertical Scale w/ Children Value: {value}");

                _userInterface.AddWidget(_helloWorldLabel);
                _userInterface.AddWidget(_bounceEffectLabel);
                _userInterface.AddWidget(_randomBounceEffectLabel);
                _userInterface.AddWidget(_jitterEffectLabel);
                _userInterface.AddWidget(_typewriterEffectLabel);
                _userInterface.AddWidget(_rainbowEffectLabel); // Added new label to UI
                _userInterface.AddWidget(_fpsLabel);
                _userInterface.AddWidget(_sampleButton);
                _userInterface.AddWidget(_horizontalScale);
                _userInterface.AddWidget(_verticalScale);
                _userInterface.AddWidget(_controlledScaleLabel);
                _userInterface.AddWidget(_controlledScale);
                _userInterface.AddWidget(_scaleWithChildrenHorizontal);
                _userInterface.AddWidget(_scaleWithChildrenVertical);

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

            _totalElapsedTime += e.Time; // Accumulate total time

            if (_fpsLabel != null)
            {
                _fpsLabel.Text = $"FPS: {1f / e.Time:F0}";
                // Update the label's position after its text (and thus width) has changed.
                _fpsLabel.UpdateActualPosition(ClientSize.X, ClientSize.Y);
            }

            // Pass accumulated time to UserInterface.Draw
            _userInterface.Draw((float)_totalElapsedTime);
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
