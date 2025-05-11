using Xunit;
using GameFramework;
using GameFramework.UI; // Added for UI elements
using System;
using System.Linq;

namespace GameFramework.Tests
{
    public class UserInterfaceTests
    {
        [Fact]
        public void UserInterface_Creation_ShouldInitializeEmptyWidgetList()
        {
            // Arrange & Act
            var ui = new UserInterface();

            // Assert
            Assert.Empty(ui.GetWidgets());
        }

        [Fact]
        public void UserInterface_AddWidget_ShouldAddWidgetToList()
        {
            // Arrange
            var ui = new UserInterface();
            var widget = new Widget("testWidget", 0, 0);

            // Act
            ui.AddWidget(widget);

            // Assert
            Assert.Single(ui.GetWidgets());
            Assert.Contains(widget, ui.GetWidgets());
        }

        [Fact]
        public void UserInterface_AddWidget_NullWidget_ShouldThrowArgumentNullException()
        {
            // Arrange
            var ui = new UserInterface();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ui.AddWidget(null!));
        }

        [Fact]
        public void UserInterface_AddWidget_DuplicateId_ShouldThrowArgumentException()
        {
            // Arrange
            var ui = new UserInterface();
            var widget1 = new Widget("testWidget", 0, 0);
            var widget2 = new Widget("testWidget", 10, 10); // Same ID
            ui.AddWidget(widget1);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ui.AddWidget(widget2));
        }

        [Fact]
        public void UserInterface_RemoveWidget_ShouldRemoveWidgetFromList()
        {
            // Arrange
            var ui = new UserInterface();
            var widget = new Widget("testWidget", 0, 0);
            ui.AddWidget(widget);

            // Act
            bool removed = ui.RemoveWidget("testWidget");

            // Assert
            Assert.True(removed);
            Assert.Empty(ui.GetWidgets());
        }

        [Fact]
        public void UserInterface_RemoveWidget_NonExistentWidget_ShouldReturnFalse()
        {
            // Arrange
            var ui = new UserInterface();
            var widget = new Widget("testWidget", 0, 0);
            ui.AddWidget(widget);

            // Act
            bool removed = ui.RemoveWidget("nonExistentWidget");

            // Assert
            Assert.False(removed);
            Assert.Single(ui.GetWidgets());
        }

        [Fact]
        public void UserInterface_RemoveWidget_NullOrEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var ui = new UserInterface();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ui.RemoveWidget(null!));
            Assert.Throws<ArgumentException>(() => ui.RemoveWidget(""));
            Assert.Throws<ArgumentException>(() => ui.RemoveWidget("   "));
        }

        [Fact]
        public void UserInterface_GetWidget_ShouldReturnWidgetById()
        {
            // Arrange
            var ui = new UserInterface();
            var widget = new Widget("testWidget", 0, 0);
            ui.AddWidget(widget);

            // Act
            var foundWidget = ui.GetWidget("testWidget");

            // Assert
            Assert.NotNull(foundWidget);
            Assert.Same(widget, foundWidget);
        }

        [Fact]
        public void UserInterface_GetWidget_NonExistentWidget_ShouldReturnNull()
        {
            // Arrange
            var ui = new UserInterface();

            // Act
            var foundWidget = ui.GetWidget("nonExistentWidget");

            // Assert
            Assert.Null(foundWidget);
        }

        [Fact]
        public void UserInterface_GetWidget_NullOrEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var ui = new UserInterface();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ui.GetWidget(null!));
            Assert.Throws<ArgumentException>(() => ui.GetWidget(""));
            Assert.Throws<ArgumentException>(() => ui.GetWidget("   "));
        }

        // Mocking Draw method for testing purposes
        private class TestWidget : Widget
        {
            public int DrawCallCount { get; private set; }
            public TestWidget(string id, int x, int y) : base(id, x, y) { }
            public override void Draw()
            {
                if (IsVisible)
                {
                    DrawCallCount++;
                }
            }
        }

        [Fact]
        public void UserInterface_Draw_ShouldCallDrawOnVisibleWidgetsOnly()
        {
            // Arrange
            var ui = new UserInterface();
            var visibleWidget = new TestWidget("visibleWidget", 0, 0);
            var hiddenWidget = new TestWidget("hiddenWidget", 10, 10);
            hiddenWidget.Hide();

            ui.AddWidget(visibleWidget);
            ui.AddWidget(hiddenWidget);

            // Act
            ui.Draw();

            // Assert
            Assert.Equal(1, visibleWidget.DrawCallCount);
            Assert.Equal(0, hiddenWidget.DrawCallCount);
        }

        [Fact]
        public void UserInterface_GetWidgets_ShouldReturnReadOnlyListOfWidgets()
        {
            // Arrange
            var ui = new UserInterface();
            var widget1 = new Widget("widget1", 0, 0);
            ui.AddWidget(widget1);

            // Act
            var widgets = ui.GetWidgets();

            // Assert
            Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<Widget>>(widgets);
            // Attempting to modify should ideally fail or not be possible if truly read-only
            // For List.AsReadOnly(), Add/Remove will throw NotSupportedException.
            Assert.Throws<NotSupportedException>(() => ((System.Collections.Generic.IList<Widget>)widgets).Add(new Widget("newWidget",0,0)));
        }
    }
}
