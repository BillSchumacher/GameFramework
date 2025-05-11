using Xunit;
using GameFramework;
using GameFramework.UI; // Added for UI elements
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GameFramework.Tests.Core
{
    // Test-specific concrete class to instantiate Widget for UserInterface testing
    internal class TestableWidget : Widget
    {
        public bool WasClicked { get; private set; }
        public bool HandleClick { get; set; } // Determines if this widget will "handle" the click
        public MouseButton ClickButton { get; private set; }

        public TestableWidget(string id, int x, int y, int width, int height, bool handle = true)
            : base(id, x, y, AnchorPoint.Manual, 0, 0) // Using manual anchor for direct X, Y
        {
            // Manually set X, Y, Width, Height as UpdateActualPosition won't be called in these simple tests
            // For these tests, we will assume X, Y are the top-left coordinates as set by AnchorPoint.Manual.
            base.WidgetWidth = width;
            base.WidgetHeight = height;
            HandleClick = handle;
        }

        public override void Draw()
        {
            // No-op
        }

        public override bool OnMouseDown(float mouseX, float mouseY, MouseButton button)
        {
            WasClicked = true;
            ClickButton = button;
            return HandleClick;
        }
    }

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
            var foundWidget = ui.GetWidgetById("testWidget"); // Renamed from GetWidget

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
            var foundWidget = ui.GetWidgetById("nonExistentWidget"); // Renamed from GetWidget

            // Assert
            Assert.Null(foundWidget);
        }

        [Fact]
        public void UserInterface_GetWidget_NullOrEmptyId_ShouldThrowArgumentException()
        {
            // Arrange
            var ui = new UserInterface();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ui.GetWidgetById(null!)); // Renamed from GetWidget
            Assert.Throws<ArgumentException>(() => ui.GetWidgetById("")); // Renamed from GetWidget
            Assert.Throws<ArgumentException>(() => ui.GetWidgetById("   ")); // Renamed from GetWidget
        }

        // Mocking Draw method for testing purposes
        private class TestWidget : Widget
        {
            public int DrawCallCount { get; private set; }
            public TestWidget(string id, int x, int y) : base(id, x, y) { }
            public override void Draw() // Added override
            {
                base.Draw(); // Call base.Draw() to respect IsVisible or other base logic
                if (IsVisible) // Redundant if base.Draw() handles IsVisible, but good for clarity here
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

        [Fact]
        public void HandleMouseDown_SingleWidget_Hit_CallsOnMouseDownAndStops()
        {
            // Arrange
            var ui = new UserInterface();
            var widget1 = new TestableWidget("widget1", 10, 10, 50, 50, handle: true);
            // Manually call UpdateActualPosition as it's not part of AddWidget
            widget1.UpdateActualPosition(800, 600); // Parent dimensions don't matter for Manual anchor
            ui.AddWidget(widget1);

            // Act
            ui.HandleMouseDown(20, 20, MouseButton.Left); // 20,20 is within 10,10 -> 60,60

            // Assert
            Assert.True(widget1.WasClicked);
            Assert.Equal(MouseButton.Left, widget1.ClickButton);
        }

        [Fact]
        public void HandleMouseDown_SingleWidget_Miss_DoesNotCallOnMouseDown()
        {
            // Arrange
            var ui = new UserInterface();
            var widget1 = new TestableWidget("widget1", 10, 10, 50, 50);
            widget1.UpdateActualPosition(800, 600);
            ui.AddWidget(widget1);

            // Act
            ui.HandleMouseDown(100, 100, MouseButton.Left); // 100,100 is outside 10,10 -> 60,60

            // Assert
            Assert.False(widget1.WasClicked);
        }

        [Fact]
        public void HandleMouseDown_MultipleWidgets_TopmostHit_CallsOnlyTopmostOnMouseDown()
        {
            // Arrange
            var ui = new UserInterface();
            var widgetBottom = new TestableWidget("widgetBottom", 10, 10, 50, 50, handle: true);
            var widgetTop = new TestableWidget("widgetTop", 15, 15, 50, 50, handle: true); // Overlaps, added later (drawn on top)
            
            widgetBottom.UpdateActualPosition(800, 600);
            widgetTop.UpdateActualPosition(800, 600);

            ui.AddWidget(widgetBottom);
            ui.AddWidget(widgetTop);

            // Act
            // Click at 20,20. widgetBottom is [10,10]-[60,60]. widgetTop is [15,15]-[65,65]. Both are hit.
            ui.HandleMouseDown(20, 20, MouseButton.Left); 

            // Assert
            Assert.True(widgetTop.WasClicked, "Top widget should be clicked.");
            Assert.False(widgetBottom.WasClicked, "Bottom widget should not be clicked if top handles it.");
        }
        
        [Fact]
        public void HandleMouseDown_MultipleWidgets_TopmostHit_DoesNotHandle_NextWidgetHitAndHandles()
        {
            // Arrange
            var ui = new UserInterface();
            var widgetBottomHandles = new TestableWidget("widgetBottomHandles", 10, 10, 50, 50, handle: true);
            var widgetTopNoHandle = new TestableWidget("widgetTopNoHandle", 15, 15, 50, 50, handle: false); // Overlaps, doesn't handle click

            widgetBottomHandles.UpdateActualPosition(800, 600);
            widgetTopNoHandle.UpdateActualPosition(800, 600);

            ui.AddWidget(widgetBottomHandles);
            ui.AddWidget(widgetTopNoHandle);

            // Act
            // Click at 20,20. widgetBottomHandles is [10,10]-[60,60]. widgetTopNoHandle is [15,15]-[65,65]. Both are hit.
            ui.HandleMouseDown(20, 20, MouseButton.Left); 

            // Assert
            Assert.True(widgetTopNoHandle.WasClicked, "Top widget (no handle) should still register OnMouseDown call.");
            Assert.True(widgetBottomHandles.WasClicked, "Bottom widget should be clicked as top one didn't handle it.");
        }

        [Fact]
        public void HandleMouseDown_InvisibleWidget_NotClicked()
        {
            // Arrange
            var ui = new UserInterface();
            var widget1 = new TestableWidget("widget1", 10, 10, 50, 50);
            widget1.IsVisible = false;
            widget1.UpdateActualPosition(800, 600);
            ui.AddWidget(widget1);

            // Act
            ui.HandleMouseDown(20, 20, MouseButton.Left);

            // Assert
            Assert.False(widget1.WasClicked);
        }

        [Fact]
        public void HandleMouseDown_NoWidgets_DoesNothing()
        {
            // Arrange
            var ui = new UserInterface();

            // Act & Assert (should not throw)
            ui.HandleMouseDown(20, 20, MouseButton.Left);
            // No explicit assert needed, test passes if no exception occurs
        }
    }
}
