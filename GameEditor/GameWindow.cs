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
using GameFramework.Core; // Added for Profiler
using System.Collections.Generic; // Added for List<Vector3>
using System.Text; // Added for StringBuilder

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
        private LabelWidget? _profilerLabel; // Added for displaying profiler information
        private PanelWidget? _profilerPanel; // Panel for profiler display
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
        private Matrix4 _projectionMatrix; // Added to store the projection matrix

        // New fields for the button scaling example
        private ScaleWidget? _scaleWithButton;
        private ButtonWidget? _scalableButton;

        private Profiler _profiler; // Added Profiler instance
        private bool _logProfilerToConsole = true; // Option to log profiler to console

        public GameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _userInterface = new UserInterface();
            _profiler = new Profiler(); // Initialize Profiler
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _profiler.Start("OnLoad"); // Profile OnLoad
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Initialize projection matrix
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ClientSize.X, ClientSize.Y, 0, -1f, 1f);

            _resolvedFontPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", FontNameForUI);
            if (!File.Exists(_resolvedFontPath))
            {
                Console.WriteLine($"FATAL ERROR: Font file not found at {_resolvedFontPath}.");
                // Consider closing or throwing
                return; // Exit OnLoad if font is missing
            }

            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;
            // FontRenderer.Initialize will set its own projection matrix, 
            // but we also maintain one here for general UI widget rendering.
            FontRenderer.Initialize(_resolvedFontPath, DefaultFontSize);
            FontRenderer.UpdateProjectionMatrix(); // Ensure FontRenderer's matrix is also up-to-date

            try
            {
                _profiler.Start("UIInitialization"); // Profile UI Initialization
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

                // Profiler display panel and label
                _profilerPanel = new PanelWidget(
                    id: "profilerPanel",
                    x: 0, y: 0, // Anchored
                    width: 350, // Adjust width as needed
                    height: 150, // Adjust height as needed
                    backgroundColor: new Vector4(0.1f, 0.1f, 0.1f, 0.7f), // Semi-transparent dark background
                    anchor: AnchorPoint.BottomLeft,
                    offsetX: 10,
                    offsetY: -10 // Position above bottom edge
                );

                _profilerLabel = new LabelWidget(
                    id: "profilerLabel",
                    x: 0, y: 0, // Relative to panel
                    text: "Profiler:",
                    fontName: FontNameForUI,
                    fontSize: (int)(DefaultFontSize * 0.8f), 
                    textColor: new Vector3(0.9f, 0.9f, 0.9f),
                    anchor: AnchorPoint.TopLeft, // Anchor within the panel
                    offsetX: 5, // Padding within the panel
                    offsetY: 5  // Padding within the panel
                );
                _profilerPanel.AddChild(_profilerLabel); // Add label to panel

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
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0, // Changed
                    offsetY: -100 
                );
                _horizontalScale.BackgroundColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f); // Added background color
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
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0,  // Changed
                    offsetY: -75   
                );
                _verticalScale.BackgroundColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f); // Added background color
                _verticalScale.OnValueChanged += (value) => Console.WriteLine($"Vertical Scale Value: {value}");

                // Controlled ScaleWidget and its Label
                _controlledScaleLabel = new LabelWidget(
                    id: "controlledScaleLabel",
                    x: 0, y: 0, // Anchored
                    text: "Controlled Scale: 50",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0, // Changed
                    offsetY: -160 
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
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0, // Changed
                    offsetY: -130  
                );
                _controlledScale.BackgroundColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f); // Added background color
                _controlledScale.OnValueChanged += (value) => {
                    if (_controlledScaleLabel != null)
                    {
                        _controlledScaleLabel.Text = $"Controlled Scale: {value:F0}";
                        _controlledScaleLabel.UpdateActualPosition(0, 0, ClientSize.X, ClientSize.Y); 
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
                    height: 80, 
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0, // Changed
                    offsetY: 0     
                );
                _scaleWithChildrenHorizontal.BackgroundColor = new Vector4(0.3f, 0.3f, 0.4f, 1.0f); 

                _childLabelForHorizontalScale = new LabelWidget(
                    id: "childLabelH",
                    x: 0, // Initial X/Y are less critical when offsetX/Y are used with anchoring
                    y: 0,
                    text: "Child Lbl",
                    fontName: FontNameForUI,
                    fontSize: (int)(DefaultFontSize * 0.8), // Smaller font
                    textColor: new Vector3(1.0f, 1.0f, 1.0f),
                    anchor: AnchorPoint.TopLeft, // Anchored within parent
                    offsetX: 10, // Explicitly use offsetX for positioning
                    offsetY: 10  // Explicitly use offsetY for positioning
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
                    width: 80, 
                    height: 250,
                    anchor: AnchorPoint.MiddleCenter, 
                    offsetX: 0,  // Changed
                    offsetY: 0     
                );
                _scaleWithChildrenVertical.BackgroundColor = new Vector4(0.4f, 0.3f, 0.3f, 1.0f); 

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

                // ScaleWidget with a Button that scales
                _scaleWithButton = new ScaleWidget(
                    id: "scaleWithButton",
                    minValue: 1.0f, // Min scale factor
                    maxValue: 3.0f, // Max scale factor
                    initialValue: 1.0f,
                    orientation: GameFramework.UI.Orientation.Horizontal,
                    x: 0, y: 0, // Anchored
                    width: 200,
                    height: 80, // Make it a bit taller to contain the button comfortably
                    anchor: AnchorPoint.BottomLeft,
                    offsetX: 10,
                    offsetY: -100
                );
                _scaleWithButton.BackgroundColor = new Vector4(0.3f, 0.4f, 0.3f, 1.0f);

                _scalableButton = new ButtonWidget(
                    id: "scalableButton",
                    fontName: FontNameForUI,
                    fontSize: (int)DefaultFontSize,
                    width: 100, // Initial width
                    height: 30, // Initial height
                    text: "Scalable Btn",
                    anchor: AnchorPoint.MiddleCenter, // Center it within the ScaleWidget
                    offsetX: 0,
                    offsetY: 0
                );
                _scalableButton.OnClick += () => Console.WriteLine("Scalable Button Clicked!");

                _scaleWithButton.AddChild(_scalableButton);
                _scaleWithButton.OnValueChanged += (scaleValue) => 
                {
                    if (_scalableButton != null)
                    {
                        Console.WriteLine($"ScaleWithButton Value: {scaleValue}, Button Width: {_scalableButton.WidgetWidth}, Height: {_scalableButton.WidgetHeight}");
                    }
                };

                _userInterface.AddWidget(_helloWorldLabel);
                _userInterface.AddWidget(_bounceEffectLabel);
                _userInterface.AddWidget(_randomBounceEffectLabel);
                _userInterface.AddWidget(_jitterEffectLabel);
                _userInterface.AddWidget(_typewriterEffectLabel);
                _userInterface.AddWidget(_rainbowEffectLabel); // Added new label to UI
                _userInterface.AddWidget(_fpsLabel);
                _userInterface.AddWidget(_profilerPanel); // Add profiler panel to UI
                _userInterface.AddWidget(_sampleButton);
                _userInterface.AddWidget(_horizontalScale);
                _userInterface.AddWidget(_verticalScale);
                _userInterface.AddWidget(_controlledScaleLabel);
                _userInterface.AddWidget(_controlledScale);
                _userInterface.AddWidget(_scaleWithChildrenHorizontal);
                _userInterface.AddWidget(_scaleWithChildrenVertical);
                _userInterface.AddWidget(_scaleWithButton); // Add the new ScaleWidget to the UI

                // Update positions for all widgets after adding them
                UpdateAllWidgetPositions();
                _profiler.Stop("UIInitialization"); // Stop UI Initialization profiling
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing UI: {ex.Message}\n{ex.StackTrace}");
            }
            _profiler.Stop("OnLoad"); // Stop OnLoad profiling
        }

        private void UpdateAllWidgetPositions()
        {
            foreach (var widget in _userInterface.GetWidgets())
            {
                widget.UpdateActualPosition(0, 0, ClientSize.X, ClientSize.Y);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _profiler.Start("OnRenderFrame"); // Profile OnRenderFrame
            base.OnRenderFrame(e);
            _profiler.Start("ClearBuffer");
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Added DepthBufferBit
            _profiler.Stop("ClearBuffer");

            _totalElapsedTime += e.Time; // Accumulate total time

            _profiler.Start("FPSLabelUpdate");
            if (_fpsLabel != null)
            {
                _fpsLabel.Text = $"FPS: {1f / e.Time:F0}";
                // Update the label's position after its text (and thus width) has changed.
                _fpsLabel.UpdateActualPosition(0, 0, ClientSize.X, ClientSize.Y);
            }
            _profiler.Stop("FPSLabelUpdate");

            _profiler.Start("ProfilerUpdate");
            string profilerTitle = "Profiler (ms):";
            if (_logProfilerToConsole)
            {
                Console.Clear(); // Clear console for fresh output
                Console.WriteLine(_profiler.GetFormattedOutput(profilerTitle, true));
            }

            if (_profilerPanel != null && _profilerLabel != null) // Ensure panel and label exist
            {
                _profilerLabel.Text = _profiler.GetFormattedOutput(profilerTitle, false);
                _profilerPanel.UpdateActualPosition(0, 0, ClientSize.X, ClientSize.Y); // Update panel first
                _profilerLabel.UpdateActualPosition(_profilerPanel.ActualX, _profilerPanel.ActualY, _profilerPanel.WidgetWidth, _profilerPanel.WidgetHeight); // Then child label relative to panel
            }
            _profiler.Stop("ProfilerUpdate");

            // Pass accumulated time and projection matrix to UserInterface.Draw
            _profiler.Start("UIDraw");
            _userInterface.Draw((float)_totalElapsedTime, _projectionMatrix);
            _profiler.Stop("UIDraw");

            _profiler.Start("SwapBuffers");
            SwapBuffers();
            _profiler.Stop("SwapBuffers");
            _profiler.Stop("OnRenderFrame"); // Stop OnRenderFrame profiling
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _profiler.Start("OnUpdateFrame"); // Profile OnUpdateFrame
            base.OnUpdateFrame(e);
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            // Potentially update widget states or handle input for UI elements here
            _profiler.Start("UIUpdate");
            _userInterface.Update((float)e.Time); // Assuming UserInterface has an Update method
            _profiler.Stop("UIUpdate");
            _profiler.Stop("OnUpdateFrame"); // Stop OnUpdateFrame profiling
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            // Update the projection matrix for the window
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ClientSize.X, ClientSize.Y, 0, -1f, 1f);

            FontRenderer.ScreenWidth = ClientSize.X;
            FontRenderer.ScreenHeight = ClientSize.Y;
            FontRenderer.UpdateProjectionMatrix(); // Update FontRenderer's internal matrix

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
