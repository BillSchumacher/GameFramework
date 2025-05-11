using Xunit;
using GameFramework;
using GameFramework.Core; // Added this using directive

namespace GameFramework.Tests
{
    public class PlayerTests
    {
        [Fact]
        public void Player_Creation_ShouldSetNameAndDefaultScore()
        {
            // Arrange
            string playerName = "Hero";

            // Act
            Player player = new Player(playerName);

            // Assert
            Assert.Equal(playerName, player.Name);
            Assert.Equal(0, player.Score); // Default score should be 0
        }

        [Fact]
        public void Player_SetScore_ShouldUpdateScore()
        {
            // Arrange
            Player player = new Player("Hero");
            int newScore = 100;

            // Act
            player.Score = newScore;

            // Assert
            Assert.Equal(newScore, player.Score);
        }

        [Fact]
        public void Player_Name_CannotBeNullOrEmpty()
        {
            // Arrange & Act & Assert
            Assert.Throws<System.ArgumentException>(() => new Player(null!)); // Use null-forgiving operator
            Assert.Throws<System.ArgumentException>(() => new Player(""));
            Assert.Throws<System.ArgumentException>(() => new Player("   ")); // Whitespace only
        }

        [Fact]
        public void Player_HandleInput_ShouldProcessInputAndStoreActionWithFrame()
        {
            // Arrange
            Player player = new Player("TestPlayer");
            string input = "move_left";
            int frameNumber = 1;

            // Act
            player.HandleInput(input, frameNumber);

            // Assert
            Assert.Equal(input, player.LastAction);
            var history = player.GetActionHistory();
            Assert.Single(history);
            Assert.Equal(input, history[0].Action.Name); // Compare with Action.Name
            Assert.Equal(frameNumber, history[0].FrameNumber);
        }

        [Fact]
        public void Player_HandleInput_EmptyInput_ShouldNotStoreAction()
        {
            // Arrange
            Player player = new Player("TestPlayer");
            int frameNumber = 1;

            // Act
            player.HandleInput("", frameNumber); // Empty input
            player.HandleInput("   ", frameNumber + 1); // Whitespace input

            // Assert
            Assert.Empty(player.LastAction);
            Assert.Empty(player.GetActionHistory());
        }

        [Fact]
        public void Player_GetActionHistory_ShouldReturnAllActionsInOrder()
        {
            // Arrange
            Player player = new Player("TestPlayer");
            string input1 = "jump";
            int frame1 = 10;
            string input2 = "shoot";
            int frame2 = 12;

            // Act
            player.HandleInput(input1, frame1);
            player.HandleInput(input2, frame2);

            // Assert
            var history = player.GetActionHistory();
            Assert.Equal(2, history.Count);
            Assert.Equal(input1, history[0].Action.Name); // Compare with Action.Name
            Assert.Equal(frame1, history[0].FrameNumber);
            Assert.Equal(input2, history[1].Action.Name); // Compare with Action.Name
            Assert.Equal(frame2, history[1].FrameNumber);
            Assert.Equal(input2, player.LastAction); // LastAction should be the latest one
        }

        [Fact]
        public void Player_UndoLastAction_ShouldRemoveLastActionAndRestorePrevious()
        {
            // Arrange
            Player player = new Player("TestPlayer");
            string input1 = "walk";
            int frame1 = 5;
            string input2 = "run";
            int frame2 = 6;

            player.HandleInput(input1, frame1);
            player.HandleInput(input2, frame2);

            // Act
            player.UndoLastAction();

            // Assert
            var history = player.GetActionHistory();
            Assert.Single(history);
            Assert.Equal(input1, history[0].Action.Name); // Compare with Action.Name
            Assert.Equal(frame1, history[0].FrameNumber);
            Assert.Equal(input1, player.LastAction);
        }

        [Fact]
        public void Player_UndoLastAction_OnSingleAction_ShouldResultInEmptyHistory()
        {
            // Arrange
            Player player = new Player("TestPlayer");
            string input1 = "interact";
            int frame1 = 20;
            player.HandleInput(input1, frame1);

            // Act
            player.UndoLastAction();

            // Assert
            Assert.Empty(player.GetActionHistory());
            Assert.Empty(player.LastAction);
        }

        [Fact]
        public void Player_UndoLastAction_OnEmptyHistory_ShouldDoNothing()
        {
            // Arrange
            Player player = new Player("TestPlayer");

            // Act
            player.UndoLastAction();

            // Assert
            Assert.Empty(player.GetActionHistory());
            Assert.Empty(player.LastAction);
        }

        [Fact]
        public void Player_LastAction_ShouldBeEmpty_WhenHistoryIsEmpty()
        {
            // Arrange
            Player player = new Player("TestPlayer");

            // Assert
            Assert.Empty(player.LastAction);
        }

