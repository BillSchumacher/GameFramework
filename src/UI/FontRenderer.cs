using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO; // Keep this for File.Exists, etc.
using System.Linq;
using GameFramework.Rendering; // Added for ShaderHelper
using System.Reflection; // Added for path resolution
using System; // Added for Math.Sin
// System.Numerics.Vector2 and System.Numerics.PointF are used by SixLabors.Fonts
// OpenTK.Mathematics.Vector2 is used for GL operations. Disambiguate where necessary.

namespace GameFramework.UI
{
    /// <summary>
    /// Represents a single character's metrics and texture coordinates within the font atlas.
    /// </summary>
    public struct CharacterMetrics
    {
        /// <summary>Gets or sets the U texture coordinate (left edge of the character in the atlas).</summary>
        public float TexU { get; set; }
        /// <summary>Gets or sets the V texture coordinate (top edge of the character in the atlas).</summary>
        public float TexV { get; set; }
        /// <summary>Gets or sets the width of the character in texture coordinates.</summary>
        public float TexWidth { get; set; }
        /// <summary>Gets or sets the height of the character in texture coordinates.</summary>
        public float TexHeight { get; set; }

        /// <summary>Gets or sets the width of the character in pixels.</summary>
        public int Width { get; set; }
        /// <summary>Gets or sets the height of the character in pixels.</summary>
        public int Height { get; set; }

        /// <summary>Gets or sets the bearing (offset from the pen position to the top-left of the character glyph) in pixels. Uses System.Numerics.Vector2 for SixLabors compatibility.</summary>
        public System.Numerics.Vector2 Bearing { get; set; }
        /// <summary>Gets or sets the advance width (how far to move the pen horizontally for the next character) in pixels.</summary>
        public int Advance { get; set; }
    }

    /// <summary>
    /// Provides static methods for rendering text using OpenGL and a generated font atlas.
    /// </summary>
    public static class FontRenderer
    {
        private static int _shaderProgram = -1;
        private static int _vao = -1;
        private static int _vbo = -1;

        /// <summary>Gets the OpenGL texture ID for the font atlas.</summary>
        public static int FontTextureID { get; private set; } = -1;

        private static Dictionary<char, CharacterMetrics> _characterMetrics = new Dictionary<char, CharacterMetrics>();

        private static Matrix4 _projectionMatrix;
        private static int _projectionMatrixLocation = -1;
        private static int _modelMatrixLocation = -1;
        private static int _textureSamplerLocation = -1;
        private static int _textColorLocation = -1;

        private static float[] _quadVertices = new float[6 * 4]; // 6 vertices, 4 components (x,y,u,v) each

        private static OpenTK.Mathematics.Vector3 _currentColor = OpenTK.Mathematics.Vector3.Zero; // Default to Black
        private static float _fontLineHeight = 0;
        private static float _scaledAscender = 0f; // Added for Y-coordinate calculation

        private static Random _random = new Random(); // Added for RandomBounce effect

        /// <summary>Gets or sets the current screen width for projection matrix calculation.</summary>
        public static float ScreenWidth { get; set; } = 800f;
        /// <summary>Gets or sets the current screen height for projection matrix calculation.</summary>
        public static float ScreenHeight { get; set; } = 600f;

