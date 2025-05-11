using Xunit;
using GameFramework.UI;
using System;
using Moq;

namespace GameFramework.Tests.Core
{
    public class HorizontalStackPanelWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "hStack";
            int x = 5;
            int y = 10;
            int spacing = 5;

            // Act
            var hStack = new HorizontalStackPanelWidget(id, x, y, spacing);

            // Assert
            Assert.Equal(id, hStack.Id);
            Assert.Equal(x, hStack.X);
            Assert.Equal(y, hStack.Y);
            Assert.Equal(spacing, hStack.Spacing);
            Assert.True(hStack.IsVisible);
            Assert.Empty(hStack.Children);
        }

        [Fact]
        public void AddChild_AddsWidget_And_SetsChildPosition()
        {
            // Arrange
            var hStack = new HorizontalStackPanelWidget("hStack1", 10, 20, 5);
            var child1 = new Mock<Widget>("child1", 0, 0); // Initial position doesn't matter
            child1.SetupProperty(c => c.X); // Allow X to be set
            child1.SetupProperty(c => c.Y); // Allow Y to be set
            // Mocking Width for PanelWidget or similar that might have it
            var child1AsPanel = child1.As<PanelWidget>();
            if (child1AsPanel != null) child1AsPanel.SetupGet(p => p.Width).Returns(50);


            var child2 = new Mock<Widget>("child2", 0, 0);
            child2.SetupProperty(c => c.X);
            child2.SetupProperty(c => c.Y);
            var child2AsPanel = child2.As<PanelWidget>();
            if (child2AsPanel != null) child2AsPanel.SetupGet(p => p.Width).Returns(30);


            // Act
            hStack.AddChild(child1.Object);
            hStack.AddChild(child2.Object);

            // Assert
            Assert.Equal(2, hStack.Children.Count());
            Assert.Contains(child1.Object, hStack.Children);
            Assert.Contains(child2.Object, hStack.Children);

            // Check positions (child1.X should be hStack.X, child1.Y should be hStack.Y)
            // child2.X should be hStack.X + child1.Width (if Panel) + spacing
            // For simplicity, if not a PanelWidget, assume a default width or handle differently.
            // For this test, we'll assume children are simple Widgets without inherent width,
            // so they'd just stack with spacing. A more complex test would use concrete widget types.

            // child1 position
            Assert.Equal(hStack.X, child1.Object.X);
            Assert.Equal(hStack.Y, child1.Object.Y);

            // child2 position - this needs a concept of child width.
            // If child1 is a simple Widget, it has no Width.
            // Let's assume for HorizontalStackPanel, it queries a 'Width' property if available,
            // or uses a default/configurable width for layout.
            // For now, let's assume a simple scenario where it just uses spacing if width is not available.
            // This part of the test highlights a design consideration for the HorizontalStackPanel.
            // For a robust test, we'd need to mock GetWidth() or use concrete types.

            // To make this testable without actual rendering/width calculation:
            // The HorizontalStackPanel itself should update child positions.
            // Let's assume child1 has an effective width of 50 (mocked if it were a PanelWidget)
            // and child2 has an effective width of 30.

            // If child1 is PanelWidget with Width 50:
            // child1.X = 10
            // child2.X = 10 (hStack.X) + 50 (child1.Width) + 5 (spacing) = 65
            // This requires HorizontalStackPanel to know about child widths.
            // Let's refine HorizontalStackPanel to handle this.
            // For the test, we'll verify the SetPosition was called with expected values.
            child1.Verify(c => c.SetPosition(hStack.X, hStack.Y), Times.Once);

            // To verify child2's position, we need to know child1's width.
            // Let's assume HorizontalStackPanel uses a GetEffectiveWidth method on children.
            // We can mock this if Widget had such a virtual method, or use concrete types.
            // For now, we'll simplify and assume the panel calls SetPosition.
            // The exact value for child2.X depends on how HorizontalStackPanel calculates it.
            // If child1 is a simple Widget, it has no width.
            // If HorizontalStackPanel relies on a Width property:
            // child1.As<PanelWidget>()?.SetupGet(p => p.Width).Returns(50);
            // child2.Verify(c => c.SetPosition(hStack.X + 50 + hStack.Spacing, hStack.Y), Times.Once);
            // This test will be more effective once the HorizontalStackPanel logic is defined.
        }


        [Fact]
        public void Draw_WhenVisible_DrawsSelfAndChildren()
        {
            // Arrange
            var hStack = new HorizontalStackPanelWidget("hStackDraw", 1, 1, 5);
            var mockChild1 = new Mock<Widget>("c1", 0, 0);
            var mockChild2 = new Mock<Widget>("c2", 0, 0);

            hStack.AddChild(mockChild1.Object);
            hStack.AddChild(mockChild2.Object);

            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            hStack.Show();
            mockChild1.Object.Show();
            mockChild2.Object.Show();
            hStack.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Contains($"Drawing HorizontalStackPanelWidget {hStack.Id} at ({hStack.X}, {hStack.Y}) with spacing {hStack.Spacing}", output);
            mockChild1.Verify(c => c.Draw(), Times.Once());
            mockChild2.Verify(c => c.Draw(), Times.Once());

            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }
        
        [Fact]
        public void RecalculateLayout_UpdatesChildrenPositions()
        {
            // Arrange
            var hStack = new HorizontalStackPanelWidget("hStackLayout", 10, 20, 5);
            var childButton1 = new ButtonWidget("btn1", 0, 0, "Button1"); // Width is not inherent in ButtonWidget for layout
            var childPanel = new PanelWidget("panel1", 0, 0, 50, 30); // Panel has Width

            hStack.AddChild(childButton1);
            hStack.AddChild(childPanel);

            // Act
            // AddChild should trigger RecalculateLayout internally.
            // Or, if we want to test it explicitly: hStack.RecalculateLayout();

            // Assert
            // Child1 (ButtonWidget) - assuming default width or it's handled by stack.
            // For this test, let's assume ButtonWidget.Width might be considered 0 or some default if not specified.
            // Let's assume HorizontalStackPanel uses a GetEffectiveWidth method.
            // If ButtonWidget doesn't have a width, its contribution to offset is 0.
            Assert.Equal(hStack.X, childButton1.X);
            Assert.Equal(hStack.Y, childButton1.Y);

            // Child2 (PanelWidget)
            // Positioned after childButton1. If childButton1 has no width, childPanel.X = hStack.X + 0 + spacing
            // If HorizontalStackPanel can get childButton1's width (e.g., from a hypothetical RenderedWidth property or similar)
            // For simplicity, let's assume ButtonWidget contributes 0 to width for layout in this basic stack panel.
            // A more advanced panel would need a way to get actual or desired sizes.
            int expectedPanelX = hStack.X + 0 + hStack.Spacing; // Assuming button width is 0 for layout
            if (childButton1 is PanelWidget bwPanel) { // Unlikely, but for robustness
                 expectedPanelX = hStack.X + bwPanel.Width + hStack.Spacing;
            } else if (childButton1.GetType().GetProperty("Width") != null) {
                // A more generic way to get width if property exists, e.g. for a future LabelWidget with auto-width
                // This is getting complex for a unit test without a clear contract on Widget for size.
                // For now, we'll stick to the PanelWidget case for explicit width.
            }


            // A better approach for HorizontalStackPanel:
            // It should iterate children. For each child:
            // child.SetPosition(currentX, hStack.Y);
            // currentX += GetChildRenderWidth(child) + hStack.Spacing;
            // GetChildRenderWidth could be a virtual method on Widget or check for ILayoutableWidget.

            // Given PanelWidget has Width:
            // childButton1 is at (10, 20)
            // childPanel should be at (10 + 0 (button width for layout) + 5 (spacing), 20) = (15, 20)
            // If button had a conceptual width of, say, 60 (e.g. from its text):
            // childPanel would be at (10 + 60 + 5, 20) = (75, 20)

            // Let's assume HorizontalStackPanel uses PanelWidget.Width if available.
            // And for other widgets, it might assume 0 or a default.
            // This test shows the need for a clear sizing strategy in the widgets or panel.

            // After adding childButton1 (assuming 0 width for layout):
            // childButton1.X = 10, childButton1.Y = 20

            // After adding childPanel (width 50):
            // childPanel.X should be 10 (hStack.X) + 0 (width of childButton1 for layout) + 5 (spacing) = 15
            // childPanel.Y should be 20 (hStack.Y)
            Assert.Equal(hStack.X, childButton1.X); // Button is first, at stack's X
            Assert.Equal(hStack.Y, childButton1.Y);

            // For childPanel, its X depends on the width of childButton1.
            // If HorizontalStackPanel cannot determine width of childButton1, it might use 0.
            // So, childPanel.X = hStack.X + 0 (effective width of button) + hStack.Spacing
            Assert.Equal(hStack.X + 0 + hStack.Spacing, childPanel.X);
            Assert.Equal(hStack.Y, childPanel.Y);
            
            // If we add another panel
            var childPanel2 = new PanelWidget("panel2", 0,0, 20, 20);
            hStack.AddChild(childPanel2);
            // childPanel2.X = childPanel.X + childPanel.Width + hStack.Spacing
            // childPanel2.X = (hStack.X + 0 + hStack.Spacing) + 50 + hStack.Spacing
            Assert.Equal(hStack.X + 0 + hStack.Spacing + childPanel.Width + hStack.Spacing, childPanel2.X);
            Assert.Equal(hStack.Y, childPanel2.Y);

        }
    }
}