        [Fact]
        public void Player_Creation_ShouldSetPlayerType()
        {
            // Arrange
            var localPlayer = new Player("LocalHero", PlayerType.Local);
            var remotePlayer = new Player("RemoteHero", PlayerType.Remote);

            // Assert
            Assert.Equal(PlayerType.Local, localPlayer.Type);
            Assert.Equal(PlayerType.Remote, remotePlayer.Type);
        }

        [Fact]
        public void Player_DefaultPlayerType_ShouldBeLocal()
        {
            // Arrange
            var player = new Player("DefaultPlayer");

            // Assert
            Assert.Equal(PlayerType.Local, player.Type);
        }

        [Fact]
        public void Player_AssignControl_ShouldSetControlledObject()
        {
            // Arrange
            var player = new Player("TestPlayer");
            var worldObject = new WorldObject("ControllableObject", 0, 0, 0); // Added Z coordinate

            // Act
            player.AssignControl(worldObject);

            // Assert
            Assert.Equal(worldObject, player.ControlledObject);
        }

        [Fact]
        public void Player_AssignControl_NullObject_ShouldThrowArgumentNullException()
        {
            // Arrange
            var player = new Player("TestPlayer");

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => player.AssignControl(null!));
        }

        [Fact]
        public void Player_ReleaseControl_ShouldSetControlledObjectToNull()
        {
            // Arrange
            var player = new Player("TestPlayer");
            var worldObject = new WorldObject("ControllableObject", 0, 0, 0); // Added Z coordinate
            player.AssignControl(worldObject);

            // Act
            player.ReleaseControl();

            // Assert
            Assert.Null(player.ControlledObject);
        }

        [Fact]
        public void Player_HandleInput_WhenControllingObject_ShouldMoveObject()
        {
            // Arrange
            var player = new Player("TestPlayer");
            var worldObject = new WorldObject("ControllableObject", 0, 0, 0); // Added Z coordinate
            player.AssignControl(worldObject);
            int frameNumber = 1;

            // Act: Simulate moving right.
            player.HandleInput("move_right", frameNumber++);
            // Assert
            Assert.Equal(1, player.ControlledObject?.X);
            Assert.Equal(0, player.ControlledObject?.Y);
            Assert.Equal(0, player.ControlledObject?.Z);

            // Act: Simulate moving up.
            player.HandleInput("move_up", frameNumber++);
            // Assert
            Assert.Equal(1, player.ControlledObject?.X);
            Assert.Equal(1, player.ControlledObject?.Y);
            Assert.Equal(0, player.ControlledObject?.Z);

            // Act: Simulate moving left.
            player.HandleInput("move_left", frameNumber++);
            // Assert
            Assert.Equal(0, player.ControlledObject?.X);
            Assert.Equal(1, player.ControlledObject?.Y);
            Assert.Equal(0, player.ControlledObject?.Z);

            // Act: Simulate moving down.
            player.HandleInput("move_down", frameNumber++);
            // Assert
            Assert.Equal(0, player.ControlledObject?.X);
            Assert.Equal(0, player.ControlledObject?.Y);
            Assert.Equal(0, player.ControlledObject?.Z);
        }

        [Fact]
        public void Player_SetCamera_ShouldUpdateCameraProperty()
        {
            // Arrange
            var player = new Player("TestPlayer", PlayerType.Local);
            var camera = new Camera(0, 0, 0, 60.0f, 16.0f / 9.0f, 0.1f, 1000.0f); // Corrected constructor call

            // Act
            player.SetCamera(camera);

            // Assert
            Assert.NotNull(player.Camera);
            Assert.Same(camera, player.Camera);
        }

        [Fact]
        public void Player_ReleaseControl_WhenNotControllingObject_ShouldDoNothing()
        {
            // Arrange
            var player = new Player("TestPlayer");
            // Ensure ControlledObject is null initially
            Assert.Null(player.ControlledObject);

            // Act
            player.ReleaseControl();

            // Assert
            Assert.Null(player.ControlledObject); // Should still be null
            // No exception should be thrown
        }

        [Fact]
        public void Player_HandleInput_UnknownInput_ShouldNotMoveObjectAndStoreAction()
        {
            // Arrange
            var player = new Player("TestPlayer");
            var worldObject = new WorldObject("ControllableObject", 0, 0, 0);
            player.AssignControl(worldObject);
            int frameNumber = 5;
            string unknownInput = "fly_to_the_moon";

            // Act
            player.HandleInput(unknownInput, frameNumber);

            // Assert
            // Object position should not change
            Assert.Equal(0, player.ControlledObject?.X);
            Assert.Equal(0, player.ControlledObject?.Y);
            Assert.Equal(0, player.ControlledObject?.Z);

            // Action should still be recorded
            Assert.Equal(unknownInput, player.LastAction);
            var history = player.GetActionHistory();
            Assert.Single(history);
            Assert.Equal(unknownInput, history[0].Action.Name);
            Assert.Equal(frameNumber, history[0].FrameNumber);
        }
    }
}
