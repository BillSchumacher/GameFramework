using System;
using Xunit;
using GameFramework;

namespace GameFramework.Tests
{
    // Concrete implementation of BaseAction for testing purposes
    public class TestAction : BaseAction
    {
        public override string Name { get; } // Added override
        public TestAction(string name) { Name = name; }
        // Override Equals and GetHashCode for proper comparison in tests
        public override bool Equals(object? obj) // Changed to object?
        {
            return obj is TestAction action && Name == action.Name;
        }
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }

    public class PlayerActionTests
    {
        [Fact]
        public void Constructor_ValidParameters_InitializesProperties()
        {
            // Arrange
            var action = new TestAction("Jump");
            int frameNumber = 10;

            // Act
            var playerAction = new PlayerAction(action, frameNumber);

            // Assert
            Assert.Equal(action, playerAction.Action);
            Assert.Equal(frameNumber, playerAction.FrameNumber);
        }

        [Fact]
        public void Constructor_NullAction_ThrowsArgumentNullException()
        {
            // Arrange
            BaseAction? invalidAction = null; // Made BaseAction nullable
            int frameNumber = 5;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PlayerAction(invalidAction!, frameNumber)); // Added null-forgiving operator
        }

        [Fact]
        public void Constructor_NegativeFrameNumber_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var action = new TestAction("Shoot");
            int frameNumber = -1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerAction(action, frameNumber));
        }
    }
}
