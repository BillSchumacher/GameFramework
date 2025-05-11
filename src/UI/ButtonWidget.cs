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

        public override void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            // Update button's own position
            base.UpdateActualPosition(parentActualX, parentActualY, containerWidth, containerHeight);
            Console.WriteLine($"[ButtonWidget {Id}] UpdateActualPosition: ActualX={ActualX}, ActualY={ActualY}, WidgetWidth={WidgetWidth}, WidgetHeight={WidgetHeight}, parentActualX={parentActualX}, parentActualY={parentActualY}, containerWidth={containerWidth}, containerHeight={containerHeight}"); // Debug

            // Update label's position relative to this button
            if (_label != null)
            {
                _label.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
            }
        }
        
        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            if (!IsVisible) return;

            // Draw button background using base class implementation
            base.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix

            // Draw the label
            if (_label != null)
            {
                _label.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
                _label.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix
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
