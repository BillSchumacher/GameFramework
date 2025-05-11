using Xunit;
using GameFramework.UI;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop; // Added for NativeWindow
using OpenTK.Windowing.Common;   // Added for VSyncMode, ContextFlags
using OpenTK.Graphics.OpenGL4;   // Added for GL related operations if needed directly in helper
using System.Reflection; // Added for testing internal state

namespace GameFramework.Tests.UI
{
    public class FontRendererTests : IDisposable
    {
        private const string TestFontPath = "TestAssets/arial.ttf"; // Relative to execution
        private const float TestFontSize = 16f;
        private static bool _isInitialized = false;
        private static object _initLock = new object();

        public FontRendererTests()
        {
            lock (_initLock) // Ensure thread-safe initialization for parallel tests
            {
                if (!_isInitialized)
                {
                    try
                    {
                        GameWindowlessHelper.EnsureInitialized();
                        if (!File.Exists(TestFontPath))
                        {
                            throw new FileNotFoundException($"Test font file not found at {Path.GetFullPath(TestFontPath)}. Ensure it is copied to the output directory via GameFramework.Tests.csproj.");
                        }
                        FontRenderer.Initialize(TestFontPath, TestFontSize);
                        FontRenderer.ScreenWidth = 800; // Set default screen size for tests
                        FontRenderer.ScreenHeight = 600;
                        FontRenderer.UpdateProjectionMatrix();
                        _isInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Test constructor FontRenderer.Initialize failed: {ex.Message}. Some tests might not run as expected.");
                        // Allow tests to proceed; they should fail if FontRenderer is not usable.
                    }
                }
            }
        }

        public void Dispose()
        {
            // No per-test disposal needed if FontRenderer is static and initialized once
            // GameWindowlessHelper.CleanUp(); // This would be called globally after all tests
        }

        [Fact]
        public void Initialize_WithFont_PopulatesMetricsAndTexture()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized.");
            Assert.True(FontRenderer.FontTextureID > 0, "FontTextureID should be greater than 0 after initialization.");
            Assert.True(FontRenderer.GetTextHeight() > 0, "Font line height should be greater than 0.");
        }

        [Fact]
        public void GetTextWidth_WithValidText_ReturnsNonZeroForRealFont()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextWidth.");
            float width = FontRenderer.GetTextWidth("Hello");
            Assert.True(width > 0, "Text width for 'Hello' should be positive.");
        }

        [Fact]
        public void GetTextWidth_EmptyString_ReturnsZero()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextWidth (empty string).");
            float width = FontRenderer.GetTextWidth("");
            Assert.Equal(0, width);
        }

        [Fact]
        public void GetTextWidth_Spaces_ReturnsWidth()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextWidth (spaces).");
            float spaceWidth = FontRenderer.GetTextWidth(" ");
            Assert.True(spaceWidth > 0, "Width of a single space should be greater than 0.");
            float spacesWidth = FontRenderer.GetTextWidth("   ");
            Assert.True(spacesWidth > spaceWidth, "Width of multiple spaces should be greater than a single space.");
            // Approximate check, assuming advance of space is consistent
            Assert.InRange(spacesWidth, spaceWidth * 2.9f, spaceWidth * 3.1f); 
        }

        [Fact]
        public void GetTextWidth_KnownString_ReturnsApproximateWidth()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextWidth (known string).");
            // This is highly dependent on the font and size. 
            // For Arial 16pt, "Test" is roughly 30-40 pixels wide.
            // This is a sanity check, not a precise measurement.
            float width = FontRenderer.GetTextWidth("Test");
            Assert.InRange(width, 25f, 55f); // Adjusted range based on typical Arial 16px rendering
        }


        [Fact]
        public void GetTextHeight_ReturnsNonZeroForRealFont()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextHeight.");
            float height = FontRenderer.GetTextHeight();
            Assert.True(height > 0, "Text height should be positive.");
        }

        [Fact]
        public void GetTextHeight_IsConsistent()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for GetTextHeight (consistent).");
            float height1 = FontRenderer.GetTextHeight();
            float height2 = FontRenderer.GetTextHeight();
            Assert.Equal(height1, height2);
            Assert.InRange(height1, 10f, 30f); // Sanity check for Arial 16px
        }

        [Fact]
        public void SetColor_UpdatesInternalColorField()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for SetColor.");
            var expectedColor = new OpenTK.Mathematics.Vector3(0.1f, 0.2f, 0.3f);
            FontRenderer.SetColor(expectedColor.X, expectedColor.Y, expectedColor.Z);

            // Use reflection to check the internal static field _currentColor
            FieldInfo? colorField = typeof(FontRenderer).GetField("_currentColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(colorField); 
            
            object? fieldValue = colorField.GetValue(null);
            Assert.IsType<OpenTK.Mathematics.Vector3>(fieldValue);
            var actualColor = (OpenTK.Mathematics.Vector3)fieldValue;
            
            Assert.Equal(expectedColor, actualColor);
        }

        [Fact]
        public void DrawText_NullOrEmptyString_DoesNotThrow()
        {
            Assert.True(_isInitialized, "FontRenderer should be initialized for DrawText (null/empty).");
            Exception? ex = Record.Exception(() => FontRenderer.DrawText(null, 0, 0));
            Assert.Null(ex);
            ex = Record.Exception(() => FontRenderer.DrawText("", 0, 0));
            Assert.Null(ex);
        }

        // It's difficult to test DrawText output without visual inspection or framebuffer sampling in a unit test.
        // We assume if Initialize, GetTextWidth, GetTextHeight, and SetColor work, DrawText is likely functional.

    }

    static class GameWindowlessHelper
    {
        private static INativeWindow? _nativeWindow;
        private static IGraphicsContext? _context;

        public static void EnsureInitialized()
        {
            if (_nativeWindow == null)
            {
                try
                {
                    var nativeWindowSettings = new NativeWindowSettings()
                    {
                        Size = new Vector2i(1, 1),
                        Title = "TestWindow - Headless",
                        Flags = ContextFlags.Offscreen,
                        APIVersion = new Version(3, 3),
                        Profile = ContextProfile.Core,
                        IsVisible = false
                    };

                    _nativeWindow = new NativeWindow(nativeWindowSettings);
                    _context = _nativeWindow.Context;
                    _context.MakeCurrent();

                    Console.WriteLine("GameWindowlessHelper: Successfully created and made current an OpenGL context.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GameWindowlessHelper: Failed to initialize an OpenGL context: {ex.Message}");
                    _nativeWindow?.Dispose();
                    _nativeWindow = null;
                    _context = null;
                    throw;
                }
            }
        }

        public static void CleanUp()
        {
            _nativeWindow?.Dispose();
            _nativeWindow = null;
            _context = null;
            Console.WriteLine("GameWindowlessHelper: Cleaned up OpenGL context.");
        }
    }
}
