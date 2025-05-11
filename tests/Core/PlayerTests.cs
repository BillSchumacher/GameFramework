using Xunit;
using GameFramework.Core; 
using System;
using System.Linq;

namespace GameFramework.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void Player_Creation_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var player = new Player("player1", "TestPlayer", 100);

            // Assert
            Assert.Equal("player1", player.Id);
            Assert.Equal("TestPlayer", player.Name);
            Assert.Equal(100, player.Score);
            Assert.Equal(PlayerType.Local, player.Type);
            Assert.Empty(player.LastAction);
            Assert.Null(player.ControlledObject);
        }

        [Fact]
        public void Player_Creation_WithEmptyName_ShouldThrowArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => new Player("player1", "", 100));
        }

        [Fact]
        public void Player_Creation_WithNullName_ShouldThrowArgumentException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentException>(() => new Player("player1", null!, 100));
        }

        [Fact]
        public void Player_AssignControl_ShouldSetControlledObject()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);
            var worldObject = new WorldObject("obj1", "Controllable", 0,0,0);

            // Act
            player.AssignControl(worldObject);

            // Assert
            Assert.Same(worldObject, player.ControlledObject);
        }

        [Fact]
        public void Player_AssignControl_NullObject_ShouldThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => player.AssignControl(null!));
        }

        [Fact]
        public void Player_ReleaseControl_ShouldClearControlledObject()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);
            var worldObject = new WorldObject("obj1", "Controllable", 0,0,0);
            player.AssignControl(worldObject);

            // Act
            player.ReleaseControl();

            // Assert
            Assert.Null(player.ControlledObject);
        }

        [Fact]
        public void Player_SetCamera_ShouldAssignCamera()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);
            var camera = new Camera(0,0,0, 60f, 16f/9f, 0.1f, 1000f); 

            // Act
            player.SetCamera(camera);

            // Assert
            Assert.Same(camera, player.Camera);
        }

        [Fact]
        public void Player_SetCamera_NullCamera_ShouldThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => player.SetCamera(null!));
        }

        [Fact]
        public void Player_SetPlayerType_ShouldUpdatePlayerType()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);

            // Act
            player.SetPlayerType(PlayerType.AI); 

            // Assert
            Assert.Equal(PlayerType.AI, player.Type);
        }

        [Fact]
        public void Player_RecordAction_ShouldAddActionToHistory()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);
            var action = new StringInputAction("TestAction"); // Using StringInputAction as a concrete type
            int frameNumber = 1;

            // Act
            player.RecordAction(action, frameNumber); 

            // Assert
            var history = player.GetActionHistory();
            Assert.Single(history);
            Assert.Same(action, history[0].Action);
            Assert.Equal(frameNumber, history[0].FrameNumber);
            Assert.Equal(action.Name, player.LastAction);
        }

        [Fact]
        public void Player_RecordAction_NullAction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => player.RecordAction(null!, 1)); 
        }

        [Fact]
        public void Player_GetActionHistory_ShouldReturnAllRecordedActions()
        {
            // Arrange
            var player = new Player("player1", "TestPlayer", 0);
            var action1 = new StringInputAction("Action1"); 
            var action2 = new StringInputAction("Action2"); 
            player.RecordAction(action1, 1); 
            player.RecordAction(action2, 2); 

            // Act
            var history = player.GetActionHistory();

            // Assert
            Assert.Equal(2, history.Count);
            Assert.Same(action1, history[0].Action);
            Assert.Same(action2, history[1].Action);
        }


        [Fact]
        public void Player_Constructor_WithValidIdAndName_ShouldCreatePlayer()
        {
            // Act
            var player = new Player("testPlayerId", "Test Player", 0); 

            // Assert
            Assert.Equal("testPlayerId", player.Id);
            Assert.Equal("Test Player", player.Name);
            Assert.Equal(0, player.Score);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Player_Constructor_WithInvalidId_ShouldThrowArgumentException(string? invalidId) // Changed to string?
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Player(invalidId!, "Test Player", 0));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Player_Constructor_WithInvalidName_ShouldThrowArgumentException(string? invalidName) // Changed to string?
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Player("testPlayerId", invalidName!, 0));
        }


        [Fact]
        public void Player_InitialScore_ShouldBeZeroOrSetValue() 
        {
            // Act
            var player = new Player("testPlayerId", "Test Player", 50); 

            // Assert
            Assert.Equal(50, player.Score);
        }

        [Fact]
        public void Player_AddScore_ShouldIncreaseScore()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);
            var initialScore = player.Score;

            // Act
            player.AddScore(10); 

            // Assert
            Assert.Equal(initialScore + 10, player.Score);
        }

        [Fact]
        public void Player_AddScore_NegativeAmount_ShouldDecreaseScore()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);
            player.AddScore(20); 
            var initialScore = player.Score;

            // Act
            player.AddScore(-5); 

            // Assert
            Assert.Equal(initialScore - 5, player.Score);
        }

        [Fact]
        public void Player_ControlledObject_Initial_ShouldBeNull()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);

            // Assert
            Assert.Null(player.ControlledObject);
        }

        [Fact]
        public void Player_AssignControl_ValidObject_ShouldSetControlledObject()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);
            var worldObject = new WorldObject("objId", "Test Object", 0, 0, 0); 

            // Act
            player.AssignControl(worldObject);

            // Assert
            Assert.Same(worldObject, player.ControlledObject);
        }

        [Fact]
        public void Player_AssignControl_NullObject_Should_ThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => player.AssignControl(null!));
        }

        [Fact]
        public void Player_ReleaseControl_WhenControllingObject_ShouldSetControlledObjectToNull()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);
            var worldObject = new WorldObject("objId", "Test Object", 0, 0, 0);
            player.AssignControl(worldObject);

            // Act
            player.ReleaseControl();

            // Assert
            Assert.Null(player.ControlledObject);
        }

        [Fact]
        public void Player_ReleaseControl_WhenNotControllingObject_ShouldDoNothing()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);

            // Act
            player.ReleaseControl(); 

            // Assert
            Assert.Null(player.ControlledObject); 
        }

        [Fact]
        public void Player_Camera_Initial_ShouldBeNull()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);

            // Assert
            Assert.Null(player.Camera);
        }

        [Fact]
        public void Player_SetCamera_ValidCamera_ShouldSetPlayerCamera()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);
            var camera = new Camera(0,0,0); 

            // Act
            player.SetCamera(camera);

            // Assert
            Assert.Same(camera, player.Camera);
        }

        [Fact]
        public void Player_SetCamera_NullCamera_Should_ThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("testPlayerId", "Test Player", 0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => player.SetCamera(null!));
        }
    }
}
