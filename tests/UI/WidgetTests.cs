using Xunit;
using GameFramework.UI;

namespace GameFramework.Tests.UI
{
    // Test-specific concrete class to instantiate Widget for testing
    internal class TestWidget : Widget
    {
        public TestWidget(string id, int x, int y, int width, int height, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
            : base(id, x, y, anchor, offsetX, offsetY)
        {
            // Set dimensions directly for testing purposes
            base.WidgetWidth = width;
            base.WidgetHeight = height;
        }

        // Allow public setting for tests if needed, otherwise constructor is fine
        public void SetDimensions(int width, int height)
        {
            base.WidgetWidth = width;
            base.WidgetHeight = height;
        }

        public override void Draw()
        {
            // No-op for testing position logic
        }
    }

    public class WidgetTests
    {
        [Theory]
        [InlineData(AnchorPoint.TopLeft, 0, 0, 0, 0)] // No offset
        [InlineData(AnchorPoint.TopLeft, 10, 20, 10, 20)] // With offset
        public void UpdateActualPosition_TopLeft_CalculatesCorrectly(AnchorPoint anchor, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, 50, 30, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(800, 600);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.TopCenter, 800, 50, 0, 0, 375, 0)] // Parent: 800, Widget: 50, OffsetX: 0 -> (800/2) - (50/2) + 0 = 400 - 25 = 375
        [InlineData(AnchorPoint.TopCenter, 800, 50, 10, 5, 385, 5)] // Parent: 800, Widget: 50, OffsetX: 10 -> 375 + 10 = 385
        public void UpdateActualPosition_TopCenter_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int widgetWidth, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, 30, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, 600);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.TopRight, 800, 50, 0, 0, 750, 0)] // Parent: 800, Widget: 50, OffsetX: 0 -> 800 - 50 + 0 = 750
        [InlineData(AnchorPoint.TopRight, 800, 50, -10, 5, 740, 5)] // Parent: 800, Widget: 50, OffsetX: -10 -> 750 - 10 = 740
        public void UpdateActualPosition_TopRight_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int widgetWidth, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, 30, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, 600);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.MiddleLeft, 600, 30, 0, 0, 0, 285)] // ParentH: 600, WidgetH: 30, OffsetY: 0 -> (600/2) - (30/2) + 0 = 300 - 15 = 285
        [InlineData(AnchorPoint.MiddleLeft, 600, 30, 10, 5, 10, 290)] // ParentH: 600, WidgetH: 30, OffsetY: 5 -> 285 + 5 = 290
        public void UpdateActualPosition_MiddleLeft_CalculatesCorrectly(AnchorPoint anchor, int parentHeight, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, 50, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(800, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.MiddleCenter, 800, 600, 50, 30, 0, 0, 375, 285)] // PW:800, PH:600, WW:50, WH:30 -> X:375, Y:285
        [InlineData(AnchorPoint.MiddleCenter, 800, 600, 50, 30, 10, -5, 385, 280)]// PW:800, PH:600, WW:50, WH:30, OX:10, OY:-5 -> X:375+10=385, Y:285-5=280
        public void UpdateActualPosition_MiddleCenter_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int parentHeight, int widgetWidth, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.MiddleRight, 800, 600, 50, 30, 0, 0, 750, 285)] // PW:800,WW:50 -> X:750. PH:600,WH:30 -> Y:285
        [InlineData(AnchorPoint.MiddleRight, 800, 600, 50, 30, -10, 5, 740, 290)]// PW:800,WW:50,OX:-10 -> X:740. PH:600,WH:30,OY:5 -> Y:290
        public void UpdateActualPosition_MiddleRight_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int parentHeight, int widgetWidth, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }
        
        [Theory]
        [InlineData(AnchorPoint.BottomLeft, 600, 30, 0, 0, 0, 570)] // ParentH:600, WidgetH:30, OffsetY:0 -> 600-30+0 = 570
        [InlineData(AnchorPoint.BottomLeft, 600, 30, 10, -5, 10, 565)] // ParentH:600, WidgetH:30, OffsetY:-5 -> 570-5 = 565
        public void UpdateActualPosition_BottomLeft_CalculatesCorrectly(AnchorPoint anchor, int parentHeight, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, 50, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(800, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.BottomCenter, 800, 600, 50, 30, 0, 0, 375, 570)] // PW:800,WW:50 -> X:375. PH:600,WH:30 -> Y:570
        [InlineData(AnchorPoint.BottomCenter, 800, 600, 50, 30, 10, -5, 385, 565)]// PW:800,WW:50,OX:10 -> X:385. PH:600,WH:30,OY:-5 -> Y:565
        public void UpdateActualPosition_BottomCenter_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int parentHeight, int widgetWidth, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Theory]
        [InlineData(AnchorPoint.BottomRight, 800, 600, 50, 30, 0, 0, 750, 570)] // PW:800,WW:50 -> X:750. PH:600,WH:30 -> Y:570
        [InlineData(AnchorPoint.BottomRight, 800, 600, 50, 30, -10, -5, 740, 565)]// PW:800,WW:50,OX:-10 -> X:740. PH:600,WH:30,OY:-5 -> Y:565
        public void UpdateActualPosition_BottomRight_CalculatesCorrectly(AnchorPoint anchor, int parentWidth, int parentHeight, int widgetWidth, int widgetHeight, int offsetX, int offsetY, int expectedX, int expectedY)
        {
            var widget = new TestWidget("test", 0, 0, widgetWidth, widgetHeight, anchor, offsetX, offsetY);
            widget.UpdateActualPosition(parentWidth, parentHeight);
            Assert.Equal(expectedX, widget.X);
            Assert.Equal(expectedY, widget.Y);
        }

        [Fact]
        public void UpdateActualPosition_ManualAnchor_DoesNotChangePosition()
        {
            var widget = new TestWidget("testManual", 100, 150, 50, 30, AnchorPoint.Manual, 10, 20);
            // For Manual, UpdateActualPosition should be a no-op regarding X and Y based on parent.
            // X and Y are set in constructor or by SetPosition.
            // The current implementation of UpdateActualPosition returns early if Anchor == Manual.
            
            int initialX = widget.X;
            int initialY = widget.Y;

            widget.UpdateActualPosition(800, 600); // Call with parent dimensions

            Assert.Equal(initialX, widget.X); // Should remain 100
            Assert.Equal(initialY, widget.Y); // Should remain 150
        }
    }
}