        /// <summary>
        /// Initializes the FontRenderer with a specific TTF font file and font size.
        /// Generates a font atlas texture and prepares OpenGL resources for text rendering.
        /// </summary>
        /// <param name="ttfPath">The file path to the TrueType Font (.ttf) file.</param>
        /// <param name="fontSize">The desired font size in points.</param>
        /// <param name="vertexShaderFileName">Optional file name of the vertex shader (e.g., "ui_vertex.glsl"). Expected to be in a 'Shaders' subdirectory relative to the GameFramework assembly or build output.</param>
        /// <param name="fragmentShaderFileName">Optional file name of the fragment shader (e.g., "ui_fragment_texture.glsl"). Expected to be in a 'Shaders' subdirectory relative to the GameFramework assembly or build output.</param>
        /// <exception cref="FileNotFoundException">Thrown if the TTF file or shader files are not found.</exception>
        /// <exception cref="Exception">Thrown if shader compilation or linking fails.</exception>
        public static void Initialize(string ttfPath, float fontSize,
                                      string vertexShaderFileName = "ui_vertex.glsl",
                                      string fragmentShaderFileName = "ui_fragment_texture.glsl")
        {
            try
            {
                string? assemblyDir = System.IO.Path.GetDirectoryName(typeof(FontRenderer).Assembly.Location);
                if (string.IsNullOrEmpty(assemblyDir))
                {
                    assemblyDir = AppContext.BaseDirectory; // Fallback
                }

                // Define base directories to search for shaders
                // GameFramework.csproj copies shaders to "src/Shaders/" relative to its output directory.
                // AppContext.BaseDirectory is the execution directory of the consuming app (e.g., GameEditor/bin/Debug/net8.0)
                var searchBaseDirs = new List<string>();
                if (!string.IsNullOrEmpty(assemblyDir)) {
                    searchBaseDirs.Add(assemblyDir);
                }
                searchBaseDirs.Add(AppContext.BaseDirectory); // Add execution directory of the app

                // Define relative paths to the Shaders folder
                string[] shaderRelativeFolders = {
                    System.IO.Path.Combine("src", "Shaders"), // As per GameFramework.csproj
                    "Shaders"                               // A common alternative
                };

                string? resolvedVertexShaderPath = null;
                string? resolvedFragmentShaderPath = null;
                List<string> attemptedVertexPaths = new List<string>();
                List<string> attemptedFragmentPaths = new List<string>();

                foreach (var baseDir in searchBaseDirs.Distinct().Where(d => !string.IsNullOrEmpty(d)))
                {
                    foreach (var relativeFolder in shaderRelativeFolders)
                    {
                        string potentialPath = System.IO.Path.Combine(baseDir!, relativeFolder, vertexShaderFileName);
                        attemptedVertexPaths.Add(System.IO.Path.GetFullPath(potentialPath));
                        if (File.Exists(potentialPath))
                        {
                            resolvedVertexShaderPath = potentialPath;
                            break;
                        }
                    }
                    if (resolvedVertexShaderPath != null) break;
                }

                foreach (var baseDir in searchBaseDirs.Distinct().Where(d => !string.IsNullOrEmpty(d)))
                {
                    foreach (var relativeFolder in shaderRelativeFolders)
                    {
                        string potentialPath = System.IO.Path.Combine(baseDir!, relativeFolder, fragmentShaderFileName);
                        attemptedFragmentPaths.Add(System.IO.Path.GetFullPath(potentialPath));
                        if (File.Exists(potentialPath))
                        {
                            resolvedFragmentShaderPath = potentialPath;
                            break;
                        }
                    }
                    if (resolvedFragmentShaderPath != null) break;
                }

                if (string.IsNullOrEmpty(resolvedVertexShaderPath))
                {
                    throw new FileNotFoundException($"Vertex shader '{vertexShaderFileName}' not found. Searched in paths derived from assembly and base directories:\n{string.Join("\n", attemptedVertexPaths.Distinct())}", vertexShaderFileName);
                }
                if (string.IsNullOrEmpty(resolvedFragmentShaderPath))
                {
                    throw new FileNotFoundException($"Fragment shader '{fragmentShaderFileName}' not found. Searched in paths derived from assembly and base directories:\n{string.Join("\n", attemptedFragmentPaths.Distinct())}", fragmentShaderFileName);
                }

                Console.WriteLine($"FontRenderer: Using vertex shader: {System.IO.Path.GetFullPath(resolvedVertexShaderPath)}");
                Console.WriteLine($"FontRenderer: Using fragment shader: {System.IO.Path.GetFullPath(resolvedFragmentShaderPath)}");

                _shaderProgram = ShaderHelper.CreateProgram(resolvedVertexShaderPath, resolvedFragmentShaderPath);
                GL.UseProgram(_shaderProgram);

                _projectionMatrixLocation = GL.GetUniformLocation(_shaderProgram, "projection"); // CORRECTED
                _modelMatrixLocation = GL.GetUniformLocation(_shaderProgram, "model");          // CORRECTED
                _textureSamplerLocation = GL.GetUniformLocation(_shaderProgram, "textureSampler");
                _textColorLocation = GL.GetUniformLocation(_shaderProgram, "textColor");

                GL.Uniform1(_textureSamplerLocation, 0); // Texture unit 0
                SetColor(0, 0, 0); // Default to black

                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _quadVertices.Length * sizeof(float), _quadVertices, BufferUsageHint.DynamicDraw);

                // Position attribute (x, y)
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                // Texture coordinate attribute (u, v)
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                UpdateProjectionMatrix();

                if (string.IsNullOrEmpty(ttfPath) || !File.Exists(ttfPath))
                {
                    // The caller of Initialize is responsible for providing a valid, resolvable ttfPath.
                    // If ttfPath is relative, it's relative to the *current working directory* of the process.
                    string fullTtfPath = System.IO.Path.GetFullPath(ttfPath); // Resolve to full path for clarity in error
                    Console.WriteLine($"FontRenderer Error: TTF file not found at the provided path: {fullTtfPath} (Original path: '{ttfPath}')");
                    throw new FileNotFoundException($"TTF file not found. Path provided: '{ttfPath}', Resolved to: '{fullTtfPath}'", ttfPath);
                }

                FontCollection fontCollection = new FontCollection();
                FontFamily fontFamily = fontCollection.Add(ttfPath);
                Font font = fontFamily.CreateFont(fontSize, FontStyle.Regular);

                // Calculate scaled ascender and line height
                var fontMetrics = font.FontMetrics;
                _scaledAscender = (fontMetrics.VerticalMetrics.Ascender / (float)fontMetrics.UnitsPerEm) * font.Size;
                _fontLineHeight = ((fontMetrics.VerticalMetrics.Ascender - fontMetrics.VerticalMetrics.Descender + fontMetrics.VerticalMetrics.LineGap) / (float)fontMetrics.UnitsPerEm) * font.Size;

                const string charSet = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
                _characterMetrics.Clear();

                int atlasWidth = 1024;
                int atlasHeight = 1024;
                int padding = 2;
                float dpi = 72f; // Using 72 DPI means 1 point = 1 pixel for font size

                using (Image<Rgba32> atlasImage = new Image<Rgba32>(Configuration.Default, atlasWidth, atlasHeight, SixLabors.ImageSharp.Color.Transparent))
                {
                    int currentX_atlas = padding;
                    int currentY_atlas = padding;
                    float maxRowHeight_atlas = 0;

                    TextOptions textOptions = new TextOptions(font)
                    {
                        Dpi = dpi,
                        KerningMode = KerningMode.None // Important for individual character measurement
                    };

                    // Corrected line height calculation using FontMetrics properties
                    var fontMetricsForAtlas = font.FontMetrics;
                    _fontLineHeight = ((fontMetricsForAtlas.VerticalMetrics.Ascender - fontMetricsForAtlas.VerticalMetrics.Descender + fontMetricsForAtlas.VerticalMetrics.LineGap) / (float)fontMetricsForAtlas.UnitsPerEm) * font.Size;

                    foreach (char c in charSet)
                    {
                        FontRectangle renderedBounds = TextMeasurer.MeasureBounds(c.ToString(), textOptions);
                        FontRectangle advanceRect = TextMeasurer.MeasureAdvance(c.ToString(), textOptions);
                        float advance = advanceRect.Width;

                        if (c == ' ' || renderedBounds.Width == 0 || renderedBounds.Height == 0) // Handle space or non-rendering chars
                        {
                            _characterMetrics[c] = new CharacterMetrics
                            {
                                TexU = 0,
                                TexV = 0,
                                TexWidth = 0,
                                TexHeight = 0,
                                Width = 0,
                                Height = 0,
                                Bearing = System.Numerics.Vector2.Zero,
                                Advance = (int)Math.Ceiling((double)advance)
                            };
                            continue;
                        }

                        if (currentX_atlas + (int)Math.Ceiling((double)renderedBounds.Width) + padding > atlasWidth)
                        {
                            currentX_atlas = padding;
                            currentY_atlas += (int)maxRowHeight_atlas + padding;
                            maxRowHeight_atlas = 0;
                        }

                        if (currentY_atlas + (int)Math.Ceiling((double)renderedBounds.Height) + padding > atlasHeight)
                        {
                            Console.WriteLine($"FontRenderer Warning: Atlas size too small. Character '{c}' skipped.");
                            FontRectangle skippedAdvanceRect = TextMeasurer.MeasureAdvance(c.ToString(), textOptions);
                            float skippedAdvance = skippedAdvanceRect.Width;
                            _characterMetrics[c] = new CharacterMetrics
                            {
                                TexU = 0,
                                TexV = 0,
                                TexWidth = 0,
                                TexHeight = 0,
                                Width = 0,
                                Height = 0,
                                Bearing = System.Numerics.Vector2.Zero,
                                Advance = (int)Math.Ceiling((double)skippedAdvance)
                            };
                            continue;
                        }

                        // Configure drawing options and text options for SixLabors.ImageSharp.Drawing
                        DrawingOptions drawingOptions = new DrawingOptions();
                        drawingOptions.GraphicsOptions.Antialias = true;
                        // textOptions is already defined: new TextOptions(font) { Dpi = dpi, KerningMode = None };

                        var brush = new SolidBrush(SixLabors.ImageSharp.Color.White);
                        var penLocation = new SixLabors.ImageSharp.PointF(currentX_atlas - renderedBounds.X, currentY_atlas - renderedBounds.Y);

                        atlasImage.Mutate(ctx => ctx.DrawText(
                            c.ToString(),
                            font,
                            brush,
                            penLocation
                        ));

                        _characterMetrics[c] = new CharacterMetrics
                        {
                            TexU = (float)currentX_atlas / atlasWidth,
                            TexV = (float)currentY_atlas / atlasHeight,
                            TexWidth = renderedBounds.Width / atlasWidth,
                            TexHeight = renderedBounds.Height / atlasHeight,
                            Width = (int)Math.Ceiling((double)renderedBounds.Width),
                            Height = (int)Math.Ceiling((double)renderedBounds.Height),
                            Bearing = new System.Numerics.Vector2(renderedBounds.X, renderedBounds.Y),
                            Advance = (int)Math.Ceiling((double)advance)
                        };

                        currentX_atlas += (int)Math.Ceiling((double)renderedBounds.Width) + padding;
                        if (Math.Ceiling((double)renderedBounds.Height) > maxRowHeight_atlas)
                        {
                            maxRowHeight_atlas = (float)Math.Ceiling((double)renderedBounds.Height);
                        }
                    }

                    FontTextureID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, FontTextureID);

                    byte[] pixelData = new byte[atlasWidth * atlasHeight * 4];
                    atlasImage.CopyPixelDataTo(pixelData);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, atlasWidth, atlasHeight, 0,
                                  OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, pixelData);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }

