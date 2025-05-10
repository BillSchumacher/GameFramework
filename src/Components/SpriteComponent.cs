using System;
using System.Drawing; // Required for Color

namespace GameFramework
{
    public class SpriteComponent : IComponent
    {
        public WorldObject? Parent { get; set; } // Made Parent nullable
        public string SpritePath { get; private set; }
        public Color Color { get; set; } // Tint color for the sprite
        // public Material Material { get; set; } // Future enhancement for custom shaders

        public SpriteComponent(string spritePath)
        {
            if (string.IsNullOrWhiteSpace(spritePath))
            {
                throw new ArgumentException("Sprite path cannot be null or whitespace.", nameof(spritePath));
            }
            SpritePath = spritePath;
            Color = Color.White; // Default to no tint
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
