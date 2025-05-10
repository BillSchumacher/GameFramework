using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks; // Required for Task.Delay

namespace GameFramework
{
    // IGameLogic is now defined in IGameLogic.cs

    public class GameFramework
    {
        private IGameLogic? _gameLogic;
        private bool _isRunning = false;

        public Game CreateNewGame(string name = "Untitled")
        {
            return new Game { Name = name, Levels = new List<string>() };
        }

        public void SetGameLogic(IGameLogic logic)
        {
            _gameLogic = logic;
        }

        public void InitializeGame()
        {
            _gameLogic?.Initialize();
        }

        public async Task RunGameLoop(int frames)
        {
            if (_gameLogic == null) return;

            _isRunning = true;
            // The GameLogic's IsRunning property should be managed by its own implementation.
            // The GameFramework loop will respect it.

            for (int i = 0; i < frames && _isRunning && _gameLogic.IsRunning; i++)
            {
                _gameLogic.Update(16); // Assuming 60 FPS, deltaTime is approx 16ms.
                _gameLogic.Render();
                await Task.Delay(16); // Simulate frame delay
            }
            _isRunning = false; // Ensure framework loop stops if frames are done or logic stopped
        }

        public void RequestStopGame()
        {
            _gameLogic?.Stop();
            _isRunning = false;
        }

        public void ProcessInput(string input)
        {
            _gameLogic?.HandleInput(input);
        }
    }

    public class Game
    {
        public string Name { get; set; }
        public List<string> Levels { get; set; }

        public Game()
        {
            Name = string.Empty;
            Levels = new List<string>();
        }
    }
}