                // Enable blending for transparency
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest); // Explicitly disable depth test for UI

                Console.WriteLine($"FontRenderer initialized with font {font.Name}. Atlas generated. Texture ID: {FontTextureID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FontRenderer Initialization Error: {ex.Message}\n{ex.StackTrace}");
                // Clean up partial initialization if possible
                Dispose();
                throw; // Re-throw to signal failure
            }
        }

        /// <summary>
        /// Updates the projection matrix based on the current <see cref="ScreenWidth"/> and <see cref="ScreenHeight"/>.
        /// Should be called when the game window resizes.
        /// </summary>
        public static void UpdateProjectionMatrix()
        {
            _projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ScreenWidth, ScreenHeight, 0, -1f, 1f);
            if (_shaderProgram != -1 && _projectionMatrixLocation != -1)
            {
                GL.UseProgram(_shaderProgram); // Ensure correct program is active
                GL.UniformMatrix4(_projectionMatrixLocation, false, ref _projectionMatrix);
            }
        }

        /// <summary>
        /// Sets the color for subsequent text rendering.
        /// </summary>
        /// <param name="r">Red component (0.0 to 1.0).</param>
        /// <param name="g">Green component (0.0 to 1.0).</param>
        /// <param name="b">Blue component (0.0 to 1.0).</param>
        public static void SetColor(float r, float g, float b)
        {
            _currentColor = new OpenTK.Mathematics.Vector3(r, g, b);
            if (_shaderProgram != -1 && _textColorLocation != -1)
            {
                GL.UseProgram(_shaderProgram); // Ensure correct program is active
                GL.Uniform3(_textColorLocation, _currentColor);
            }
        }

        /// <summary>
        /// Draws the specified text string at the given screen coordinates (top-left origin).
        /// This overload defaults to no text effect.
        /// </summary>
        /// <param name="text">The string to render.</param>
        /// <param name="startX">The x-coordinate for the top-left of the text.</param>
        /// <param name="startY">The y-coordinate for the top-left of the text.</param>
        public static void DrawText(string text, float startX, float startY)
        {
            DrawText(text, startX, startY, TextEffect.None, 0f, 0f, 0f);
        }

        /// <summary>
        /// Draws the specified text string at the given screen coordinates (top-left origin),
        /// optionally applying a text effect.
        /// </summary>
        /// <param name="text">The string to render.</param>
        /// <param name="startX">The x-coordinate for the top-left of the text.</param>
        /// <param name="startY">The y-coordinate for the top-left of the text.</param>
        /// <param name="effect">The text effect to apply.</param>
        /// <param name="effectStrength">The strength of the effect (e.g., bounce height).</param>
        /// <param name="effectSpeed">The speed of the effect (e.g., bounce cycles per second).</param>
        /// <param name="elapsedTime">The total elapsed time, used for animation.</param>
        public static void DrawText(string text, float startX, float startY, TextEffect effect, float effectStrength, float effectSpeed, float elapsedTime)
        {
            if (_shaderProgram == -1 || string.IsNullOrEmpty(text) || FontTextureID <= 0 || _characterMetrics.Count == 0)
            {
                return;
            }

            GL.UseProgram(_shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, FontTextureID);
            GL.Uniform1(_textureSamplerLocation, 0); 
            GL.Uniform3(_textColorLocation, _currentColor); 
            GL.UniformMatrix4(_projectionMatrixLocation, false, ref _projectionMatrix);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo); 

            float currentPenX = startX;
            float charIndex = 0; // For staggering effects like bounce

            foreach (char c in text)
            {
                if (!_characterMetrics.TryGetValue(c, out CharacterMetrics metrics))
                {
                    if (!_characterMetrics.TryGetValue('?', out metrics)) 
                    {
                        currentPenX += GetTextWidth(" ") / 2; 
                        continue;
                    }
                }

                if (c == ' ')
                {
                    currentPenX += metrics.Advance;
                    charIndex++;
                    continue;
                }
                
                if (metrics.Width == 0 || metrics.Height == 0)
                {
                    currentPenX += metrics.Advance;
                    charIndex++;
                    continue;
                }

                float xPos = currentPenX + metrics.Bearing.X;
                float yOffset = 0;

                if (effect == TextEffect.Bounce)
                {
                    yOffset = (float)Math.Sin((elapsedTime * effectSpeed * 2 * Math.PI) + (charIndex * 0.5f)) * effectStrength;
                }
                else if (effect == TextEffect.RandomBounce)
                {
                    int charSeed = (int)charIndex * 12345; // Multiply by a prime to spread seeds
                    Random charRandom = new Random(charSeed);

                    float charStrength = effectStrength * (0.5f + (float)charRandom.NextDouble() * 0.5f);
                    float charSpeed = effectSpeed * (0.5f + (float)charRandom.NextDouble()); 
                    float phaseOffset = (float)charRandom.NextDouble() * 2.0f * (float)Math.PI;

                    yOffset = (float)Math.Sin((elapsedTime * charSpeed * 2 * Math.PI) + phaseOffset) * charStrength;
                }
                
                float yPos = startY + metrics.Bearing.Y + yOffset;

                float w = metrics.Width;
                float h = metrics.Height;

                _quadVertices[0] = xPos; _quadVertices[1] = yPos; _quadVertices[2] = metrics.TexU; _quadVertices[3] = metrics.TexV;                                         
                _quadVertices[4] = xPos; _quadVertices[5] = yPos + h; _quadVertices[6] = metrics.TexU; _quadVertices[7] = metrics.TexV + metrics.TexHeight;                     
                _quadVertices[8] = xPos + w; _quadVertices[9] = yPos; _quadVertices[10] = metrics.TexU + metrics.TexWidth; _quadVertices[11] = metrics.TexV;                      

                _quadVertices[12] = xPos; _quadVertices[13] = yPos + h; _quadVertices[14] = metrics.TexU; _quadVertices[15] = metrics.TexV + metrics.TexHeight;                  
                _quadVertices[16] = xPos + w; _quadVertices[17] = yPos + h; _quadVertices[18] = metrics.TexU + metrics.TexWidth; _quadVertices[19] = metrics.TexV + metrics.TexHeight; 
                _quadVertices[20] = xPos + w; _quadVertices[21] = yPos; _quadVertices[22] = metrics.TexU + metrics.TexWidth; _quadVertices[23] = metrics.TexV;                   

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _quadVertices.Length * sizeof(float), _quadVertices);
                Matrix4 modelMatrix = Matrix4.Identity; 
                GL.UniformMatrix4(_modelMatrixLocation, false, ref modelMatrix);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                currentPenX += metrics.Advance;
                charIndex++;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /// <summary>
        /// Calculates the width of the given text string if rendered with the current font.
        /// </summary>
        /// <param name="text">The text string to measure.</param>
        /// <returns>The width of the text in pixels.</returns>
        public static float GetTextWidth(string text)
        {
            if (string.IsNullOrEmpty(text) || _characterMetrics.Count == 0)
                return 0;

            float totalWidth = 0;
            foreach (char c in text)
            {
                if (_characterMetrics.TryGetValue(c, out var metrics))
                {
                    totalWidth += metrics.Advance;
                }
                else if (_characterMetrics.TryGetValue('?', out var fallbackMetrics)) // Fallback for unknown char
                {
                    totalWidth += fallbackMetrics.Advance;
                }
            }
            return totalWidth;
        }

        /// <summary>
        /// Gets the height of a single line of text (font dependent).
        /// </summary>
        /// <returns>The height of a line of text in pixels.</returns>
        public static float GetTextHeight()
        {
            // This could be based on the tallest character in the atlas, or a fixed value from font metrics.
            // Using _fontLineHeight which should be set during Initialize from FontMetrics.
            if (_fontLineHeight > 0) return _fontLineHeight;

            // Fallback if _fontLineHeight wasn't set: find max char height in current set (less accurate for line spacing)
            float maxHeight = 0;
            if (_characterMetrics.Count > 0)
            {
                foreach (var metrics in _characterMetrics.Values)
                {
                    if (metrics.Height > maxHeight) maxHeight = metrics.Height;
                }
            }
            return maxHeight > 0 ? maxHeight : 20; // Default if no metrics
        }


        /// <summary>
        /// Releases OpenGL resources used by the FontRenderer.
        /// </summary>
        public static void Dispose()
        {
            if (_vbo != -1)
            {
                GL.DeleteBuffer(_vbo); _vbo = -1;
            }
            if (_vao != -1)
            {
                GL.DeleteVertexArray(_vao); _vao = -1;
            }
            if (FontTextureID != -1)
            {
                GL.DeleteTexture(FontTextureID); FontTextureID = -1;
            }
            if (_shaderProgram != -1)
            {
                GL.DeleteProgram(_shaderProgram); _shaderProgram = -1;
            }
            _characterMetrics.Clear();

            // Disable blending if it was enabled by this renderer
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest); // Restore depth test state

            Console.WriteLine("FontRenderer disposed.");
        }
    }
}
