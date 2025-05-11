using System;
using GameFramework.Core;

namespace GameFramework
{
    /// <summary>
    /// Represents a light as a component that can be attached to a WorldObject.
    /// </summary>
    public class LightComponent : IComponent
    {
        /// <summary>
        /// Gets the Light object managed by this component.
        /// </summary>
        public Light Light { get; private set; }

        /// <summary>
        /// Gets or sets the WorldObject this component is attached to.
        /// </summary>
        public global::GameFramework.Core.WorldObject? Parent { get; set; } // Explicitly qualified with global::

        /// <summary>
        /// Initializes a new instance of the <see cref="LightComponent"/> class.
        /// </summary>
        /// <param name="light">The light to associate with this component.</param>
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
            // For example, registering the light with a rendering system if the Parent is active.
        }

        /// <summary>
        /// Called when the component is removed from a WorldObject.
        /// </summary>
        public void OnDetach()
        {
            // Specific logic for when the light component is detached.
            // For example, unregistering the light from a rendering system.
            Parent = null; // Set Parent to null
        }

        /// <summary>
        /// Called every frame to update the component's state.
        /// </summary>
        public void Update()
        {
            // Light position could be updated relative to the Parent WorldObject if needed.
            // For now, Light has its own Position property.
            // If the Light's position should follow the WorldObject, that logic would go here.
            if (Parent is not null && Light != null) // Used "is not null" for nullable reference type
            {
                // Example: Make the light follow the parent object. 
                // This assumes Light.Position is in world space and Parent.X, Y, Z are also world space.
                // If Light.Position should be an offset, this logic would need adjustment.
                // Light.Position = new System.Numerics.Vector3(Parent.X, Parent.Y, Parent.Z);
            }
        }
    }
}
