using Xunit;
using GameFramework;
using GameFramework.Core; // For WorldObject
using System.Drawing; // For Color

namespace GameFramework.Tests.Components
{
    public class SpriteComponentTests
    {
        [Fact]
        public void SpriteComponent_Creation_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var spriteComponent = new SpriteComponent("path/to/sprite.png", Color.White);

            // Assert
            Assert.Equal("path/to/sprite.png", spriteComponent.SpritePath);
            Assert.Equal(Color.White, spriteComponent.Color);
            Assert.Null(spriteComponent.Parent);
        }

        [Theory]
        [InlineData("")] // Null path handled by constructor allowing it
        [InlineData(" ")]
        public void SpriteComponent_Creation_WithInvalidPath_ShouldStillCreate(string invalidPath)
        {
            // Arrange & Act
            var spriteComponent = new SpriteComponent(invalidPath, Color.Red);

            // Assert
            Assert.Equal(invalidPath, spriteComponent.SpritePath);
            Assert.Equal(Color.Red, spriteComponent.Color);
        }
        
        [Fact]
        public void SpriteComponent_Creation_WithNullPath_ShouldStillCreate()
        {
            // Arrange & Act
            var spriteComponent = new SpriteComponent(null!, Color.Red);

            // Assert
            Assert.Null(spriteComponent.SpritePath); // Or string.Empty depending on constructor logic for null
            Assert.Equal(Color.Red, spriteComponent.Color);
        }


        [Fact]
        public void SpriteComponent_OnAttach_ShouldSetParent()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("path/to/sprite.png", Color.Blue);
            var parentObject = new WorldObject("parentId", "ParentObj", 0,0,0); // Added name

            // Act
            parentObject.AddComponent(spriteComponent); 

            // Assert
            Assert.Same(parentObject, spriteComponent.Parent);
        }

        [Fact]
        public void SpriteComponent_OnDetach_ShouldClearParent()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("path/to/sprite.png", Color.Green);
            var parentObject = new WorldObject("parentId", "ParentObj",0,0,0); // Added name
            parentObject.AddComponent(spriteComponent);

            // Act
            parentObject.RemoveComponent(spriteComponent); 

            // Assert
            Assert.Null(spriteComponent.Parent);
        }

        [Fact]
        public void SpriteComponent_Update_ShouldNotThrowException()
        {
            // Arrange
            var spriteComponent = new SpriteComponent("path/to/sprite.png", Color.Yellow);
            var parentObject = new WorldObject("parentId", "ParentObj",0,0,0); // Added name
            parentObject.AddComponent(spriteComponent);

            // Act & Assert
            Exception? ex = Record.Exception(() => spriteComponent.Update());
            Assert.Null(ex);
        }
    }
}
