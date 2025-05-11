using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GameFramework.Rendering;
using OpenTK.Windowing.GraphicsLibraryFramework; // Ensure this is present
using System.IO; 
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.UI
{
    public class ButtonWidget : Widget
    {
        private string _text;
        private LabelWidget _label;
        private int _originalFontSize;
        private int _originalWidth;
        private int _originalHeight;

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
            }
        }

        public Action? OnClick { get; set; } // Made nullable
        public bool IsPressed { get; private set; } = false;

        public Vector3 TextColor // Proxy to Label's TextColor
        {
            get => _label.TextColor;
            set
            {
                if (_label != null)
                {
                    _label.TextColor = value;
                }
            }
        }

        public ButtonWidget(
            string id,
            string fontName,
            int fontSize,
            int width,
            int height,
            string text,
            AnchorPoint anchor = AnchorPoint.Manual, 
            int offsetX = 0, 
            int offsetY = 0)
            : base(id, 0, 0, anchor, offsetX, offsetY) 
        {
            // Store original dimensions and font size
            _originalWidth = width;
            _originalHeight = height;
            _originalFontSize = fontSize;

            WidgetWidth = width;   
            WidgetHeight = height; 
            _text = text ?? string.Empty;
            
            BackgroundColor = new Vector4(0.7f, 0.7f, 0.7f, 1.0f); 
            // Ensure the button has a shader to draw its background
            try
            {
                // Corrected shader paths
                string vertexShaderPath = Path.Combine(AppContext.BaseDirectory, "Shaders", "ui_vertex.glsl");
                string fragmentShaderPath = Path.Combine(AppContext.BaseDirectory, "Shaders", "ui_fragment_color.glsl");
                Console.WriteLine($"[ButtonWidget {Id}] Constructor: Attempting to load shaders. Vertex: '{vertexShaderPath}', Fragment: '{fragmentShaderPath}'"); // Debug
                if (!File.Exists(vertexShaderPath)) Console.WriteLine($"[ButtonWidget {Id}] Vertex shader not found at: {vertexShaderPath}");
                if (!File.Exists(fragmentShaderPath)) Console.WriteLine($"[ButtonWidget {Id}] Fragment shader not found at: {fragmentShaderPath}");
                
                Shader = new ShaderProgram(vertexShaderPath, fragmentShaderPath);
                Console.WriteLine($"[ButtonWidget {Id}] Constructor: Shader loaded successfully. Shader is null: {(Shader == null)}"); // Debug
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ButtonWidget {Id}] Constructor: Error loading shader: {ex.Message}");
                Console.WriteLine($"[ButtonWidget {Id}] StackTrace: {ex.StackTrace}"); // Debug
                Shader = null; // Ensure shader is null on error
            }

            _label = new LabelWidget(fontName, fontSize, _text, new Vector3(0.0f, 0.0f, 0.0f), AnchorPoint.MiddleCenter, 0, 0, id + "_label");
            _label.Text = _text; 
        }

        public override int WidgetWidth
        {
            get => base.WidgetWidth;
            set
            {
                base.WidgetWidth = value;
                // Font size scaling is now handled in Draw to ensure it happens just before rendering
            }
        }

        public override int WidgetHeight
        {
            get => base.WidgetHeight;
            set
            {
                base.WidgetHeight = value;
                // Font size scaling is now handled in Draw
            }
        }

        public override void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            base.UpdateActualPosition(parentActualX, parentActualY, containerWidth, containerHeight);
            // The label's position is updated in Draw, after potential font size changes affect its dimensions
        }
        
        public override void Draw(float elapsedTime, Matrix4 projectionMatrix)
        {
            if (!IsVisible) return;

            base.Draw(elapsedTime, projectionMatrix);

            if (_label != null)
            {
                if (_originalWidth > 0 && _originalFontSize > 0) // Ensure original dimensions are valid
                {
                    float scaleFactor = 1.0f;
                    if (_originalWidth != 0) // Avoid division by zero if original width was 0
                    {
                        scaleFactor = (float)base.WidgetWidth / _originalWidth;
                    }
                    
                    int newFontSize = (int)(_originalFontSize * scaleFactor);
                    if (newFontSize <= 0) newFontSize = 1; // Prevent zero or negative font size

                    if (_label.FontSize != newFontSize)
                    {
                        _label.FontSize = newFontSize;
                    }
                }
                // Update label's position *after* font size might have changed, as this affects its own width/height for centering
                _label.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
                _label.Draw(elapsedTime, projectionMatrix);
            }
        }

        public override bool OnMouseDown(float mouseX, float mouseY, MouseButton mouseButton)
        {
            if (HitTest(mouseX, mouseY) && mouseButton == MouseButton.Left)
            {
                IsPressed = true;
                return true; 
            }
            return false;
        }

        public override bool OnMouseUp(float mouseX, float mouseY, MouseButton mouseButton)
        {
            if (IsPressed && mouseButton == MouseButton.Left)
            {
                IsPressed = false;
                if (HitTest(mouseX, mouseY)) 
                {
                    OnClick?.Invoke();
                    return true; 
                }
            }
            return false;
        }
    }
}
