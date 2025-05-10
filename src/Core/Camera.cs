using System;

namespace GameFramework
{
    public class Camera
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; } // Added Z-coordinate
        public WorldObject? AttachedObject { get; private set; }

        public float FieldOfView { get; private set; }
        public float AspectRatio { get; private set; }
        public float NearPlaneDistance { get; private set; }
        public float FarPlaneDistance { get; private set; }

        public Camera(int x, int y, int z, float fieldOfView = 60.0f, float aspectRatio = 16.0f / 9.0f, float nearPlaneDistance = 0.1f, float farPlaneDistance = 1000.0f) // Added z parameter
        {
            X = x;
            Y = y;
            Z = z; // Initialize Z
            AttachedObject = null;
            FieldOfView = fieldOfView;
            AspectRatio = aspectRatio;
            NearPlaneDistance = nearPlaneDistance;
            FarPlaneDistance = farPlaneDistance;
        }

        public void AttachToObject(WorldObject worldObject)
        {
            AttachedObject = worldObject;
            if (AttachedObject != null)
            {
                UpdatePosition(); // Initial sync of position upon attachment
            }
        }

        public void DetachObject()
        {
            AttachedObject = null;
        }

        public void UpdatePosition()
        {
            if (AttachedObject != null)
            {
                X = AttachedObject.X;
                Y = AttachedObject.Y;
                Z = AttachedObject.Z; // Update Z from attached object
            }
        }

        // Optional: Method to manually set camera position if not attached
        public void SetPosition(int x, int y, int z) // Added z parameter
        {
            if (AttachedObject == null) // Only allow manual position set if not attached
            {
                X = x;
                Y = y;
                Z = z; // Set Z
            }
            // Potentially log a warning or throw an exception if trying to set position while attached
        }
    }
}
