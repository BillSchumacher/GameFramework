using Xunit;
using GameFramework;
using System.Collections.Generic;
using Moq;

namespace GameFramework.Tests.Core
{
    public class PanelWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "testPanel";
            int x = 5;
            int y = 10;
            int width = 100;
            int height = 50;

            // Act
            var panel = new PanelWidget(id, x, y, width, height);

            // Assert
            Assert.Equal(id, panel.Id);
            Assert.Equal(x, panel.X);
            Assert.Equal(y, panel.Y);
            Assert.Equal(width, panel.Width);
            Assert.Equal(height, panel.Height);
            Assert.True(panel.IsVisible);
            Assert.Empty(panel.Children);
        }

        [Theory]
        [InlineData(0, 50)]
        [InlineData(-1, 50)]
        [InlineData(50, 0)]
        [InlineData(50, -1)]
        public void Constructor_InvalidWidthOrHeight_ThrowsArgumentOutOfRangeException(int width, int height)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PanelWidget("invalidPanel", 0, 0, width, height));
        }

        [Fact]
        public void AddChild_AddsWidgetToChildren()
        {
            // Arrange
            var panel = new PanelWidget("parentPanel", 0, 0, 200, 100);
            var childButton = new ButtonWidget("childBtn", 10, 10, "Child");

            // Act
            panel.AddChild(childButton);

            // Assert
            Assert.Single(panel.Children);
            Assert.Contains(childButton, panel.Children);
        }

        [Fact]
        public void AddChild_NullWidget_ThrowsArgumentNullException()
        {
            // Arrange
            var panel = new PanelWidget("panel", 0, 0, 100, 100);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => panel.AddChild(null));
        }

        [Fact]
        public void AddChild_WidgetAlreadyHasParent_ThrowsInvalidOperationException()
        {
            // Arrange
            var panel1 = new PanelWidget("panel1", 0, 0, 100, 100);
            var panel2 = new PanelWidget("panel2", 0, 0, 100, 100);
            var childButton = new ButtonWidget("childBtn", 10, 10, "Child");
            panel1.AddChild(childButton); // childButton now has panel1 as parent

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => panel2.AddChild(childButton));
        }

        [Fact]
        public void RemoveChild_RemovesWidgetFromChildren()
        {
            // Arrange
            var panel = new PanelWidget("parentPanel", 0, 0, 200, 100);
            var childButton = new ButtonWidget("childBtn", 10, 10, "Child");
            panel.AddChild(childButton);

            // Act
            bool removed = panel.RemoveChild(childButton);

            // Assert
            Assert.True(removed);
            Assert.Empty(panel.Children);
        }

        [Fact]
        public void RemoveChild_WidgetNotInPanel_ReturnsFalse()
        {
            // Arrange
            var panel = new PanelWidget("parentPanel", 0, 0, 200, 100);
            var childButton = new ButtonWidget("childBtn", 10, 10, "Child");
            var otherButton = new ButtonWidget("otherBtn", 0, 0, "Other");
            panel.AddChild(childButton);

            // Act
            bool removed = panel.RemoveChild(otherButton);

            // Assert
            Assert.False(removed);
            Assert.Single(panel.Children);
        }

        [Fact]
        public void Draw_WhenVisible_DrawsPanelAndChildren()
        {
            // Arrange
            var panel = new PanelWidget("mainPanel", 1, 1, 100, 100);
            var mockChild1 = new Mock<Widget>("child1", 10, 10);
            var mockChild2 = new Mock<Widget>("child2", 20, 20);
            
            panel.AddChild(mockChild1.Object);
            panel.AddChild(mockChild2.Object);

            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            panel.Show();
            mockChild1.Object.Show(); // Ensure children are also visible for their draw to be called
            mockChild2.Object.Show();
            panel.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Drawing PanelWidget {panel.Id} at ({panel.X}, {panel.Y}) with size ({panel.Width}x{panel.Height})", output);
            mockChild1.Verify(c => c.Draw(), Times.Once());
            mockChild2.Verify(c => c.Draw(), Times.Once());

            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        [Fact]
        public void Draw_WhenNotVisible_DoesNotDrawPanelOrChildren()
        {
            // Arrange
            var panel = new PanelWidget("hiddenPanel", 1, 1, 100, 100);
            var mockChild = new Mock<Widget>("childHidden", 10, 10);
            panel.AddChild(mockChild.Object);

            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            panel.Hide();
            panel.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Empty(output.Trim());
            mockChild.Verify(c => c.Draw(), Times.Never());

            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
    }
}
