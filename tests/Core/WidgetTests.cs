using Xunit;
using GameFramework;
using System;

namespace GameFramework.Tests
{
    public class WidgetTests
    {
        [Fact]
        public void Widget_Creation_ShouldSetDefaultProperties()
        {
            // Arrange
            string widgetId = "testWidget";

            // Act
            var widget = new Widget(widgetId, 0, 0);

            // Assert
            Assert.Equal(widgetId, widget.Id);
            Assert.True(widget.IsVisible); // Default to visible
            Assert.Equal(0, widget.X);
            Assert.Equal(0, widget.Y);
        }

        [Fact]
        public void Widget_Creation_NullOrEmptyId_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new Widget(null!, 0, 0));
            Assert.Throws<ArgumentException>(() => new Widget("", 0, 0));
            Assert.Throws<ArgumentException>(() => new Widget("   ", 0, 0));
        }

        [Fact]
        public void Widget_Show_ShouldSetIsVisibleToTrue()
        {
            // Arrange
            var widget = new Widget("testWidget", 0, 0);
            widget.Hide(); // Start with hidden

            // Act
            widget.Show();

            // Assert
            Assert.True(widget.IsVisible);
        }

        [Fact]
        public void Widget_Hide_ShouldSetIsVisibleToFalse()
        {
            // Arrange
            var widget = new Widget("testWidget", 0, 0);

            // Act
            widget.Hide();

            // Assert
            Assert.False(widget.IsVisible);
        }

        [Fact]
        public void Widget_SetPosition_ShouldUpdatePosition()
        {
            // Arrange
            var widget = new Widget("testWidget", 0, 0);
            int newX = 100;
            int newY = 200;

            // Act
            widget.SetPosition(newX, newY);

            // Assert
            Assert.Equal(newX, widget.X);
            Assert.Equal(newY, widget.Y);
        }
    }
}
