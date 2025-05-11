using Xunit;
using GameFramework;
using GameFramework.Core;

namespace GameFramework.Tests
{
    public class WorldObjectTests
    {
        [Fact]
        public void WorldObject_Creation_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30); // Added Z coordinate

            // Assert
            Assert.Equal("obj1", worldObject.Id);
            Assert.Equal("TestObject", worldObject.Name);
            Assert.Equal(10, worldObject.X);
            Assert.Equal(20, worldObject.Y);
            Assert.Equal(30, worldObject.Z);
        }

        [Fact]
        public void WorldObject_SetPosition_ShouldUpdateCoordinates()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0); // Added Z coordinate

            // Act
            worldObject.SetPosition(10, 20, 30);

            // Assert
            Assert.Equal(10, worldObject.X);
            Assert.Equal(20, worldObject.Y);
            Assert.Equal(30, worldObject.Z);
        }
    }
}
