using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics; // Added for Vector4, Matrix4
using GameFramework.Rendering; // Added for ShaderProgram
using OpenTK.Graphics.OpenGL4; // Added for GL calls

namespace GameFramework.UI
{
    [JsonDerivedType(typeof(ButtonWidget), "ButtonWidget")]
    [JsonDerivedType(typeof(CheckboxWidget), "CheckboxWidget")]
    [JsonDerivedType(typeof(PanelWidget), "PanelWidget")]
    [JsonDerivedType(typeof(HorizontalStackPanelWidget), "HorizontalStackPanelWidget")]
    [JsonDerivedType(typeof(VerticalStackPanelWidget), "VerticalStackPanelWidget")]
    [JsonDerivedType(typeof(TextFieldWidget), "TextFieldWidget")]
    [JsonDerivedType(typeof(ScaleWidget), "ScaleWidget")]
    public class Widget
    {
        public string Id { get; set; }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public bool IsVisible { get; set; } = true;

        public AnchorPoint Anchor { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public virtual int WidgetWidth { get; set; }
        public virtual int WidgetHeight { get; set; }

        public virtual Vector4 BackgroundColor { get; set; } = new Vector4(0, 0, 0, 0);

        // New/Modified properties
        public ShaderProgram? Shader { get; set; } // Made nullable
        public float ActualX { get; protected set; }
        public float ActualY { get; protected set; }

        // Shared graphics resources for simple quad rendering
        protected static int SharedVao;
        protected static int SharedVbo;
        protected static bool SharedGraphicsInitialized = false;

        protected static void InitializeSharedGraphics()
        {
            if (SharedGraphicsInitialized) return;

            float[] vertices = {
                // Positions // Texture Coords
                0.0f, 1.0f,  0.0f, 1.0f, // Top-left
                1.0f, 0.0f,  1.0f, 0.0f, // Bottom-right
                0.0f, 0.0f,  0.0f, 0.0f, // Bottom-left

                0.0f, 1.0f,  0.0f, 1.0f, // Top-left
                1.0f, 1.0f,  1.0f, 1.0f, // Top-right
                1.0f, 0.0f,  1.0f, 0.0f  // Bottom-right
            };

            SharedVao = GL.GenVertexArray();
            SharedVbo = GL.GenBuffer();

            GL.BindVertexArray(SharedVao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, SharedVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute (now vec2)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // Texture coord attribute (offset adjusted)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            SharedGraphicsInitialized = true;
        }

        public Widget() : this("default_widget_id", 0, 0)
        {
        }

        public Widget(string id, int x, int y, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }
            Id = id;
            IsVisible = true;

            Anchor = anchor;
            OffsetX = offsetX;
            OffsetY = offsetY;

            if (Anchor == AnchorPoint.Manual)
            {
                X = x;
                Y = y;
            }
            else
            {
                X = x;
                Y = y;
            }

            InitializeSharedGraphics(); // Ensure shared VAO/VBO are ready
        }

        // Modified signature and logic
        public virtual void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            float calculatedX = 0;
            float calculatedY = 0;

            if (Anchor == AnchorPoint.Manual)
            {
                calculatedX = X;
                calculatedY = Y;
            }
            else
            {
                switch (Anchor)
                {
                    case AnchorPoint.TopLeft:
                        calculatedX = 0;
                        calculatedY = 0;
                        break;
                    case AnchorPoint.TopCenter:
                        calculatedX = containerWidth / 2 - WidgetWidth / 2;
                        calculatedY = 0;
                        break;
                    case AnchorPoint.TopRight:
                        calculatedX = containerWidth - WidgetWidth;
                        calculatedY = 0;
                        break;
                    case AnchorPoint.MiddleLeft:
                        calculatedX = 0;
                        calculatedY = containerHeight / 2 - WidgetHeight / 2;
                        break;
                    case AnchorPoint.MiddleCenter:
                        calculatedX = containerWidth / 2 - WidgetWidth / 2;
                        calculatedY = containerHeight / 2 - WidgetHeight / 2;
                        break;
                    case AnchorPoint.MiddleRight:
                        calculatedX = containerWidth - WidgetWidth;
                        calculatedY = containerHeight / 2 - WidgetHeight / 2;
                        break;
                    case AnchorPoint.BottomLeft:
                        calculatedX = 0;
                        calculatedY = containerHeight - WidgetHeight;
                        break;
                    case AnchorPoint.BottomCenter:
                        calculatedX = containerWidth / 2 - WidgetWidth / 2;
                        calculatedY = containerHeight - WidgetHeight;
                        break;
                    case AnchorPoint.BottomRight:
                        calculatedX = containerWidth - WidgetWidth;
                        calculatedY = containerHeight - WidgetHeight;
                        break;
                }
                calculatedX += OffsetX;
                calculatedY += OffsetY;
            }

            this.ActualX = parentActualX + calculatedX;
            this.ActualY = parentActualY + calculatedY;
        }

