using System.Drawing;
using System.Numerics;

namespace GameFramework
{
    /// <summary>
    /// Represents a light source in the game world.
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Gets or sets a value indicating whether the light is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        public Color LightColor { get; set; }

        /// <summary>
        /// Gets or sets the intensity of the light.
        /// </summary>
        public float Intensity { get; set; }

        /// <summary>
        /// Gets or sets the position of the light in 3D space.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets the type of the light.
        /// </summary>
        public LightType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Light"/> class.
        /// </summary>
        /// <param name="type">The type of the light.</param>
        /// <param name="color">The color of the light.</param>
        /// <param name="position">The position of the light.</param>
        /// <param name="intensity">The intensity of the light (0.0 to 1.0 or higher for HDR).</param>
        /// <param name="isEnabled">Whether the light is initially enabled.</param>
        public Light(LightType type, Color color, Vector3 position, float intensity = 1.0f, bool isEnabled = true)
        {
            if (intensity < 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(intensity), "Intensity cannot be negative.");
            }

            Type = type;
            LightColor = color;
            Position = position;
            Intensity = intensity;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Toggles the enabled state of the light.
        /// </summary>
        public void Toggle()
        {
            IsEnabled = !IsEnabled;
        }
    }
}
