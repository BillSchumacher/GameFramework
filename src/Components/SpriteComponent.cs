using System;
using System.Drawing; // Required for Color
using GameFramework.Core; // Added using statement
using System.Text.Json.Serialization; // Added for JsonConstructor

namespace GameFramework
{
    public class SpriteComponent : IComponent
    {
        [JsonIgnore] // Parent is handled by WorldObject deserialization
        public global::GameFramework.Core.WorldObject? Parent { get; set; } // Explicitly qualified with global::
        public string SpritePath { get; set; } // Made settable
        public Color Color { get; set; } // Tint color for the sprite, made settable
        // public Material Material { get; set; } // Future enhancement for custom shaders

        // Parameterless constructor for JSON deserialization
        public SpriteComponent() : this(string.Empty, Color.White)
        {
        }

        [JsonConstructor] // Hint for serializer
        public SpriteComponent(string spritePath, Color color)
        {
            if (string.IsNullOrWhiteSpace(spritePath) && GetType() == typeof(SpriteComponent)) // Allow derived classes to have no path initially
            {
                // Consider if this should throw or be handled by a default spritePath
                // For now, allowing empty for flexibility during deserialization if set later.
            }
            SpritePath = spritePath;
            Color = color;
        }

        public void OnAttach()
        {
            // Logic to perform when attached, e.g., load sprite texture, register with rendering system
            // Parent property is expected to be set before this method is called.
        }

        public void OnDetach()
        {
            // Logic to perform when detached, e.g., unload texture, unregister from rendering system
            Parent = null; // Set Parent to null
        }

        public void Update()
        {
            // Logic to update the component each frame, if necessary
            // For a static sprite, this might be empty or handle animations
        }
    }
}
