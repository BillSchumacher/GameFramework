using Xunit;
using GameFramework;
using System.Linq;

namespace GameFramework.Tests
{
    public class WorldTests
    {
        [Fact]
        public void World_AddObject_ShouldContainObject()
        {
            // Arrange
            var world = new World();
            var worldObject = new WorldObject("TestObject", 0, 0, 0);

            // Act
            world.AddObject(worldObject);

            // Assert
            Assert.Contains(worldObject, world.GetObjects());
        }

        [Fact]
        public void World_RemoveObject_ShouldNotContainObject()
        {
            // Arrange
            var world = new World();
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            world.AddObject(worldObject);

            // Act
            world.RemoveObject(worldObject);

            // Assert
            Assert.DoesNotContain(worldObject, world.GetObjects());
        }

        [Fact]
        public void World_GetObjectById_ShouldReturnCorrectObject()
        {
            // Arrange
            var world = new World();
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            world.AddObject(worldObject);

            // Act
            var retrievedObject = world.GetObjectById(worldObject.Id);

            // Assert
            Assert.Equal(worldObject, retrievedObject);
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
