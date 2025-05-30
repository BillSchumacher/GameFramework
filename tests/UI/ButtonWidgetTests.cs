using Xunit;
using GameFramework.UI;
using OpenTK.Mathematics; // For Vector3
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameFramework.Tests.UI
{
    public class ButtonWidgetTests
    {
        [Fact]
        public void Constructor_Default_SetsDefaultColors()
        {
            var button = new ButtonWidget(0, 0, 100, 30, "Test");
            Assert.Equal(new Vector3(0.8f, 0.8f, 0.8f), button.BackgroundColor); // Default light gray
            Assert.Equal(new Vector3(0.2f, 0.2f, 0.2f), button.TextColor);     // Default dark gray
        }

        [Fact]
        public void Constructor_WithColors_SetsSpecifiedColors()
        {
            var bgColor = new Vector3(1.0f, 0.0f, 0.0f); // Red
            var textColor = new Vector3(0.0f, 1.0f, 0.0f); // Green
            var button = new ButtonWidget(0, 0, 100, 30, "Test", bgColor, textColor);
            Assert.Equal(bgColor, button.BackgroundColor);
            Assert.Equal(textColor, button.TextColor);
        }

        [Fact]
        public void Constructor_WithColorsAndSize_SetsSpecifiedColorsAndSize()
        {
            var bgColor = new Vector3(0.0f, 0.0f, 1.0f); // Blue
            var textColor = new Vector3(1.0f, 1.0f, 0.0f); // Yellow
            int x = 10, y = 20, width = 150, height = 40;
            var button = new ButtonWidget(x, y, width, height, "Test Button", bgColor, textColor);

            Assert.Equal(bgColor, button.BackgroundColor);
            Assert.Equal(textColor, button.TextColor);
            Assert.Equal(x, button.X);
            Assert.Equal(y, button.Y);
            Assert.Equal(width, button.Width);
            Assert.Equal(height, button.Height);
            Assert.Equal("Test Button", button.Text);
        }

        [Fact]
        public void ButtonWidget_Properties_CanBeSet()
        {
            var button = new ButtonWidget(0, 0, 100, 30, "Initial Text");

            // Test initial values (defaults or from constructor)
            Assert.Equal(0, button.X);
            Assert.Equal(0, button.Y);
            Assert.Equal(100, button.Width);
            Assert.Equal(30, button.Height);
            Assert.Equal("Initial Text", button.Text);
            Assert.Equal(new Vector3(0.8f, 0.8f, 0.8f), button.BackgroundColor);
            Assert.Equal(new Vector3(0.2f, 0.2f, 0.2f), button.TextColor);

            // Modify properties
            button.X = 50;
            button.Y = 60;
            button.Width = 200;
            button.Height = 50;
            button.Text = "Updated Text";
            var newBgColor = new Vector3(0.1f, 0.2f, 0.3f);
            var newTextColor = new Vector3(0.9f, 0.8f, 0.7f);
            button.BackgroundColor = newBgColor;
            button.TextColor = newTextColor;

            // Assert new values
            Assert.Equal(50, button.X);
            Assert.Equal(60, button.Y);
            Assert.Equal(200, button.Width);
            Assert.Equal(50, button.Height);
            Assert.Equal("Updated Text", button.Text);
            Assert.Equal(newBgColor, button.BackgroundColor);
            Assert.Equal(newTextColor, button.TextColor);
        }

        [Fact]
        public void OnMouseDown_LeftButton_InvokesOnClickAndReturnsTrue()
        {
            // Arrange
            var button = new ButtonWidget("testButton", "ARIAL.TTF", 16, 100, 30, "Test");
            bool onClickCalled = false;
            button.OnClick += () => { onClickCalled = true; };

            // Act
            bool result = button.OnMouseDown(10, 10, MouseButton.Left);

            // Assert
            Assert.True(onClickCalled, "OnClick event should have been invoked.");
            Assert.True(result, "OnMouseDown should return true when handling a left click.");
        }

        [Theory]
        [InlineData(MouseButton.Right)]
        [InlineData(MouseButton.Middle)]
        [InlineData(MouseButton.Button4)] // Example of another button
        public void OnMouseDown_NonLeftButton_DoesNotInvokeOnClickAndReturnsFalse(MouseButton mouseButton)
        {
            // Arrange
            var button = new ButtonWidget("testButton", "ARIAL.TTF", 16, 100, 30, "Test");
            bool onClickCalled = false;
            button.OnClick += () => { onClickCalled = true; };

            // Act
            bool result = button.OnMouseDown(10, 10, mouseButton);

            // Assert
            Assert.False(onClickCalled, "OnClick event should not have been invoked.");
            Assert.False(result, "OnMouseDown should return false for non-left clicks.");
        }

        [Fact]
        public void OnMouseDown_LeftButton_NoSubscriber_ReturnsTrue()
        {
            // Arrange
            var button = new ButtonWidget("testButton", "ARIAL.TTF", 16, 100, 30, "Test");
            // No OnClick subscriber

            // Act
            bool result = button.OnMouseDown(10, 10, MouseButton.Left);

            // Assert
            Assert.True(result, "OnMouseDown should still return true for a left click even if OnClick has no subscribers.");
        }
    }
}
