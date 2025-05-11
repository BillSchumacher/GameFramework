using Xunit;
using GameFramework.UI;
using System;

namespace GameFramework.Tests.Core
{
    public class TextFieldWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "testTextField";
            int x = 10;
            int y = 20;
            string initialText = "Hello";
            int maxLength = 100;

            // Act
            var textField = new TextFieldWidget(id, x, y, initialText, maxLength);

            // Assert
            Assert.Equal(id, textField.Id);
            Assert.Equal(x, textField.X);
            Assert.Equal(y, textField.Y);
            Assert.Equal(initialText, textField.Text);
            Assert.Equal(maxLength, textField.MaxLength);
            Assert.True(textField.IsVisible);
            Assert.False(textField.IsReadOnly);
        }

        [Fact]
        public void Constructor_NullInitialText_SetsToEmptyString()
        {
            // Arrange
            string id = "testTextField";
            int x = 10;
            int y = 20;

            // Act
            var textField = new TextFieldWidget(id, x, y, null);

            // Assert
            Assert.Equal(string.Empty, textField.Text);
        }

        [Fact]
        public void Constructor_MaxLengthZeroOrNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            string id = "testTextField";
            int x = 10;
            int y = 20;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new TextFieldWidget(id, x, y, "text", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TextFieldWidget(id, x, y, "text", -1));
        }

        [Fact]
        public void SetText_UpdatesText_And_InvokesEvent_WhenNotReadOnly()
        {
            // Arrange
            var textField = new TextFieldWidget("tf1", 0, 0, "Initial");
            string newText = "Updated Text";
            bool eventFired = false;
            string eventText = null;
            textField.OnTextChanged += (text) =>
            {
                eventFired = true;
                eventText = text;
            };

            // Act
            textField.SetText(newText);

            // Assert
            Assert.Equal(newText, textField.Text);
            Assert.True(eventFired);
            Assert.Equal(newText, eventText);
        }

        [Fact]
        public void SetText_WhenReadOnly_DoesNotUpdateText_And_DoesNotInvokeEvent()
        {
            // Arrange
            string initialText = "Initial ReadOnly";
            var textField = new TextFieldWidget("tfReadOnly", 0, 0, initialText);
            textField.IsReadOnly = true;
            string newText = "Attempted Update";
            bool eventFired = false;
            textField.OnTextChanged += (text) => eventFired = true;

            // Act
            textField.SetText(newText);

            // Assert
            Assert.Equal(initialText, textField.Text); // Text should not change
            Assert.False(eventFired); // Event should not fire
        }

        [Fact]
        public void SetText_ExceedsMaxLength_TruncatesText_And_InvokesEvent()
        {
            // Arrange
            int maxLength = 5;
            var textField = new TextFieldWidget("tfMax", 0, 0, "", maxLength);
            string longText = "TooLongText";
            string expectedText = "TooLo";
            bool eventFired = false;
            string eventText = null;
            textField.OnTextChanged += (text) =>
            {
                eventFired = true;
                eventText = text;
            };

            // Act
            textField.SetText(longText);

            // Assert
            Assert.Equal(expectedText, textField.Text);
            Assert.True(eventFired);
            Assert.Equal(expectedText, eventText);
        }

        [Fact]
        public void SetText_NullText_SetsToEmptyString_And_InvokesEvent()
        {
            // Arrange
            var textField = new TextFieldWidget("tfNull", 0, 0, "Initial");
            bool eventFired = false;
            string eventText = null;
            textField.OnTextChanged += (text) =>
            {
                eventFired = true;
                eventText = text;
            };

            // Act
            textField.SetText(null);

            // Assert
            Assert.Equal(string.Empty, textField.Text);
            Assert.True(eventFired);
            Assert.Equal(string.Empty, eventText);
        }

        [Fact]
        public void Draw_WhenVisible_PrintsDrawingTextWithCurrentText()
        {
            // Arrange
            var textField = new TextFieldWidget("tfDraw", 5, 5, "Sample Text");
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            textField.Show();
            textField.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Drawing TextFieldWidget {textField.Id} with text \"{textField.Text}\" at ({textField.X}, {textField.Y})", output);
            
            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        [Fact]
        public void Draw_WhenNotVisible_DoesNotPrint()
        {
            // Arrange
            var textField = new TextFieldWidget("tfDrawHidden", 5, 5, "Hidden Text");
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            textField.Hide();
            textField.Draw();
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
