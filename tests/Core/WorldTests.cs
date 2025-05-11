using Xunit;
using GameFramework;
using GameFramework.Core; // Added this using directive
using System.Linq;

namespace GameFramework.Tests
{
    public class WorldTests
    {
        [Fact]
        public void World_AddObject_ShouldAddObjectToList()
        {
            // Arrange
            var world = new World();
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0); // Added Z coordinate

            // Act
            world.AddObject(worldObject);

            // Assert
            Assert.Contains(worldObject, world.Objects); // Changed from GetObjects()
        }

        [Fact]
        public void World_AddObject_NullObject_ShouldNotAdd()
        {
            // Arrange
            var world = new World();

            // Act
            world.AddObject(null!);

            // Assert
            Assert.Empty(world.Objects); // Changed from GetObjects()
        }

        [Fact]
        public void World_RemoveObject_ShouldRemoveObjectFromList()
        {
            // Arrange
            var world = new World();
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0); // Added Z coordinate
            world.AddObject(worldObject);

            // Act
            world.RemoveObject(worldObject);

            // Assert
            Assert.DoesNotContain(worldObject, world.Objects); // Changed from GetObjects()
        }

        [Fact]
        public void World_GetObjectById_ShouldReturnCorrectObject()
        {
            // Arrange
            var world = new World();
            var worldObject1 = new WorldObject("obj1", "TestObject1", 0, 0, 0); // Added Z coordinate
            var worldObject2 = new WorldObject("obj2", "TestObject2", 0, 0, 0); // Added Z coordinate
            world.AddObject(worldObject1);
            world.AddObject(worldObject2);

            // Act
            var foundObject = world.GetObjectById("obj1");

            // Assert
            Assert.Equal(worldObject1, foundObject);
        }

        [Fact]
        public void World_GetObjectById_ShouldReturnNullIfNotFound()
        {
            // Arrange
            var world = new World();

            // Act
            var retrievedObject = world.GetObjectById("NonExistentId");

            // Assert
            Assert.Null(retrievedObject);
        }
    }
}
