using Xunit;
using Moq;
using GameFramework; // Add this using directive
using System.Threading.Tasks; // Required for async Task

namespace GameFramework.Tests
{
    public class GameLoopTests
    {
        private readonly Mock<IGameLogic> _mockGameLogic;
        private readonly GameFramework _gameFramework;

        public GameLoopTests()
        {
            _mockGameLogic = new Mock<IGameLogic>();
            _gameFramework = new GameFramework();
            _gameFramework.SetGameLogic(_mockGameLogic.Object); 
        }

        [Fact]
        public void GameLoop_Initialize_ShouldCallGameLogicInitialize()
        {
            // Arrange
            _mockGameLogic.Setup(gl => gl.Initialize());

            // Act
            _gameFramework.InitializeGame(); 

            // Assert
            _mockGameLogic.Verify(gl => gl.Initialize(), Times.Once);
        }

        [Fact]
        public async Task GameLoop_Run_ShouldCallUpdateAndRenderMultipleTimes() // Changed to async Task
        {
            // Arrange
            _mockGameLogic.SetupGet(gl => gl.IsRunning).Returns(true); // Game logic starts as running
            _mockGameLogic.Setup(gl => gl.Update(It.IsAny<double>()));
            _mockGameLogic.Setup(gl => gl.Render());

            // Act
            await _gameFramework.RunGameLoop(3); // Await the async call

            // Assert
            _mockGameLogic.Verify(gl => gl.Update(It.IsAny<double>()), Times.Exactly(3));
            _mockGameLogic.Verify(gl => gl.Render(), Times.Exactly(3));
        }
        
        [Fact]
        public async Task GameLoop_Stop_ShouldSetIsRunningToFalseAndStopLoop() // Changed to async Task
        {
            // Arrange
            bool mockInternalIsRunning = true; // Simulate the internal state of IsRunning for the mock

            _mockGameLogic.SetupGet(gl => gl.IsRunning).Returns(() => mockInternalIsRunning);
            _mockGameLogic.Setup(gl => gl.Stop()).Callback(() => 
            {
                mockInternalIsRunning = false; // When Stop() is called, change the simulated state
            });
        
            // Act
            _gameFramework.InitializeGame(); // Initialize first

            // Request the game to stop. This will call _gameLogic.Stop(), then the callback changes mockInternalIsRunning.
            _gameFramework.RequestStopGame(); 
            
            // Attempt to run the loop. It should recognize that IsRunning is false (or becomes false quickly)
            // and not run for the full 5 frames.
            await _gameFramework.RunGameLoop(5); // Await the async call
        
            // Assert
            _mockGameLogic.Verify(gl => gl.Stop(), Times.AtLeastOnce()); // Stop should have been called.
            Assert.False(mockInternalIsRunning); // Verify the simulated state changed.
        }

        [Fact]
        public void GameLoop_HandleInput_ShouldCallGameLogicHandleInput()
        {
            // Arrange
            var testInput = "Jump";
            _mockGameLogic.Setup(gl => gl.HandleInput(testInput));

            // Act
            _gameFramework.ProcessInput(testInput); 

            // Assert
            _mockGameLogic.Verify(gl => gl.HandleInput(testInput), Times.Once);
        }
    }
}
