// filepath: c:/Users/willi/game_platform/IGameLogic.cs
namespace GameFramework
{
    public interface IGameLogic
    {
        void Initialize();
        void Update(double deltaTime);
        void Render();
        void HandleInput(string input);
        bool IsRunning { get; } // Game logic controls its running state
        void Stop();
    }
}
