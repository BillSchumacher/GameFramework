using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFramework.Core
{
    public class World
    {
        // Made properties settable for deserialization
        public List<WorldObject> Objects { get; set; }
        public List<Player> Players { get; set; } // Added Players list
        // Potentially add UserInterface here if it's part of the world state to be serialized
        // public UserInterface UI { get; set; }

        // Parameterless constructor for JSON deserialization
        public World()
        {
            Objects = new List<WorldObject>();
            Players = new List<Player>();
            // UI = new UserInterface(); // Initialize if UI is part of the world
        }

        public void AddObject(WorldObject worldObject)
        {
            if (worldObject != null && !Objects.Contains(worldObject))
            {
                Objects.Add(worldObject);
            }
        }

        public void RemoveObject(WorldObject worldObject)
        {
            Objects.Remove(worldObject);
        }

        public WorldObject? GetObjectById(string id)
        {
            return Objects.FirstOrDefault(obj => obj.Id == id);
        }

        public void AddPlayer(Player player)
        {
            if (player != null && !Players.Contains(player))
            {
                Players.Add(player);
            }
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player);
        }

        public Player? GetPlayerById(string id)
        {
            return Players.FirstOrDefault(p => p.Id == id);
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // This is crucial for handling derived types like Player within lists of WorldObject if they were mixed.
                // However, since Players is a separate list of type Player, it's less critical here unless WorldObject list contains Players.
                // Converters for IComponent are handled by [JsonDerivedType] on IComponent interface.
            };
            return JsonSerializer.Serialize(this, options);
        }

        public static World? FromJson(string json)
        {
            var options = new JsonSerializerOptions { };
            var world = JsonSerializer.Deserialize<World>(json, options);

            // Post-deserialization steps, e.g., re-linking component parents
            if (world != null)
            {
                foreach (var obj in world.Objects)
                {
                    foreach (var component in obj.Components)
                    {
                        component.Parent = obj;
                    }
                }
                foreach (var player in world.Players)
                {
                    foreach (var component in player.Components) // Players inherit Components from WorldObject
                    {
                        component.Parent = player;
                    }
                }
            }
            return world;
        }
    }
}
