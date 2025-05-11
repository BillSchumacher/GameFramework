using Xunit;
using GameFramework.UI;
using System;
using Moq;

namespace GameFramework.Tests.Core
{
    public class VerticalStackPanelWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "vStack";
            int x = 5;
            int y = 10;
            int spacing = 5;

            // Act
            var vStack = new VerticalStackPanelWidget(id, x, y, spacing);

            // Assert
            Assert.Equal(id, vStack.Id);
            Assert.Equal(x, vStack.X);
            Assert.Equal(y, vStack.Y);
            Assert.Equal(spacing, vStack.Spacing);
            Assert.True(vStack.IsVisible);
            Assert.Empty(vStack.Children);
        }

        [Fact]
        public void AddChild_AddsWidget_And_SetsChildPosition()
        {
            // Arrange
            var vStack = new VerticalStackPanelWidget("vStack1", 10, 20, 5);
            var child1 = new Mock<Widget>("vChild1", 0, 0);
            child1.SetupProperty(c => c.X);
            child1.SetupProperty(c => c.Y);
            var child1AsPanel = child1.As<PanelWidget>();
            if (child1AsPanel != null) child1AsPanel.SetupGet(p => p.Height).Returns(30); // Mock Height for PanelWidget

            var child2 = new Mock<Widget>("vChild2", 0, 0);
            child2.SetupProperty(c => c.X);
            child2.SetupProperty(c => c.Y);
            var child2AsPanel = child2.As<PanelWidget>();
            if (child2AsPanel != null) child2AsPanel.SetupGet(p => p.Height).Returns(20);

            // Act
            vStack.AddChild(child1.Object);
            vStack.AddChild(child2.Object);

            // Assert
            Assert.Equal(2, vStack.Children.Count());
            Assert.Contains(child1.Object, vStack.Children);
            Assert.Contains(child2.Object, vStack.Children);

            // Child1 position
            child1.Verify(c => c.SetPosition(vStack.X, vStack.Y), Times.Once);

            // Child2 position - depends on child1's height
            // Assuming GetEffectiveHeight for child1 (if PanelWidget) returns 30
            // child2.Y = vStack.Y + 30 (child1.Height) + 5 (spacing) = 20 + 30 + 5 = 55
            // This requires VerticalStackPanel to use GetEffectiveHeight.
            // The mock setup for PanelWidget's Height is for this purpose.
            // If child1 is not a PanelWidget, GetEffectiveHeight might return a default (e.g., 20).
            int child1EffectiveHeight = (child1.Object is PanelWidget p1) ? p1.Height : 20; // Simplified from GetEffectiveHeight
            if (child1.Object is ButtonWidget || child1.Object is TextFieldWidget || child1.Object is CheckboxWidget) child1EffectiveHeight = 20;
            else if (!(child1.Object is PanelWidget)) child1EffectiveHeight = 0; // Default for unknown non-panel widgets
            
            child2.Verify(c => c.SetPosition(vStack.X, vStack.Y + child1EffectiveHeight + vStack.Spacing), Times.Once);
        }

        [Fact]
        public void Draw_WhenVisible_DrawsSelfAndChildren()
        {
            // Arrange
            var vStack = new VerticalStackPanelWidget("vStackDraw", 1, 1, 5);
            var mockChild1 = new Mock<Widget>("vc1", 0, 0);
            var mockChild2 = new Mock<Widget>("vc2", 0, 0);

            vStack.AddChild(mockChild1.Object);
            vStack.AddChild(mockChild2.Object);

            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            vStack.Show();
            mockChild1.Object.Show();
            mockChild2.Object.Show();
            vStack.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Drawing VerticalStackPanelWidget {vStack.Id} at ({vStack.X}, {vStack.Y}) with spacing {vStack.Spacing}", output);
            mockChild1.Verify(c => c.Draw(), Times.Once());
            mockChild2.Verify(c => c.Draw(), Times.Once());

            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }

        [Fact]
        public void RecalculateLayout_UpdatesChildrenPositionsCorrectly()
        {
            // Arrange
            var vStack = new VerticalStackPanelWidget("vStackLayout", 10, 20, 5);
            var childButton = new ButtonWidget("btnV", 0, 0, "ButtonV"); // Effective height e.g. 20
            var childPanel = new PanelWidget("panelV", 0, 0, 50, 30);   // Effective height 30

            vStack.AddChild(childButton);
            vStack.AddChild(childPanel);

            // Act: AddChild triggers RecalculateLayout

            // Assert
            // ChildButton (effective height 20 from StackPanelWidget.GetEffectiveHeight)
            Assert.Equal(vStack.X, childButton.X);
            Assert.Equal(vStack.Y, childButton.Y);

            // ChildPanel (effective height 30)
            // Expected Y = vStack.Y + childButton.EffectiveHeight + vStack.Spacing
            // Expected Y = 20 + 20 + 5 = 45
            Assert.Equal(vStack.X, childPanel.X);
            Assert.Equal(vStack.Y + 20 + vStack.Spacing, childPanel.Y);

            // Add another child to see continued stacking
            var childCheckbox = new CheckboxWidget("chkV", 0, 0, "CheckV"); // Effective height e.g. 20
            vStack.AddChild(childCheckbox);

            // ChildCheckbox
            // Expected Y = childPanel.Y + childPanel.Height + vStack.Spacing
            // Expected Y = (20 + 20 + 5) + 30 + 5 = 45 + 30 + 5 = 80
            Assert.Equal(vStack.X, childCheckbox.X);
            Assert.Equal(vStack.Y + 20 + vStack.Spacing + childPanel.Height + vStack.Spacing, childCheckbox.Y);
        }
    }
}
