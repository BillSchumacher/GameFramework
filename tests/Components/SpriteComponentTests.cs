using Xunit;
using GameFramework;

namespace GameFramework.Tests
{
    public class SpriteComponentTests
    {
        [Fact]
        public void SpriteComponent_Creation_ShouldInitializeProperties()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("test_sprite.png");

            // Assert
            Assert.Equal("test_sprite.png", spriteComponent.SpritePath);
            Assert.Equal(255, spriteComponent.Color.A);
            Assert.Equal(255, spriteComponent.Color.R);
            Assert.Equal(255, spriteComponent.Color.G);
            Assert.Equal(255, spriteComponent.Color.B);
            // Add assertions for other properties like Material when implemented
        }

        [Fact]
        public void SpriteComponent_Creation_NullOrEmptyPath_ShouldThrowArgumentException()
        {
            // Assert
            Assert.Throws<System.ArgumentException>(() => new SpriteComponent(null!));
            Assert.Throws<System.ArgumentException>(() => new SpriteComponent(""));
            Assert.Throws<System.ArgumentException>(() => new SpriteComponent(" "));
        }

        [Fact]
        public void SpriteComponent_OnAttach_SetsParent()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("test_sprite.png");
            var worldObject = new WorldObject("TestObject", 0, 0, 0);

            // Act
            spriteComponent.Parent = worldObject;
            spriteComponent.OnAttach();

            // Assert
            Assert.Equal(worldObject, spriteComponent.Parent);
        }

        [Fact]
        public void SpriteComponent_OnDetach_ClearsParent()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("test_sprite.png");
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            spriteComponent.Parent = worldObject;
            spriteComponent.OnAttach();

            // Act
            spriteComponent.OnDetach();

            // Assert
            Assert.Null(spriteComponent.Parent);
        }

        [Fact]
        public void SpriteComponent_Update_DoesNotThrow()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("test_sprite.png");
            var worldObject = new WorldObject("TestObject", 0, 0, 0);
            spriteComponent.Parent = worldObject;
            spriteComponent.OnAttach();

            // Act & Assert
            var exception = Record.Exception(() => spriteComponent.Update());
            Assert.Null(exception);
        }
    }
}
