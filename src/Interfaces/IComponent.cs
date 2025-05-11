using GameFramework.Core;
using System.Text.Json.Serialization; // Added for JsonDerivedType

namespace GameFramework
{
    /// <summary>
    /// Interface for all components that can be attached to a WorldObject.
    /// </summary>
    [JsonDerivedType(typeof(MeshComponent), "MeshComponent")]
    [JsonDerivedType(typeof(SpriteComponent), "SpriteComponent")]
    [JsonDerivedType(typeof(LightComponent), "LightComponent")]
    // Add other component types here as they are created
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the WorldObject this component is attached to.
        /// </summary>
        [JsonIgnore] // Parent will be set during WorldObject deserialization, avoid circular refs
        global::GameFramework.Core.WorldObject? Parent { get; set; }

        /// <summary>
        /// Called when the component is added to a WorldObject.
        /// </summary>
        void OnAttach();

        /// <summary>
        /// Called when the component is removed from a WorldObject.
        /// </summary>
        void OnDetach();

        /// <summary>
        /// Called every frame to update the component's state.
        /// </summary>
        void Update();
    }
}