        // Modified signature to include projectionMatrix
        public virtual void Draw(float elapsedTime, Matrix4 projectionMatrix)
        {
            if (!IsVisible) return;

            if (Shader != null && BackgroundColor.W > 0)
            {
                Shader.Use();
                // Set projection matrix uniform
                Shader.SetUniform("projection", projectionMatrix);
                
                Matrix4 modelMatrix = Matrix4.CreateScale(WidgetWidth, WidgetHeight, 1.0f) * Matrix4.CreateTranslation(ActualX, ActualY, 0);
                Shader.SetUniform("model", modelMatrix);
                
                // Correct uniform name to "objectColor" and use Vector3
                Shader.SetUniform("objectColor", new Vector3(BackgroundColor.X, BackgroundColor.Y, BackgroundColor.Z));

                GL.BindVertexArray(SharedVao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.BindVertexArray(0);
            }
            else
            {
                if (Shader == null) Console.WriteLine($"[Widget {Id}] Draw: Not drawing background because Shader is null.");
                else if (BackgroundColor.W <= 0) Console.WriteLine($"[Widget {Id}] Draw: Not drawing background because BackgroundColor.W is {BackgroundColor.W}.");
            }
        }

        public virtual void Update(float deltaTime)
        {
            // Base widgets may not need to do anything on update.
            // Derived widgets can override this method to implement specific update logic.
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public virtual bool OnMouseDown(float x, float y, MouseButton mouseButton)
        {
            return false;
        }

        // Added new virtual methods
        public virtual bool OnMouseUp(float x, float y, MouseButton mouseButton)
        {
            return false;
        }

        public virtual void OnMouseMove(float x, float y, float dx, float dy)
        {
            // dx, dy are deltas
        }

        // Modified HitTest to use ActualX/Y and added detailed logging
        public virtual bool HitTest(float screenX, float screenY)
        {
            if (!IsVisible)
            {
                Console.WriteLine($"[Widget {Id}] HitTest: Failed (Not Visible). Mouse: ({screenX},{screenY})"); // Debug
                return false;
            }
            bool hit = screenX >= ActualX && screenX <= ActualX + WidgetWidth &&
                       screenY >= ActualY && screenY <= ActualY + WidgetHeight;
            Console.WriteLine($"[Widget {Id}] HitTest: Result={hit}. Mouse: ({screenX},{screenY}). WidgetRect (X,Y,W,H): ({ActualX}, {ActualY}, {WidgetWidth}, {WidgetHeight})"); // Debug
            return hit;
        }

        public virtual void SetPosition(int x, int y)
        {
            Anchor = AnchorPoint.Manual;
            X = x;
            Y = y;
            OffsetX = 0;
            OffsetY = 0;
        }

        public virtual string ToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize<Widget>(this, options);
        }

        public static T FromJson<T>(string json) where T : Widget
        {
            var options = new JsonSerializerOptions { };
            var widget = JsonSerializer.Deserialize<T>(json, options);
            if (widget == null)
            {
                throw new JsonException($"Failed to deserialize {typeof(T).Name} from JSON: Input JSON may be invalid or not match the expected type.");
            }
            return widget;
        }

        public static Widget FromJson(string json)
        {
            var options = new JsonSerializerOptions { };
            var widget = JsonSerializer.Deserialize<Widget>(json, options);
            if (widget == null)
            {
                throw new JsonException("Failed to deserialize Widget from JSON: Input JSON may be invalid or not match the expected type.");
            }
            return widget;
        }
    }
}
