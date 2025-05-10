using System;
using System.Collections.Generic; // Required for List
using System.Linq; // Required for LastOrDefault

namespace GameFramework
{
    public class Player
    {
        public string Name { get; private set; }
        public int Score { get; set; }
        public PlayerType Type { get; private set; } // Added PlayerType property
        public string LastAction => actionHistory.LastOrDefault()?.Action ?? string.Empty; // Updated to get from history
        public WorldObject? ControlledObject { get; private set; } // Nullable WorldObject
        public Camera? Camera { get; private set; } // Nullable Camera

        private readonly List<PlayerAction> actionHistory; // New field for action history

        public Player(string name, PlayerType type = PlayerType.Local) // Added PlayerType parameter with default value
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Player name cannot be null or whitespace.", nameof(name));
            }
            Name = name;
            Score = 0; // Default score
            Type = type; // Assign PlayerType
            actionHistory = new List<PlayerAction>(); // Initialize action history
        }

        public void AssignControl(WorldObject worldObject)
        {
            if (worldObject == null)
            {
                throw new ArgumentNullException(nameof(worldObject), "Cannot assign control to a null object.");
            }
            ControlledObject = worldObject;
            Console.WriteLine($"{Name} is now controlling {worldObject.Name}.");
        }

        public void ReleaseControl()
        {
            if (ControlledObject != null)
            {
                Console.WriteLine($"{Name} released control of {ControlledObject.Name}.");
                ControlledObject = null;
            }
            else
            {
                Console.WriteLine($"{Name} is not controlling any object.");
            }
        }

        public void HandleInput(string input, int frameNumber) // Added frameNumber parameter
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                // Optional: Decide if empty input should be an error or ignored
                // For now, let's assume empty/whitespace input is not a valid action to record
                Console.WriteLine($"{Name} received empty input at frame {frameNumber}. No action recorded.");
                return;
            }
            var playerAction = new PlayerAction(input, frameNumber);
            actionHistory.Add(playerAction);
            // The LastAction property will now automatically reflect this new action.
            Console.WriteLine($"{Name} processed input: {input} at frame {frameNumber}. Last action: {LastAction}");

            // If controlling an object, interpret input to manipulate the object
            if (ControlledObject != null)
            {
                // Simple example: "move_right" increases X, "move_left" decreases X
                // "move_up" increases Y, "move_down" decreases Y
                // This can be expanded with more complex input parsing and actions
                int currentX = ControlledObject.X;
                int currentY = ControlledObject.Y;
                int currentZ = ControlledObject.Z; // Get current Z

                switch (input.ToLower())
                {
                    case "move_right":
                        ControlledObject.SetPosition(currentX + 1, currentY, currentZ); // Pass Z
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})"); // Log Z
                        break;
                    case "move_left":
                        ControlledObject.SetPosition(currentX - 1, currentY, currentZ); // Pass Z
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})"); // Log Z
                        break;
                    case "move_up":
                        ControlledObject.SetPosition(currentX, currentY + 1, currentZ); // Pass Z
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})"); // Log Z
                        break;
                    case "move_down":
                        ControlledObject.SetPosition(currentX, currentY - 1, currentZ); // Pass Z
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})"); // Log Z
                        break;
                        // Add more cases for other actions like "jump", "interact", etc.
                }
            }
        }

        public IReadOnlyList<PlayerAction> GetActionHistory() // New method to get action history
        {
            return actionHistory.AsReadOnly();
        }

        public void UndoLastAction()
        {
            if (actionHistory.Any())
            {
                actionHistory.RemoveAt(actionHistory.Count - 1);
                Console.WriteLine($"{Name} undid last action. Current last action: {LastAction}");
            }
            else
            {
                Console.WriteLine($"{Name} has no actions to undo.");
            }
        }

        public void SetCamera(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "Cannot set a null camera.");
            }
            Camera = camera;
            Console.WriteLine($"{Name}'s camera set.");
        }
    }

}
