using System.Collections.Generic;
using System.Numerics;

namespace GameFramework
{
    public class MeshComponent : IComponent
    {
        public WorldObject? Parent { get; set; } // Made Parent nullable
        public List<Vector3> Vertices { get; private set; }
        public List<int> Indices { get; private set; }
        public List<Vector2> UVs { get; private set; }

        public MeshComponent()
        {
            Vertices = new List<Vector3>();
            Indices = new List<int>();
            UVs = new List<Vector2>();
        }

        public void OnAttach()
        {
            // Logic to handle attachment, e.g., registering with a rendering system
            if (Parent != null)
            {
                // Example: Parent.RegisterComponent(this);
            }
        }

        public void OnDetach()
        {
            // Logic to handle detachment, e.g., unregistering from a rendering system
            if (Parent != null)
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

        // Method to load or define mesh data
        public void SetMesh(List<Vector3> vertices, List<int> indices, List<Vector2> uvs)
        {
            Vertices = vertices ?? new List<Vector3>();
            Indices = indices ?? new List<int>();
            UVs = uvs ?? new List<Vector2>();
            // Potentially trigger a refresh or update if the mesh is already in use
        }
    }
}
