using Xunit;
using GameFramework;
using GameFramework.Core; // For WorldObject
using System;
using System.Text.Json.Serialization; // For JsonIgnore if needed in test components

namespace GameFramework.Tests.Components
{
    // Define OtherComponent for testing purposes
    public class OtherComponent : IComponent
    {
        [JsonIgnore]
        public WorldObject? Parent { get; set; }
        public void OnAttach() { }
        public void OnDetach() { }
        public void Update() { }
    }

    public class ComponentTests // Made class public
    {
        // Made ConcreteComponent public for accessibility in tests
        public class ConcreteComponent : IComponent
        {
            [JsonIgnore]
            public WorldObject? Parent { get; set; }
            public bool OnAttachCalled { get; private set; }
            public bool OnDetachCalled { get; private set; }
            public bool UpdateCalled { get; private set; }

            public void OnAttach() { OnAttachCalled = true; Parent?.ToString(); /* Access Parent to ensure it's set */ }
            public void OnDetach() { OnDetachCalled = true; }
            public void Update() { UpdateCalled = true; }
        }

        [Fact]
        public void WorldObject_AddComponent_ShouldAddAndCallOnAttach()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component = new ConcreteComponent();

            // Act
            worldObject.AddComponent(component);

            // Assert
            Assert.Contains(component, worldObject.Components);
            Assert.True(component.OnAttachCalled);
            Assert.Same(worldObject, component.Parent);
        }

        [Fact]
        public void WorldObject_AddComponent_NullComponent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => worldObject.AddComponent(null!));
        }

        [Fact]
        public void WorldObject_AddComponent_DuplicateComponent_ShouldNotAdd()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component = new ConcreteComponent();
            worldObject.AddComponent(component);
            var initialCount = worldObject.Components.Count;

            // Act
            worldObject.AddComponent(component); 

            // Assert
            Assert.Equal(initialCount, worldObject.Components.Count);
        }

        [Fact]
        public void WorldObject_RemoveComponent_ShouldRemoveAndCallOnDetach()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component = new ConcreteComponent();
            worldObject.AddComponent(component);

            // Act
            worldObject.RemoveComponent(component);

            // Assert
            Assert.DoesNotContain(component, worldObject.Components);
            Assert.True(component.OnDetachCalled);
            Assert.Null(component.Parent); // Parent should be cleared on detach
        }

        [Fact]
        public void WorldObject_RemoveComponent_NullComponent_ShouldThrowArgumentNullException()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => worldObject.RemoveComponent(null!));
        }

        [Fact]
        public void WorldObject_RemoveComponent_NonExistentComponent_ShouldNotThrow()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component = new ConcreteComponent(); 

            // Act & Assert
            Exception? ex = Record.Exception(() => worldObject.RemoveComponent(component)); // Corrected Record.Exception
            Assert.Null(ex);
        }

        [Fact]
        public void WorldObject_GetComponent_ShouldReturnComponentOfType()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component = new ConcreteComponent();
            worldObject.AddComponent(component);

            // Act
            var retrievedComponent = worldObject.GetComponent<ConcreteComponent>();

            // Assert
            Assert.Same(component, retrievedComponent);
        }

        [Fact]
        public void WorldObject_GetComponent_NonExistentType_ShouldReturnNull()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            
            // Act
            var retrievedComponent = worldObject.GetComponent<OtherComponent>(); 

            // Assert
            Assert.Null(retrievedComponent);
        }

        [Fact]
        public void WorldObject_UpdateComponents_ShouldCallUpdateOnAllComponents()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var component1 = new ConcreteComponent();
            var component2 = new ConcreteComponent();
            worldObject.AddComponent(component1);
            worldObject.AddComponent(component2);

            // Act
            worldObject.UpdateComponents();

            // Assert
            Assert.True(component1.UpdateCalled);
            Assert.True(component2.UpdateCalled);
        }
    }
}
