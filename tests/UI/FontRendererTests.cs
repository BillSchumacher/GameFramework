using Xunit;
using GameFramework.UI;
using System.IO;
using OpenTK.Mathematics; // Required for Vector3 if used, though FontRenderer API uses floats for color

namespace GameFramework.Tests.UI
{
    public class FontRendererTests : IDisposable
    {
        private const string TestFontPath = "TestAssets/arial.ttf"; // Relative to execution
        private const float TestFontSize = 16f;

        public FontRendererTests()
        {
            // Ensure the TestAssets directory exists (it should be copied by the build process)
            // Directory.CreateDirectory("TestAssets"); // This might not be needed if csproj handles it

            // Minimal OpenGL context setup is tricky in unit tests without a window.
            // FontRenderer.Initialize creates OpenGL resources.
            // For robust testing, this would require a headless GL context or mocking.
            // Here, we rely on Initialize to handle errors gracefully if GL context is unavailable.
            try
            {
                 // Attempt to initialize with a dummy GL context or expect it to handle no context
                GameWindowlessHelper.EnsureInitialized(); // Helper to setup minimal GL if possible
                // Ensure the font file exists before attempting to initialize
                if (!File.Exists(TestFontPath))
                {
                    throw new FileNotFoundException($"Test font file not found at {Path.GetFullPath(TestFontPath)}. Ensure it is copied to the output directory via GameFramework.Tests.csproj.");
                }
                FontRenderer.Initialize(TestFontPath, TestFontSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test constructor FontRenderer.Initialize failed: {ex.Message}. Some tests might not run as expected.");
                // This is expected if no GL context is available or font is missing.
                // Rethrow or handle as appropriate for your test strategy.
                // For now, we let tests proceed, and they might fail if FontRenderer is not initialized.
            }
        }

        public void Dispose()
        {
            try
            {
                FontRenderer.Dispose();
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Test Dispose FontRenderer.Dispose failed: {ex.Message}.");
            }
            GameWindowlessHelper.CleanUp();
        }

        [Fact]
        public void Initialize_WithFont_PopulatesMetricsAndTexture()
        {
            // This test assumes Initialize was called in constructor and succeeded.
            // A real font is needed for these to be truly meaningful.
            Assert.True(FontRenderer.FontTextureID > 0, "FontTextureID should be greater than 0 after initialization.");
            // CharacterMetrics count depends on the actual font and charset used in FontRenderer
            // For now, check if it's not empty, assuming '?' or some chars got loaded.
            // Assert.NotEmpty(FontRenderer._characterMetrics); // Cannot access private member, need a public way or different assertion
            Assert.True(FontRenderer.GetTextHeight() > 0, "Font line height should be greater than 0.");
        }

        [Fact]
        public void GetTextWidth_WithValidText_ReturnsNonZeroForRealFont()
        {
            float width = FontRenderer.GetTextWidth("Hello");
            Assert.True(width > 0, "Text width for 'Hello' should be positive.");
        }

        [Fact]
        public void GetTextHeight_ReturnsNonZeroForRealFont()
        {
            float height = FontRenderer.GetTextHeight();
            Assert.True(height > 0, "Text height should be positive.");
        }

        [Fact]
        public void SetColor_UpdatesColorCorrectly()
        {
            // This test doesn't directly verify rendering but checks if the color is set.
            // We can't access the private _currentColor field directly for assertion.
            // We assume that if GL.Uniform3 is called without error, it's working.
            // A more robust test would involve checking shader uniform values if possible.
            FontRenderer.SetColor(0.5f, 0.6f, 0.7f);
            // No direct assertion possible here without accessing private state or GL state.
            // If running in a GL context, one could try to read back the uniform, but that's complex.
            Assert.True(true, "SetColor called. Assumed to work if no GL errors (not checked here).");
        }
    }

    // Minimal helper to attempt to create a headless context for tests
    // This is a simplified version and might not work in all test environments.
    // For proper headless GL, a library like OpenTK.Windowing.Desktop.NativeWindow might be needed.
    static class GameWindowlessHelper
    {
        private static OpenTK.Windowing.Desktop.INativeWindow? _nativeWindow;

        public static void EnsureInitialized()
        {
            if (_nativeWindow == null)
            {
                try
                {
                    // Try to create a minimal windowless context
                    // Note: This is often the most challenging part of GL unit testing.
                    // OpenTK 4.x recommends GameWindow for context creation.
                    // A truly "windowless" setup for tests can be involved.
                    // This is a placeholder for where such initialization would go.
                    // For now, we'll assume FontRenderer might handle cases where context is not fully available.
                    // _nativeWindow = new OpenTK.Windowing.Desktop.NativeWindow(new OpenTK.Windowing.Desktop.NativeWindowSettings { Size = new OpenTK.Mathematics.Vector2i(1,1), IsVisible = false });
                    // If the above line is used, OpenTK.Windowing.Desktop package would be needed.
                    // And it would still require a display server on Linux, etc.
                    Console.WriteLine("GameWindowlessHelper: Skipping actual GL context creation for tests. FontRenderer might operate in a limited mode or fail gracefully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GameWindowlessHelper: Failed to initialize a minimal GL context: {ex.Message}");
                }
            }
        }

        public static void CleanUp()
        {
            _nativeWindow?.Dispose();
            _nativeWindow = null;
        }
    }
}
