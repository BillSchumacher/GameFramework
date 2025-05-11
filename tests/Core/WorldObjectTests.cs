using Xunit;
using GameFramework;
using GameFramework.Core;

namespace GameFramework.Tests
{
    public class WorldObjectTests
    {
        [Fact]
        public void WorldObject_Creation_ShouldInitializeProperties()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);

            // Assert
            Assert.Equal("TestObject", worldObject.Name);
            Assert.Equal(0, worldObject.X);
            Assert.Equal(0, worldObject.Y);
            Assert.Equal(0, worldObject.Z);
            Assert.NotNull(worldObject.Id);
        }

        [Fact]
        public void WorldObject_SetPosition_ShouldUpdatePosition()
        {
            // Arrange
            var worldObject = new WorldObject("TestObject", 0, 0, 0);

            // Act
            worldObject.SetPosition(10, 20, 30);

            // Assert
            Assert.Equal(10, worldObject.X);
            Assert.Equal(20, worldObject.Y);
            Assert.Equal(30, worldObject.Z);
        }
    }
}
