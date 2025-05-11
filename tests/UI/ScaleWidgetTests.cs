using Xunit;
using GameFramework.UI;
using System;
using System.Text.Json;

namespace GameFramework.Tests.UI
{
    public class ScaleWidgetTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            string id = "testScale";
            float minValue = 0f;
            float maxValue = 100f;
            float initialValue = 50f;
            Orientation orientation = Orientation.Horizontal;
            int x = 10, y = 20, width = 200, height = 30;

            // Act
            var scale = new ScaleWidget(id, minValue, maxValue, initialValue, orientation, x, y, width, height);

            // Assert
            Assert.Equal(id, scale.Id);
            Assert.Equal(minValue, scale.MinValue);
            Assert.Equal(maxValue, scale.MaxValue);
            Assert.Equal(initialValue, scale.CurrentValue);
            Assert.Equal(orientation, scale.Orientation);
            Assert.Equal(x, scale.X);
            Assert.Equal(y, scale.Y);
            Assert.Equal(width, scale.WidgetWidth);
            Assert.Equal(height, scale.WidgetHeight);
            Assert.True(scale.IsVisible);
        }

        [Fact]
        public void Constructor_ThrowsArgumentOutOfRangeException_IfMinValueNotLessThanMaxValue()
        {
            // Arrange
            string id = "testScale";
            float minValue = 100f;
            float maxValue = 0f; // Invalid: maxValue <= minValue
            float initialValue = 50f;
            Orientation orientation = Orientation.Horizontal;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScaleWidget(id, minValue, maxValue, initialValue, orientation, 0, 0, 100, 20));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScaleWidget(id, 50f, 50f, initialValue, orientation, 0, 0, 100, 20)); // Equal values
        }

        [Fact]
        public void CurrentValue_ClampsToMinMax()
        {
            // Arrange
            var scale = new ScaleWidget("scale1", 0f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);

            // Act & Assert
            scale.CurrentValue = 150f;
            Assert.Equal(100f, scale.CurrentValue); // Should clamp to MaxValue

            scale.CurrentValue = -50f;
            Assert.Equal(0f, scale.CurrentValue); // Should clamp to MinValue

            scale.CurrentValue = 75f;
            Assert.Equal(75f, scale.CurrentValue); // Should set within range
        }

        [Fact]
        public void MinValue_Setter_ClampsCurrentValue()
        {
            // Arrange
            var scale = new ScaleWidget("scaleMin", 0f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);

            // Act
            scale.MinValue = 60f; // CurrentValue (50) is now less than new MinValue

            // Assert
            Assert.Equal(60f, scale.MinValue);
            Assert.Equal(60f, scale.CurrentValue); // CurrentValue should be clamped to new MinValue
        }

        [Fact]
        public void MinValue_Setter_ThrowsIfValueIsGreaterThanOrEqualToMaxValue()
        {
            // Arrange
            var scale = new ScaleWidget("scaleMinEx", 0f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => scale.MinValue = 100f); // Equal to MaxValue
            Assert.Throws<ArgumentOutOfRangeException>(() => scale.MinValue = 110f); // Greater than MaxValue
        }


        [Fact]
        public void MaxValue_Setter_ClampsCurrentValue()
        {
            // Arrange
            var scale = new ScaleWidget("scaleMax", 0f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);

            // Act
            scale.MaxValue = 40f; // CurrentValue (50) is now greater than new MaxValue

            // Assert
            Assert.Equal(40f, scale.MaxValue);
            Assert.Equal(40f, scale.CurrentValue); // CurrentValue should be clamped to new MaxValue
        }
        
        [Fact]
        public void MaxValue_Setter_ThrowsIfValueIsLessThanOrEqualToMinValue()
        {
            // Arrange
            var scale = new ScaleWidget("scaleMaxEx", 10f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => scale.MaxValue = 10f); // Equal to MinValue
            Assert.Throws<ArgumentOutOfRangeException>(() => scale.MaxValue = 0f);  // Less than MinValue
        }

        [Fact]
        public void OnValueChanged_EventTriggeredWhenCurrentValueChanges()
        {
            // Arrange
            var scale = new ScaleWidget("scaleEvent", 0f, 100f, 50f, Orientation.Horizontal, 0, 0, 100, 20);
            float newValueFromEvent = -1f;
            bool eventTriggered = false;
            scale.OnValueChanged += (newValue) => {
                newValueFromEvent = newValue;
                eventTriggered = true;
            };

            // Act
            scale.CurrentValue = 75f;

            // Assert
            Assert.True(eventTriggered);
            Assert.Equal(75f, newValueFromEvent);

            // Act: Set to same value, event should not trigger again if value hasn't changed (though current implementation might always invoke)
            // Depending on desired behavior, this part might need adjustment in ScaleWidget or test.
            // For now, we assume it triggers if set, even to the same value, due to direct invocation in setter.
            eventTriggered = false; // Reset for next check
            scale.CurrentValue = 75f;
            Assert.True(eventTriggered); // Check if it triggers again
        }

        [Fact]
        public void SerializeDeserialize_ShouldPreserveProperties()
        {
            // Arrange
            var originalScale = new ScaleWidget("scaleSerialize", 10f, 90f, 60f, Orientation.Vertical, 5, 15, 30, 150, AnchorPoint.TopRight, 2, 3);
            originalScale.IsVisible = false;

            // Act
            string json = originalScale.ToJson();
            Widget deserializedWidget = Widget.FromJson(json);

            // Assert
            Assert.NotNull(deserializedWidget);
            Assert.IsType<ScaleWidget>(deserializedWidget);
            var deserializedScale = deserializedWidget as ScaleWidget;

            Assert.Equal(originalScale.Id, deserializedScale.Id);
            Assert.Equal(originalScale.MinValue, deserializedScale.MinValue);
            Assert.Equal(originalScale.MaxValue, deserializedScale.MaxValue);
            Assert.Equal(originalScale.CurrentValue, deserializedScale.CurrentValue);
            Assert.Equal(originalScale.Orientation, deserializedScale.Orientation);
            Assert.Equal(originalScale.X, deserializedScale.X); // X, Y are set based on anchor and offsets
            Assert.Equal(originalScale.Y, deserializedScale.Y);
            Assert.Equal(originalScale.WidgetWidth, deserializedScale.WidgetWidth);
            Assert.Equal(originalScale.WidgetHeight, deserializedScale.WidgetHeight);
            Assert.Equal(originalScale.IsVisible, deserializedScale.IsVisible);
            Assert.Equal(originalScale.Anchor, deserializedScale.Anchor);
            Assert.Equal(originalScale.OffsetX, deserializedScale.OffsetX);
            Assert.Equal(originalScale.OffsetY, deserializedScale.OffsetY);
        }

        [Fact]
        public void OnMouseDown_Horizontal_UpdatesValueCorrectly()
        {
            var scale = new ScaleWidget("hScale", 0f, 100f, 0f, Orientation.Horizontal, 0, 0, 200, 20);
            bool eventHandled = false;
            float changedValue = -1;
            scale.OnValueChanged += (val) => changedValue = val;

            // Click at 25% of the width (50px / 200px)
            // Expected value = 0 + (50 / 200) * (100 - 0) = 25
            eventHandled = scale.OnMouseDown(50, 10, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.True(eventHandled);
            Assert.Equal(25f, scale.CurrentValue);
            Assert.Equal(25f, changedValue);

            // Click at 75% of the width (150px / 200px)
            // Expected value = 0 + (150 / 200) * (100 - 0) = 75
            eventHandled = scale.OnMouseDown(150, 10, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.True(eventHandled);
            Assert.Equal(75f, scale.CurrentValue);
            Assert.Equal(75f, changedValue);
        }

        [Fact]
        public void OnMouseDown_Vertical_UpdatesValueCorrectly()
        {
            var scale = new ScaleWidget("vScale", 0f, 100f, 0f, Orientation.Vertical, 0, 0, 20, 200);
            bool eventHandled = false;
            float changedValue = -1;
            scale.OnValueChanged += (val) => changedValue = val;

            // Click at 25% of the height (50px / 200px)
            // Expected value = 0 + (50 / 200) * (100 - 0) = 25
            eventHandled = scale.OnMouseDown(10, 50, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.True(eventHandled);
            Assert.Equal(25f, scale.CurrentValue);
            Assert.Equal(25f, changedValue);

            // Click at 75% of the height (150px / 200px)
            // Expected value = 0 + (150 / 200) * (100 - 0) = 75
            eventHandled = scale.OnMouseDown(10, 150, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.True(eventHandled);
            Assert.Equal(75f, scale.CurrentValue);
            Assert.Equal(75f, changedValue);
        }

        [Fact]
        public void OnMouseDown_OutsideBounds_DoesNotUpdateValue()
        {
            var scale = new ScaleWidget("boundScale", 0f, 100f, 50f, Orientation.Horizontal, 10, 10, 200, 20);
            float originalValue = scale.CurrentValue;
            bool eventHandled;
            bool valueChanged = false;
            scale.OnValueChanged += (val) => valueChanged = true;

            // Click outside X
            eventHandled = scale.OnMouseDown(5, 15, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.False(eventHandled);
            Assert.False(valueChanged);
            Assert.Equal(originalValue, scale.CurrentValue);

            // Click outside Y
            eventHandled = scale.OnMouseDown(50, 5, OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            Assert.False(eventHandled);
            Assert.False(valueChanged);
            Assert.Equal(originalValue, scale.CurrentValue);
        }
         [Fact]
        public void DefaultConstructor_InitializesWithDefaultValues()
        {
            // Act
            var scale = new ScaleWidget();

            // Assert
            Assert.Equal("default_scale", scale.Id);
            Assert.Equal(0f, scale.MinValue);
            Assert.Equal(100f, scale.MaxValue);
            Assert.Equal(50f, scale.CurrentValue); // Clamped or default initial
            Assert.Equal(Orientation.Horizontal, scale.Orientation);
            Assert.Equal(0, scale.X); // Default from base or specific default
            Assert.Equal(0, scale.Y); // Default from base or specific default
            Assert.Equal(100, scale.WidgetWidth); // Default width
            Assert.Equal(20, scale.WidgetHeight); // Default height
            Assert.True(scale.IsVisible);
            Assert.Equal(AnchorPoint.Manual, scale.Anchor); // Default from base
        }
    }
}
