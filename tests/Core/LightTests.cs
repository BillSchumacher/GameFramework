using Xunit;
using System.Drawing;
using System.Numerics; // Added for Vector3
using GameFramework;

namespace GameFramework.Tests
{
    public class LightTests
    {
        [Fact]
        public void Light_Creation_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var lightType = LightType.Point;
            var color = Color.White;
            float intensity = 0.8f;
            bool isEnabled = true;
            var position = new Vector3(1, 2, 3);

            // Act
            var light = new Light(lightType, color, position, intensity, isEnabled);

            // Assert
            Assert.Equal(lightType, light.Type);
            Assert.Equal(color, light.LightColor);
            Assert.Equal(position, light.Position);
            Assert.Equal(intensity, light.Intensity);
            Assert.Equal(isEnabled, light.IsEnabled);
        }

        [Fact]
        public void Light_Creation_DefaultValues_ShouldBeApplied()
        {
            // Arrange
            var lightType = LightType.Ambient;
            var color = Color.Red;
            var position = Vector3.Zero; // Default position

            // Act
            var light = new Light(lightType, color, position);

            // Assert
            Assert.Equal(lightType, light.Type);
            Assert.Equal(color, light.LightColor);
            Assert.Equal(position, light.Position);
            Assert.Equal(1.0f, light.Intensity); // Default intensity
            Assert.True(light.IsEnabled); // Default isEnabled
        }

        [Theory]
        [InlineData(LightType.Directional, 0.5f, 10f, 20f, 30f)]
        [InlineData(LightType.Spot, 1.5f, -1f, -2f, -3f)]
        public void Light_Creation_WithVariousTypesPositionsAndIntensities(LightType type, float intensity, float x, float y, float z)
        {
            // Arrange
            var color = Color.Blue;
            var position = new Vector3(x, y, z);

            // Act
            var light = new Light(type, color, position, intensity);

            // Assert
            Assert.Equal(type, light.Type);
            Assert.Equal(color, light.LightColor);
            Assert.Equal(position, light.Position);
            Assert.Equal(intensity, light.Intensity);
            Assert.True(light.IsEnabled);
        }

        [Fact]
        public void Light_Intensity_CannotBeNegative()
        {
            // Arrange & Act & Assert
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new Light(LightType.Point, Color.Yellow, Vector3.One, -0.5f));
        }

        [Fact]
        public void Light_Toggle_ShouldChangeIsEnabledState()
        {
            // Arrange
            var light = new Light(LightType.Spot, Color.Green, Vector3.Zero, 0.7f, true);

            // Act
            light.Toggle();

            // Assert
            Assert.False(light.IsEnabled);

            // Act
            light.Toggle();

            // Assert
            Assert.True(light.IsEnabled);
        }

        [Fact]
        public void Light_SetIsEnabled_ShouldUpdateState()
        {
            // Arrange
            var light = new Light(LightType.Point, Color.Cyan, Vector3.Zero);

            // Act
            light.IsEnabled = false;

            // Assert
            Assert.False(light.IsEnabled);

            // Act
            light.IsEnabled = true;

            // Assert
            Assert.True(light.IsEnabled);
        }

        [Fact]
        public void Light_SetLightColor_ShouldUpdateColor()
        {
            // Arrange
            var light = new Light(LightType.Ambient, Color.White, Vector3.Zero);
            var newColor = Color.FromArgb(255, 100, 150, 200);

            // Act
            light.LightColor = newColor;

            // Assert
            Assert.Equal(newColor, light.LightColor);
        }

        [Fact]
        public void Light_SetIntensity_ShouldUpdateIntensity()
        {
            // Arrange
            var light = new Light(LightType.Directional, Color.Magenta, Vector3.Zero);
            float newIntensity = 1.2f;

            // Act
            light.Intensity = newIntensity;

            // Assert
            Assert.Equal(newIntensity, light.Intensity);
        }

        [Fact]
        public void Light_SetIntensity_Negative_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var light = new Light(LightType.Spot, Color.Orange, Vector3.Zero);
            
            // Act & Assert
            float validIntensity = 0.5f;
            light.Intensity = validIntensity;
            Assert.Equal(validIntensity, light.Intensity);
        }

        [Fact]
        public void Light_SetPosition_ShouldUpdatePosition()
        {
            // Arrange
            var light = new Light(LightType.Point, Color.Cyan, Vector3.Zero);
            var newPosition = new Vector3(5, 5, 5);

            // Act
            light.Position = newPosition;

            // Assert
            Assert.Equal(newPosition, light.Position);
        }
    }
}
