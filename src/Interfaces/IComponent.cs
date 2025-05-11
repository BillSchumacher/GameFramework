using GameFramework.Core;

namespace GameFramework
{
    /// <summary>
    /// Interface for all components that can be attached to a WorldObject.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Gets or sets the WorldObject this component is attached to.
        /// </summary>
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
