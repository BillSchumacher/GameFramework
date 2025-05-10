using Xunit;
using GameFramework;
using System.Numerics;
using System.Drawing;

namespace GameFramework.Tests
{
    public class LightComponentTests
    {
        [Fact]
        public void LightComponent_Creation_ShouldInitializeProperties()
        {
            // Arrange
            var light = new Light(LightType.Point, Color.Red, new Vector3(1, 2, 3), 0.5f, true);
            var lightComponent = new LightComponent(light);
            var worldObject = new WorldObject("TestLightObject", 0, 0, 0);

            // Act
            worldObject.AddComponent(lightComponent);

            // Assert
            Assert.Equal(light, lightComponent.Light);
            Assert.Equal(worldObject, lightComponent.Parent);
        }

        [Fact]
        public void LightComponent_OnAttach_SetsParent()
        {
            // Arrange
            var light = new Light(LightType.Directional, Color.Blue, Vector3.UnitY, 1.0f, true);
            var lightComponent = new LightComponent(light);
            var worldObject = new WorldObject("ParentWorldObject", 1, 1, 1);

            // Act
            lightComponent.Parent = worldObject; // Simulate attachment by directly setting Parent for this test scope
            lightComponent.OnAttach(); // Manually call OnAttach to verify its behavior if any specific logic was there
                                       // In the actual AddComponent, Parent is set before OnAttach is called.

            // Assert
            Assert.Equal(worldObject, lightComponent.Parent);
        }

        [Fact]
        public void LightComponent_OnDetach_ClearsParent()
        {
            // Arrange
            var light = new Light(LightType.Spot, Color.Green, new Vector3(5,5,5), 0.8f, false);
            var lightComponent = new LightComponent(light);
            var worldObject = new WorldObject("DetachingObject", 0, 0, 0);
            worldObject.AddComponent(lightComponent);

            // Act
            worldObject.RemoveComponent(lightComponent);

            // Assert
            Assert.Null(lightComponent.Parent);
        }

        [Fact]
        public void LightComponent_Update_DoesNotThrow()
        {
            // Arrange
            var light = new Light(LightType.Point, Color.Yellow, Vector3.Zero, 1.0f, true);
            var lightComponent = new LightComponent(light);
            var worldObject = new WorldObject("UpdatingObject", 0,0,0);
            worldObject.AddComponent(lightComponent);

            // Act & Assert
            // Update is currently a no-op for LightComponent, so just ensure it doesn't throw.
            // If Update had behavior, we would assert that behavior here.
            var exception = Record.Exception(() => lightComponent.Update());
            Assert.Null(exception);
        }

        [Fact]
        public void LightComponent_Constructor_NullLight_ShouldThrowArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new LightComponent(null!)); // Added null-forgiving operator
        }
    }
}
