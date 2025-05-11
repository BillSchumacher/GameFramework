using System.Collections.Generic;
using System.Numerics;
using GameFramework.Core; // Added using statement
using System.Text.Json.Serialization; // Added for JsonConstructor

namespace GameFramework
{
    public class MeshComponent : IComponent
    {
        [JsonIgnore] // Parent is handled by WorldObject deserialization
        public global::GameFramework.Core.WorldObject? Parent { get; set; } // Explicitly qualified with global::
        public List<Vector3> Vertices { get; set; } // Made settable
        public List<int> Indices { get; set; }    // Made settable
        public List<Vector2> UVs { get; set; }      // Made settable

        // Parameterless constructor for JSON deserialization
        public MeshComponent() : this(new List<Vector3>(), new List<int>(), new List<Vector2>())
        {
        }

        [JsonConstructor] // Hint for serializer
        public MeshComponent(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            Vertices = vertices ?? new List<Vector3>();
            Indices = indices ?? new List<int>();
            UVs = uvs ?? new List<Vector2>();
        }

        public void OnAttach()
        {
            // Logic when attached to a WorldObject
            if (Parent is not null) // Used "is not null" for nullable reference type
            {
                // Example: Parent.RegisterComponent(this);
            }
        }

        public void OnDetach()
        {
            // Logic to handle detachment, e.g., unregistering from a rendering system
            if (Parent is not null) // Used "is not null" for nullable reference type
            {
                // Example: Parent.UnregisterComponent(this);
            }
            Parent = null; // Set Parent to null on detach
        }

        public void Update()
        {
            // Logic to update the component each frame, if necessary
            // For a static mesh, this might be empty or handle animations
        }

        public void SetMesh(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            Vertices = vertices;
            Indices = indices;
            UVs = uvs;
            // Potentially trigger an update or event here
        }
    }
}
