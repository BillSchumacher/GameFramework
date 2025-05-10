using Xunit;
using GameFramework;

namespace GameFramework.Tests
{
    public class GameCreationTests
    {
        [Fact]
        public void CreateNewGame_ShouldInitializeWithDefaultValues()
        {
            // Arrange
            var gameFramework = new GameFramework();

            // Act
            var newGame = gameFramework.CreateNewGame();

            // Assert
            Assert.NotNull(newGame);
            Assert.Equal("Untitled", newGame.Name);
            Assert.Empty(newGame.Levels);
        }

        [Fact]
        public void CreateNewGame_ShouldAllowCustomName()
        {
            // Arrange
            var gameFramework = new GameFramework();
            string customName = "My Custom Game";

            // Act
            var newGame = gameFramework.CreateNewGame(customName);

            // Assert
            Assert.NotNull(newGame);
            Assert.Equal(customName, newGame.Name);
        }
    }
}