using System;
using System.Collections.Generic; // Required for List
using System.Linq; // Required for LastOrDefault
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFramework.Core
{
    public class Player : WorldObject // Inherit from WorldObject to reuse Id, Name, serialization logic
    {
        public int Score { get; set; } // Made settable
        public PlayerType Type { get; private set; } // Added PlayerType property
        public string LastAction => actionHistory.LastOrDefault()?.Action.Name ?? string.Empty; // Updated to get Name from BaseAction
        public WorldObject? ControlledObject { get; private set; } // Nullable WorldObject
        public Camera? Camera { get; private set; } // Nullable Camera

        private readonly List<PlayerAction> actionHistory; // New field for action history

        // Parameterless constructor for JSON deserialization
        public Player() : this("default_player_id", "DefaultPlayerName", 0)
        {
        }

        [JsonConstructor]
        public Player(string id, string name, int score) : base(id, name, 0, 0, 0) // Call base constructor
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Player name cannot be null or empty.", nameof(name));
            }
            Score = score;
            Type = PlayerType.Local; // Default type
            actionHistory = new List<PlayerAction>();
        }

        public void AddScore(int amount) // New method
        {
            Score += amount;
        }

        public void SetPlayerType(PlayerType type) // New method
        {
            Type = type;
        }

        public void RecordAction(BaseAction action, int frameNumber) // New method for testing
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (frameNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameNumber), "Frame number cannot be negative.");
            }
            actionHistory.Add(new PlayerAction(action, frameNumber));
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
                Console.WriteLine($"{Name} received empty input at frame {frameNumber}. No action recorded.");
                return;
            }
            var concreteAction = new StringInputAction(input);
            var playerAction = new PlayerAction(concreteAction, frameNumber); // Pass the concrete action
            actionHistory.Add(playerAction);
            Console.WriteLine($"{Name} processed input: {input} at frame {frameNumber}. Last action: {LastAction}");

            if (ControlledObject != null)
            {
                int currentX = ControlledObject.X;
                int currentY = ControlledObject.Y;
                int currentZ = ControlledObject.Z;

                switch (input.ToLower())
                {
                    case "move_right":
                        ControlledObject.SetPosition(currentX + 1, currentY, currentZ);
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})");
                        break;
                    case "move_left":
                        ControlledObject.SetPosition(currentX - 1, currentY, currentZ);
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})");
                        break;
                    case "move_up":
                        ControlledObject.SetPosition(currentX, currentY + 1, currentZ);
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})");
                        break;
                    case "move_down":
                        ControlledObject.SetPosition(currentX, currentY - 1, currentZ);
                        Console.WriteLine($"{Name} moved {ControlledObject.Name} to ({ControlledObject.X}, {ControlledObject.Y}, {ControlledObject.Z})");
                        break;
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

        public new string ToJson() // 'new' keyword to hide base.ToJson if signature is same
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        public static new Player? FromJson(string json) // 'new' keyword
        {
            var options = new JsonSerializerOptions { };
            var player = JsonSerializer.Deserialize<Player>(json, options);
            if (player != null)
            {
                foreach (var component in player.Components) // Components from WorldObject
                {
                    component.Parent = player;
                }
            }
            return player;
        }
    }
}
