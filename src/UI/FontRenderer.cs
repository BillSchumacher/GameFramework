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
using System.Collections.Generic; // Added for List<Vector3>
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
    /// Internal class to hold data for a specific font file and size combination.
    /// </summary>
    internal class FontInstance : IDisposable
    {
        public string FilePath { get; }
        public float Size { get; }
        public int TextureID { get; internal set; } = -1;
        public Dictionary<char, CharacterMetrics> CharacterMetrics { get; } = new Dictionary<char, CharacterMetrics>();
        public float LineHeight { get; internal set; }
        public float ScaledAscender { get; internal set; }
        public Font Font { get; } // SixLabors.Fonts.Font object

        public FontInstance(string filePath, float size, Font font)
        {
            FilePath = filePath;
            Size = size;
            Font = font;
        }

        public void Dispose()
        {
            if (TextureID != -1)
            {
                GL.DeleteTexture(TextureID);
                TextureID = -1;
            }
            CharacterMetrics.Clear();
            // The Font object itself might not need explicit disposal here if managed by FontCollection
            Console.WriteLine($"Disposed FontInstance: {FilePath} ({Size}pt), TextureID: {TextureID}");
        }
    }

    /// <summary>
    /// Provides static methods for rendering text using OpenGL and a generated font atlas.
    /// </summary>
    public static class FontRenderer
    {
        private static int _shaderProgram = -1;
        private static int _vao = -1;
        private static int _vbo = -1;

        private static FontCollection _fontCollection = new FontCollection();
        private static Dictionary<string, FontInstance> _loadedFonts = new Dictionary<string, FontInstance>();
        private static FontInstance? _activeFontInstance;
        private static string _defaultFontKey = string.Empty;

        private static Matrix4 _projectionMatrix;
        private static int _projectionMatrixLocation = -1;
        private static int _modelMatrixLocation = -1;
        private static int _textureSamplerLocation = -1;
        private static int _textColorLocation = -1;

        private static float[] _quadVertices = new float[6 * 4]; // 6 vertices, 4 components (x,y,u,v) each

        private static OpenTK.Mathematics.Vector3 _currentColor = OpenTK.Mathematics.Vector3.Zero; // Default to Black

        private static Random _random = new Random(); // Added for RandomBounce effect

        /// <summary>Gets or sets the current screen width for projection matrix calculation.</summary>
        public static float ScreenWidth { get; set; } = 800f;
        /// <summary>Gets or sets the current screen height for projection matrix calculation.</summary>
        public static float ScreenHeight { get; set; } = 600f;

        /// <summary>
        /// Initializes the FontRenderer with a specific TTF font file and font size.
        /// Generates a font atlas texture and prepares OpenGL resources for text rendering.
        /// </summary>
        /// <param name="defaultTtfPath">The file path to the default TrueType Font (.ttf) file.</param>
        /// <param name="defaultFontSize">The desired default font size in points.</param>
        /// <param name="vertexShaderFileName">Optional file name of the vertex shader (e.g., "ui_vertex.glsl"). Expected to be in a 'Shaders' subdirectory relative to the GameFramework assembly or build output.</param>
        /// <param name="fragmentShaderFileName">Optional file name of the fragment shader (e.g., "ui_fragment_texture.glsl"). Expected to be in a 'Shaders' subdirectory relative to the GameFramework assembly or build output.</param>
        /// <exception cref="FileNotFoundException">Thrown if the TTF file or shader files are not found.</exception>
        /// <exception cref="Exception">Thrown if shader compilation or linking fails.</exception>
        public static void Initialize(string defaultTtfPath, float defaultFontSize,
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

                var searchBaseDirs = new List<string>();
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    searchBaseDirs.Add(assemblyDir);
                }
                searchBaseDirs.Add(AppContext.BaseDirectory);

                string[] shaderRelativeFolders = {
                    System.IO.Path.Combine("src", "Shaders"),
                    "Shaders"
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

                _projectionMatrixLocation = GL.GetUniformLocation(_shaderProgram, "projection");
                _modelMatrixLocation = GL.GetUniformLocation(_shaderProgram, "model");
                _textureSamplerLocation = GL.GetUniformLocation(_shaderProgram, "textureSampler");
                _textColorLocation = GL.GetUniformLocation(_shaderProgram, "textColor");

                GL.Uniform1(_textureSamplerLocation, 0);
                SetColor(0, 0, 0);

                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _quadVertices.Length * sizeof(float), _quadVertices, BufferUsageHint.DynamicDraw);

                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);

                UpdateProjectionMatrix();

                LoadAndSetFont(defaultTtfPath, defaultFontSize, true);
                _defaultFontKey = GetFontKey(defaultTtfPath, defaultFontSize);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);

                Console.WriteLine($"FontRenderer initialized. Default font: {defaultTtfPath} ({defaultFontSize}pt).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FontRenderer Initialization Error: {ex.Message}\n{ex.StackTrace}");
                Dispose();
                throw;
            }
        }

        private static string GetFontKey(string ttfPath, float fontSize)
        {
            return $"{System.IO.Path.GetFullPath(ttfPath).ToLowerInvariant()}:{fontSize}";
        }

        /// <summary>
        /// Loads the specified font and size if not already loaded, and sets it as the active font for rendering.
        /// </summary>
        /// <param name="ttfPath">Path to the TTF font file.</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <param name="isInitializing">Internal flag to avoid issues during initial setup.</param>
        /// <exception cref="FileNotFoundException">If the TTF file is not found.</exception>
        /// <exception cref="Exception">If font loading or atlas generation fails.</exception>
        public static void LoadAndSetFont(string ttfPath, float fontSize, bool isInitializing = false)
        {
            if (string.IsNullOrEmpty(ttfPath))
            {
                Console.WriteLine("FontRenderer Error: TTF path cannot be null or empty.");
                if (!string.IsNullOrEmpty(_defaultFontKey) && _loadedFonts.TryGetValue(_defaultFontKey, out var defaultFont))
                {
                    _activeFontInstance = defaultFont;
                    Console.WriteLine($"Reverted to default font: {_defaultFontKey}");
                }
                else if (_loadedFonts.Count > 0)
                {
                    _activeFontInstance = _loadedFonts.Values.First();
                    Console.WriteLine($"Reverted to first available font: {_activeFontInstance.FilePath} ({_activeFontInstance.Size}pt)");
                }
                else
                {
                    _activeFontInstance = null;
                    Console.WriteLine("No fonts available to set as active.");
                }
                return;
            }

            string resolvedPath = "";
            bool fileFound = false;

            if (System.IO.Path.IsPathRooted(ttfPath))
            {
                if (File.Exists(ttfPath))
                {
                    resolvedPath = System.IO.Path.GetFullPath(ttfPath);
                    fileFound = true;
                }
            }
            else
            {
                string pathRelativeToCwd = System.IO.Path.GetFullPath(ttfPath);
                if (File.Exists(pathRelativeToCwd))
                {
                    resolvedPath = pathRelativeToCwd;
                    fileFound = true;
                }
                else
                {
                    string pathRelativeToBase = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, ttfPath));
                    if (File.Exists(pathRelativeToBase))
                    {
                        resolvedPath = pathRelativeToBase;
                        fileFound = true;
                    }
                    else
                    {
                        string pathRelativeToBaseAssets = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", ttfPath));
                        if (File.Exists(pathRelativeToBaseAssets))
                        {
                            resolvedPath = pathRelativeToBaseAssets;
                            fileFound = true;
                        }
                    }
                }
            }

            if (!fileFound)
            {
                string originalAttemptForMsg = System.IO.Path.GetFullPath(ttfPath);
                Console.WriteLine($"FontRenderer Error: TTF file not found. Original input: '{ttfPath}'. Attempted to resolve to (e.g.): '{originalAttemptForMsg}', and other common locations, but failed.");

                if (!isInitializing)
                {
                    if (!string.IsNullOrEmpty(_defaultFontKey) && _loadedFonts.TryGetValue(_defaultFontKey, out var defaultFont))
                    {
                        _activeFontInstance = defaultFont;
                        Console.WriteLine($"Failed to load '{ttfPath}', reverted to default font: {_defaultFontKey}");
                    }
                    else if (_loadedFonts.Count > 0)
                    {
                        _activeFontInstance = _loadedFonts.Values.First();
                        Console.WriteLine($"Failed to load '{ttfPath}', reverted to first available font: {_activeFontInstance.FilePath} ({_activeFontInstance.Size}pt)");
                    }
                    else
                    {
                        _activeFontInstance = null;
                        Console.WriteLine($"Failed to load '{ttfPath}', and no other fonts are available.");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"TTF file not found during initialization. Original input: '{ttfPath}'. Attempted to resolve to (e.g.): '{originalAttemptForMsg}', and other common locations, but failed.", ttfPath);
                }
                return;
            }

            string fontKey = GetFontKey(resolvedPath, fontSize);

            if (_loadedFonts.TryGetValue(fontKey, out FontInstance? existingInstance))
            {
                _activeFontInstance = existingInstance;
                return;
            }

            try
            {
                FontInstance newInstance = CreateFontInstance(resolvedPath, fontSize);
                _loadedFonts[fontKey] = newInstance;
                _activeFontInstance = newInstance;
                Console.WriteLine($"FontRenderer: Loaded and set active font: {fontKey}, TextureID: {newInstance.TextureID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FontRenderer Error: Failed to create font instance for {fontKey} (from path {resolvedPath}). {ex.Message}\n{ex.StackTrace}");
                if (!isInitializing)
                {
                    if (!string.IsNullOrEmpty(_defaultFontKey) && _loadedFonts.TryGetValue(_defaultFontKey, out var defaultFont))
                    {
                        _activeFontInstance = defaultFont;
                        Console.WriteLine($"Reverted to default font: {_defaultFontKey}");
                    }
                    else if (_loadedFonts.Count > 0)
                    {
                        _activeFontInstance = _loadedFonts.Values.First();
                        Console.WriteLine($"Reverted to first available font: {_activeFontInstance.FilePath} ({_activeFontInstance.Size}pt)");
                    }
                    else _activeFontInstance = null;
                }
                else
                {
                    throw;
                }
            }
        }

        private static FontInstance CreateFontInstance(string resolvedTtfPath, float fontSize)
        {
            if (!File.Exists(resolvedTtfPath))
            {
                throw new FileNotFoundException($"TTF file not found for CreateFontInstance. Path: '{resolvedTtfPath}'", resolvedTtfPath);
            }

            FontFamily fontFamily;
            string familyNameKey = System.IO.Path.GetFileNameWithoutExtension(resolvedTtfPath);
            if (!_fontCollection.TryGet(familyNameKey, out fontFamily!))
            {
                fontFamily = _fontCollection.Add(resolvedTtfPath);
            }
            Font font = fontFamily.CreateFont(fontSize, FontStyle.Regular);

            FontInstance instance = new FontInstance(resolvedTtfPath, fontSize, font);

            var fontMetrics = font.FontMetrics;
            instance.ScaledAscender = (fontMetrics.VerticalMetrics.Ascender / (float)fontMetrics.UnitsPerEm) * font.Size;
            instance.LineHeight = ((fontMetrics.VerticalMetrics.Ascender - fontMetrics.VerticalMetrics.Descender + fontMetrics.VerticalMetrics.LineGap) / (float)fontMetrics.UnitsPerEm) * font.Size;

            const string charSet = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            instance.CharacterMetrics.Clear();

            int atlasWidth = 1024;
            int atlasHeight = 1024;
            int padding = 2;
            float dpi = 72f;

            using (Image<Rgba32> atlasImage = new Image<Rgba32>(Configuration.Default, atlasWidth, atlasHeight, SixLabors.ImageSharp.Color.Transparent))
            {
                int currentX_atlas = padding;
                int currentY_atlas = padding;
                float maxRowHeight_atlas = 0;

                TextOptions textOptions = new TextOptions(font)
                {
                    Dpi = dpi,
                    KerningMode = KerningMode.None
                };

                foreach (char c in charSet)
                {
                    FontRectangle renderedBounds = TextMeasurer.MeasureBounds(c.ToString(), textOptions);
                    FontRectangle advanceRect = TextMeasurer.MeasureAdvance(c.ToString(), textOptions);
                    float advance = advanceRect.Width;

                    if (c == ' ' || renderedBounds.Width == 0 || renderedBounds.Height == 0)
                    {
                        instance.CharacterMetrics[c] = new CharacterMetrics
                        {
                            TexU = 0, TexV = 0, TexWidth = 0, TexHeight = 0,
                            Width = 0, Height = 0,
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
                        Console.WriteLine($"FontRenderer Warning: Atlas for {font.Name} ({fontSize}pt) too small. Character '{c}' skipped.");
                        FontRectangle skippedAdvanceRect = TextMeasurer.MeasureAdvance(c.ToString(), textOptions);
                        instance.CharacterMetrics[c] = new CharacterMetrics
                        {
                            TexU = 0, TexV = 0, TexWidth = 0, TexHeight = 0,
                            Width = 0, Height = 0,
                            Bearing = System.Numerics.Vector2.Zero,
                            Advance = (int)Math.Ceiling((double)skippedAdvanceRect.Width)
                        };
                        continue;
                    }

                    var brush = new SolidBrush(SixLabors.ImageSharp.Color.White);
                    var penLocation = new SixLabors.ImageSharp.PointF(currentX_atlas - renderedBounds.X, currentY_atlas - renderedBounds.Y);

                    atlasImage.Mutate(ctx => ctx.DrawText(c.ToString(), font, brush, penLocation));

                    instance.CharacterMetrics[c] = new CharacterMetrics
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

                instance.TextureID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, instance.TextureID);

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
            return instance;
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
                GL.UseProgram(_shaderProgram);
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
                GL.UseProgram(_shaderProgram);
                GL.Uniform3(_textColorLocation, _currentColor);
            }
        }

        /// <summary>
        /// Draws the specified text string at the given screen coordinates (top-left origin).
        /// This overload defaults to no text effect and uses the globally set color.
        /// </summary>
        public static void DrawText(string text, float startX, float startY)
        {
            DrawText(text, startX, startY, TextEffect.None, 0f, 0f, 0f, null);
        }

        /// <summary>
        /// Draws the specified text string at the given screen coordinates (top-left origin),
        /// optionally applying a text effect and per-character colors.
        /// </summary>
        /// <param name="text">The string to render.</param>
        /// <param name="startX">The x-coordinate for the top-left of the text.</param>
        /// <param name="startY">The y-coordinate for the top-left of the text.</param>
        /// <param name="effect">The text effect to apply.</param>
        /// <param name="effectStrength">The strength of the effect (e.g., bounce height).</param>
        /// <param name="effectSpeed">The speed of the effect (e.g., bounce cycles per second).</param>
        /// <param name="elapsedTime">The total elapsed time, used for animation.</param>
        /// <param name="characterColors">Optional list of colors for each character. If null or count mismatch, uses globally set color.</param>
        public static void DrawText(string text, float startX, float startY, TextEffect effect, float effectStrength, float effectSpeed, float elapsedTime, List<Vector3>? characterColors = null)
        {
            if (_activeFontInstance == null || _shaderProgram == -1 || string.IsNullOrEmpty(text) || _activeFontInstance.TextureID <= 0 || _activeFontInstance.CharacterMetrics.Count == 0)
            {
                return;
            }

            GL.UseProgram(_shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _activeFontInstance.TextureID);
            GL.Uniform1(_textureSamplerLocation, 0);
            GL.UniformMatrix4(_projectionMatrixLocation, false, ref _projectionMatrix);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            float currentPenX = startX;
            int currentCharacterIndex = 0;

            string textToRender = text;
            if (effect == TextEffect.Typewriter)
            {
                if (effectSpeed <= 0) effectSpeed = 10;
                int charsToShow = (int)(elapsedTime * effectSpeed);
                if (charsToShow <= 0) textToRender = "";
                else if (charsToShow < text.Length)
                {
                    textToRender = text.Substring(0, charsToShow);
                }
            }

            bool useCustomCharColors = characterColors != null && characterColors.Count == textToRender.Length;
            if (!useCustomCharColors)
            {
                GL.Uniform3(_textColorLocation, _currentColor);
            }

            foreach (char c in textToRender)
            {
                if (!_activeFontInstance.CharacterMetrics.TryGetValue(c, out CharacterMetrics metrics))
                {
                    if (!_activeFontInstance.CharacterMetrics.TryGetValue('?', out metrics))
                    {
                        currentPenX += GetTextWidth(" ") / 2;
                        currentCharacterIndex++;
                        continue;
                    }
                }

                if (c == ' ')
                {
                    currentPenX += metrics.Advance;
                    currentCharacterIndex++;
                    continue;
                }

                if (metrics.Width == 0 || metrics.Height == 0)
                {
                    currentPenX += metrics.Advance;
                    currentCharacterIndex++;
                    continue;
                }

                if (useCustomCharColors)
                {
                    Vector3 charColor = characterColors![currentCharacterIndex];
                    GL.Uniform3(_textColorLocation, charColor);
                }

                float xPos = currentPenX + metrics.Bearing.X;
                float yPos = startY + metrics.Bearing.Y;
                float xOffset = 0;
                float yOffset = 0;

                if (effect == TextEffect.Bounce)
                {
                    yOffset = (float)Math.Sin((elapsedTime * effectSpeed * 2 * Math.PI) + (currentCharacterIndex * 0.5f)) * effectStrength;
                }
                else if (effect == TextEffect.RandomBounce)
                {
                    int charSeed = currentCharacterIndex * 12345;
                    Random charRandom = new Random(charSeed);
                    float charStrength = effectStrength * (0.5f + (float)charRandom.NextDouble() * 0.5f);
                    float charSpeedFactor = effectSpeed * (0.5f + (float)charRandom.NextDouble());
                    float phaseOffset = (float)charRandom.NextDouble() * 2.0f * (float)Math.PI;
                    yOffset = (float)Math.Sin((elapsedTime * charSpeedFactor * 2 * Math.PI) + phaseOffset) * charStrength;
                }
                else if (effect == TextEffect.Jitter)
                {
                    xOffset = ((float)_random.NextDouble() * 2f - 1f) * effectStrength;
                    yOffset = ((float)_random.NextDouble() * 2f - 1f) * effectStrength;
                }

                xPos += xOffset;
                yPos += yOffset;

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
                currentCharacterIndex++;
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
            if (_activeFontInstance == null || string.IsNullOrEmpty(text) || _activeFontInstance.CharacterMetrics.Count == 0)
                return 0;

            float totalWidth = 0;
            foreach (char c in text)
            {
                if (_activeFontInstance.CharacterMetrics.TryGetValue(c, out var metrics))
                {
                    totalWidth += metrics.Advance;
                }
                else if (_activeFontInstance.CharacterMetrics.TryGetValue('?', out var fallbackMetrics))
                {
                    totalWidth += fallbackMetrics.Advance;
                }
            }
            return totalWidth;
        }

        /// <summary>
        /// Gets the height of a single line of text (font dependent) using the active font.
        /// </summary>
        /// <returns>The height of a line of text in pixels.</returns>
        public static float GetTextHeight()
        {
            if (_activeFontInstance == null) return 20;

            if (_activeFontInstance.LineHeight > 0) return _activeFontInstance.LineHeight;

            float maxHeight = 0;
            if (_activeFontInstance.CharacterMetrics.Count > 0)
            {
                foreach (var metrics in _activeFontInstance.CharacterMetrics.Values)
                {
                    if (metrics.Height > maxHeight) maxHeight = metrics.Height;
                }
            }
            return maxHeight > 0 ? maxHeight : 20;
        }

        /// <summary>
        /// Releases OpenGL resources used by the FontRenderer, including all loaded font textures.
        /// </summary>
        public static void Dispose()
        {
            // Dispose individual font instances (textures)
            foreach (var fontInstance in _loadedFonts.Values)
            {
                fontInstance.Dispose(); // This will call GL.DeleteTexture
            }
            _loadedFonts.Clear();
            _activeFontInstance = null;
            _fontCollection = new FontCollection(); // Reset font collection

            // Dispose shared OpenGL resources
            if (_vbo != -1)
            {
                GL.DeleteBuffer(_vbo); _vbo = -1;
            }
            if (_vao != -1)
            {
                GL.DeleteVertexArray(_vao); _vao = -1;
            }
            // FontTextureID is per instance now, handled by FontInstance.Dispose()
            if (_shaderProgram != -1)
            {
                GL.DeleteProgram(_shaderProgram); _shaderProgram = -1;
            }
            // _characterMetrics is per instance now.

            // Disable blending if it was enabled by this renderer
            try
            {
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest); // Restore depth test state
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FontRenderer Dispose: Error during GL state reset (context might be lost): {ex.Message}");
            }

            Console.WriteLine("FontRenderer disposed all resources.");
        }
    }
}
