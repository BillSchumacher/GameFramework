using System;
using GameFramework.Core; // This directive should bring Light, LightType, and WorldObject into scope
using System.Text.Json.Serialization;
using System.Numerics;    // For Vector3, used by GameFramework.Core.Light constructor
using System.Drawing;     // For Color, used by GameFramework.Core.Light constructor

namespace GameFramework // Namespace of this LightComponent.cs file
{
    /// <summary>
    /// Represents a light as a component that can be attached to a WorldObject.
    /// </summary>
    public class LightComponent : IComponent
    {
        /// <summary>
        /// Gets the Light object managed by this component.
        /// </summary>
        public Light Light { get; set; }

        /// <summary>
        /// Gets or sets the WorldObject this component is attached to.
        /// </summary>
        [JsonIgnore] // Parent is handled by WorldObject deserialization
        public WorldObject? Parent { get; set; }

        /// <summary>
        /// Parameterless constructor for JSON deserialization.
        /// </summary>
        public LightComponent()
            : this(new Light(LightType.Point, Color.White, Vector3.Zero, 1.0f, true))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponent"/> class.
        /// </summary>
        /// <param name="light">The light to associate with this component.</param>
        [JsonConstructor] // Hint for serializer
        public LightComponent(Light light)
        {
            Light = light ?? throw new ArgumentNullException(nameof(light));
        }

        /// <summary>
        /// Called when the component is added to a WorldObject.
        /// </summary>
        public void OnAttach()
        {
            // Specific logic for when the light component is attached can go here.
        }

        /// <summary>
        /// Called when the component is removed from a WorldObject.
        /// </summary>
        public void OnDetach()
        {
            // Specific logic for when the light component is detached.
            Parent = null;
        }

        /// <summary>
        /// Called every frame to update the component's state.
        /// </summary>
        public void Update()
        {
            // Light position could be updated relative to the Parent WorldObject if needed.
            if (Parent is not null && Light != null)
            {
                // Example: Make the light follow the parent object.
            }
        }
    }
}
