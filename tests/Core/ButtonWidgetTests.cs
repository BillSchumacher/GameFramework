using Xunit;
using GameFramework.UI;
using System;

namespace GameFramework.Tests.Core
{
    public class ButtonWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "testButton";
            int x = 10;
            int y = 20;
            string text = "Click Me";

            // Act
            var button = new ButtonWidget(id, x, y, text);

            // Assert
            Assert.Equal(id, button.Id);
            Assert.Equal(x, button.X);
            Assert.Equal(y, button.Y);
            Assert.Equal(text, button.Text);
            Assert.True(button.IsVisible); // From base Widget class
        }

        [Fact]
        public void Constructor_NullOrEmptyText_ThrowsArgumentException()
        {
            // Arrange
            string id = "testButton";
            int x = 10;
            int y = 20;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new ButtonWidget(id, x, y, null));
            Assert.Throws<ArgumentException>(() => new ButtonWidget(id, x, y, ""));
            Assert.Throws<ArgumentException>(() => new ButtonWidget(id, x, y, "   "));
        }

        [Fact]
        public void Click_InvokesOnClickEvent()
        {
            // Arrange
            var button = new ButtonWidget("btn1", 0, 0, "Test");
            bool eventFired = false;
            button.OnClick += () => eventFired = true;

            // Act
            button.Click();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void Click_NoSubscribers_DoesNotThrow()
        {
            // Arrange
            var button = new ButtonWidget("btn1", 0, 0, "Test");

            // Act
            var ex = Record.Exception(() => button.Click());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void Draw_WhenVisible_PrintsDrawingTextWithButtonText()
        {
            // Arrange
            var button = new ButtonWidget("btnDraw", 5, 5, "DrawButton");
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            button.Show(); // Ensure it's visible
            button.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Drawing ButtonWidget {button.Id} with text \"{button.Text}\" at ({button.X}, {button.Y})", output);
            
            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        [Fact]
        public void Draw_WhenNotVisible_DoesNotPrint()
        {
            // Arrange
            var button = new ButtonWidget("btnDrawHidden", 5, 5, "HiddenButton");
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            button.Hide(); // Ensure it's not visible
            button.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Empty(output.Trim());

            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
    }
}
