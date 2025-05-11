using Xunit;
using GameFramework;
using GameFramework.Core; // Added this using directive
using System.Numerics;
using System.Drawing;

namespace GameFramework.Tests
{
    // Mock Component for testing
    public class MockComponent : IComponent
    {
        public WorldObject? Parent { get; set; } // Made Parent nullable
        public bool IsAttached { get; private set; }
        public bool IsUpdated { get; private set; }
        public bool IsDetached { get; private set; }

        public void OnAttach()
        {
            IsAttached = true;
        }

        public void OnDetach()
        {
            IsDetached = true;
        }

        public void Update()
        {
            IsUpdated = true;
        }
    }

    public class WorldObjectComponentTests
    {
        [Fact]
        public void WorldObject_AddComponent_ShouldAddComponentToList()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component = new MockComponent();

            // Act
            worldObject.AddComponent(component);

            // Assert
            Assert.Contains(component, worldObject.Components);
            Assert.Equal(worldObject, component.Parent);
            Assert.True(component.IsAttached);
        }

        [Fact]
        public void WorldObject_AddComponent_NullComponent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => worldObject.AddComponent(null!)); // Added null-forgiving operator
        }

        [Fact]
        public void WorldObject_RemoveComponent_ShouldRemoveComponentFromList()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component = new MockComponent();
            worldObject.AddComponent(component);

            // Act
            var result = worldObject.RemoveComponent(component);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(component, worldObject.Components);
            Assert.True(component.IsDetached);
            Assert.Null(component.Parent);
        }

        [Fact]
        public void WorldObject_RemoveComponent_ComponentNotPresent_ShouldReturnFalse()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component = new MockComponent();
            var otherComponent = new MockComponent();
            worldObject.AddComponent(component);

            // Act
            var result = worldObject.RemoveComponent(otherComponent);

            // Assert
            Assert.False(result);
            Assert.Contains(component, worldObject.Components); // Ensure original component is still there
            Assert.False(otherComponent.IsDetached);
        }
        
        [Fact]
        public void WorldObject_RemoveComponent_NullComponent_ShouldReturnFalse()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component = new MockComponent();
            worldObject.AddComponent(component);

            // Act
            var result = worldObject.RemoveComponent(null!); // Added null-forgiving operator

            // Assert
            Assert.False(result);
            Assert.Contains(component, worldObject.Components);
        }

        [Fact]
        public void WorldObject_GetComponent_ShouldReturnComponentOfType()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component = new MockComponent();
            worldObject.AddComponent(component);

            // Act
            var retrievedComponent = worldObject.GetComponent<MockComponent>();

            // Assert
            Assert.NotNull(retrievedComponent);
            Assert.Equal(component, retrievedComponent);
        }

        [Fact]
        public void WorldObject_GetComponent_NoComponentOfType_ShouldReturnNull()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var lightComponent = new LightComponent(new Light(LightType.Point, Color.White, Vector3.Zero));
            worldObject.AddComponent(lightComponent);


            // Act
            var retrievedComponent = worldObject.GetComponent<MockComponent>();

            // Assert
            Assert.Null(retrievedComponent);
        }

        [Fact]
        public void WorldObject_UpdateComponents_ShouldCallUpdateOnAllComponents()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            var component1 = new MockComponent();
            var component2 = new MockComponent();
            worldObject.AddComponent(component1);
            worldObject.AddComponent(component2);

            // Act
            worldObject.UpdateComponents();

            // Assert
            Assert.True(component1.IsUpdated);
            Assert.True(component2.IsUpdated);
        }
    }
}
