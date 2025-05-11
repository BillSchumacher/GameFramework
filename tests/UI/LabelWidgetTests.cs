using Xunit;
using GameFramework.UI;
using OpenTK.Mathematics;
using System.IO; // Required for Path, File
using System; // Required for Exception, IDisposable, Action
using System.Reflection; // Required for FieldInfo, BindingFlags

namespace GameFramework.Tests.UI
{
    public class LabelWidgetTests : IDisposable
    {
        private const string TestFontPath = "TestAssets/arial.ttf"; // Relative to execution
        private const float TestFontSize = 16f;
        private static bool _isFontRendererInitialized = false;
        private static object _initLock = new object();

        public LabelWidgetTests()
        {
            lock (_initLock) // Ensure thread-safe initialization for parallel tests
            {
                if (!_isFontRendererInitialized)
                {
                    try
                    {
                        GameWindowlessHelper.EnsureInitialized(); // Ensures OpenGL context
                        if (!File.Exists(TestFontPath))
                        {
                            throw new FileNotFoundException($"Test font file not found at {Path.GetFullPath(TestFontPath)}. Ensure it is copied to the output directory via GameFramework.Tests.csproj.");
                        }
                        FontRenderer.Initialize(TestFontPath, TestFontSize);
                        FontRenderer.ScreenWidth = 800; // Set default screen size for tests
                        FontRenderer.ScreenHeight = 600;
                        FontRenderer.UpdateProjectionMatrix();
                        _isFontRendererInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Test constructor FontRenderer.Initialize failed: {ex.Message}. LabelWidget tests might not run as expected.");
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
        public void Constructor_Default_InitializesProperties()
        {
            var label = new LabelWidget();
            Assert.Equal("default_label_id", label.Id);
            Assert.Equal(0, label.X);
            Assert.Equal(0, label.Y);
            Assert.Equal("Default Text", label.Text);
            Assert.Equal(new Vector3(1.0f, 1.0f, 1.0f), label.TextColor); // Default white
            Assert.True(label.IsVisible);
        }

        [Fact]
        public void Constructor_Parameterized_InitializesPropertiesCorrectly()
        {
            var textColor = new Vector3(0.1f, 0.2f, 0.3f);
            var label = new LabelWidget("testLabel", 10, 20, "Hello Test", textColor);

            Assert.Equal("testLabel", label.Id);
            Assert.Equal(10, label.X);
            Assert.Equal(20, label.Y);
            Assert.Equal("Hello Test", label.Text);
            Assert.Equal(textColor, label.TextColor);
            Assert.True(label.IsVisible);
        }

        [Fact]
        public void Constructor_Parameterized_NullTextDefaultsToEmpty()
        {
            var label = new LabelWidget("testLabel", 0, 0, null);
            Assert.Equal(string.Empty, label.Text);
        }

        [Fact]
        public void Constructor_Parameterized_NullColorDefaultsToWhite()
        {
            var label = new LabelWidget("testLabel", 0, 0, "Some Text", null);
            Assert.Equal(new Vector3(1.0f, 1.0f, 1.0f), label.TextColor);
        }

        [Fact]
        public void Text_SetProperty_UpdatesValue()
        {
            var label = new LabelWidget { Text = "Initial" };
            label.Text = "Updated Text";
            Assert.Equal("Updated Text", label.Text);
        }

        [Fact]
        public void TextColor_SetProperty_UpdatesValue()
        {
            var label = new LabelWidget();
            var newColor = new Vector3(0.5f, 0.5f, 0.5f);
            label.TextColor = newColor;
            Assert.Equal(newColor, label.TextColor);
        }

        // To properly test Draw, we'd need to mock FontRenderer or inspect OpenGL calls.
        // For now, we'll test that it attempts to use FontRenderer correctly.
        // We can verify that FontRenderer.SetColor is called with the label's color.

        [Fact]
        public void Draw_WhenVisibleAndTextNotEmpty_CallsFontRendererSetColorAndDrawText()
        {
            Assert.True(_isFontRendererInitialized, "FontRenderer must be initialized for this test.");

            var label = new LabelWidget("drawTestLabel", 50, 60, "Draw Me", new Vector3(0.2f, 0.4f, 0.6f));
            label.IsVisible = true;

            // We can't directly check GL calls easily in unit tests without a more complex setup.
            // However, we can verify FontRenderer's internal state for color as a proxy.
            // This assumes FontRenderer.DrawText doesn't change _currentColor itself.

            label.Draw(); // This should call FontRenderer.SetColor and FontRenderer.DrawText

            // Check FontRenderer._currentColor to see if SetColor was called with label's color
            FieldInfo? colorField = typeof(FontRenderer).GetField("_currentColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(colorField);
            object? fieldValue = colorField.GetValue(null);
            Assert.IsType<OpenTK.Mathematics.Vector3>(fieldValue);
            var actualFontRendererColor = (OpenTK.Mathematics.Vector3)fieldValue;

            Assert.Equal(label.TextColor, actualFontRendererColor);

            // A more robust test would involve mocking or a test-specific FontRenderer hook.
            // For now, this provides some confidence.
        }

        [Fact]
        public void Draw_WhenNotVisible_DoesNotAttemptToDraw()
        {
            Assert.True(_isFontRendererInitialized, "FontRenderer must be initialized for this test.");
            var label = new LabelWidget("drawTestNotVisible", 0, 0, "Hidden Text", new Vector3(1,0,0));
            label.IsVisible = false;

            var initialColor = new Vector3(0.9f,0.8f,0.7f); // Set a distinct color
            FontRenderer.SetColor(initialColor.X, initialColor.Y, initialColor.Z);

            label.Draw(); // Should not call FontRenderer.SetColor or DrawText

            FieldInfo? colorField = typeof(FontRenderer).GetField("_currentColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(colorField);
            object? fieldValue = colorField.GetValue(null);
            Assert.IsType<OpenTK.Mathematics.Vector3>(fieldValue);
            var actualFontRendererColor = (OpenTK.Mathematics.Vector3)fieldValue;

            // Color should remain what it was before the Draw call
            Assert.Equal(initialColor, actualFontRendererColor);
        }

        [Fact]
        public void Draw_WhenTextIsNull_DoesNotAttemptToDraw()
        {
            Assert.True(_isFontRendererInitialized, "FontRenderer must be initialized for this test.");
            var label = new LabelWidget("drawTestNullText", 0, 0, null, new Vector3(1,0,0));
            label.IsVisible = true;

            var initialColor = new Vector3(0.1f,0.8f,0.2f); // Set a distinct color
            FontRenderer.SetColor(initialColor.X, initialColor.Y, initialColor.Z);

            label.Draw();

            FieldInfo? colorField = typeof(FontRenderer).GetField("_currentColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(colorField);
            object? fieldValue = colorField.GetValue(null);
            Assert.IsType<OpenTK.Mathematics.Vector3>(fieldValue);
            var actualFontRendererColor = (OpenTK.Mathematics.Vector3)fieldValue;
            
            Assert.Equal(initialColor, actualFontRendererColor);
        }

        [Fact]
        public void Draw_WhenTextIsEmpty_DoesNotAttemptToDraw()
        {
            Assert.True(_isFontRendererInitialized, "FontRenderer must be initialized for this test.");
            var label = new LabelWidget("drawTestEmptyText", 0, 0, "", new Vector3(1,0,0));
            label.IsVisible = true;

            var initialColor = new Vector3(0.3f,0.3f,0.9f); // Set a distinct color
            FontRenderer.SetColor(initialColor.X, initialColor.Y, initialColor.Z);
            
            label.Draw();

            FieldInfo? colorField = typeof(FontRenderer).GetField("_currentColor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(colorField);
            object? fieldValue = colorField.GetValue(null);
            Assert.IsType<OpenTK.Mathematics.Vector3>(fieldValue);
            var actualFontRendererColor = (OpenTK.Mathematics.Vector3)fieldValue;

            Assert.Equal(initialColor, actualFontRendererColor);
        }
    }
}
