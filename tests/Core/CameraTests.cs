using Xunit;
using GameFramework; // For Camera
using GameFramework.Core; // For WorldObject
using System;
using System.Numerics; // For Vector3

namespace GameFramework.Tests.Core
{
    public class CameraTests
    {
        [Fact]
        public void Camera_Creation_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var camera = new Camera(10, 20, 30, 90.0f, 1.77f, 0.5f, 1500.0f);

            // Assert
            Assert.Equal(10, camera.X);
            Assert.Equal(20, camera.Y);
            Assert.Equal(30, camera.Z);
            Assert.Null(camera.AttachedObject);
            Assert.Equal(90.0f, camera.FieldOfView);
            Assert.Equal(1.77f, camera.AspectRatio);
            Assert.Equal(0.5f, camera.NearPlaneDistance);
            Assert.Equal(1500.0f, camera.FarPlaneDistance);
        }

        [Fact]
        public void Camera_AttachToObject_ShouldSetAttachedObjectAndUpdatePosition()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30);

            // Act
            camera.AttachToObject(worldObject);

            // Assert
            Assert.Same(worldObject, camera.AttachedObject);
            Assert.Equal(worldObject.X, camera.X);
            Assert.Equal(worldObject.Y, camera.Y);
            Assert.Equal(worldObject.Z, camera.Z);
        }

        [Fact]
        public void Camera_AttachToObject_NullObject_ShouldSetAttachedObjectToNull()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject); // Attach first

            // Act
            camera.AttachToObject(null!); // Attach to null

            // Assert
            Assert.Null(camera.AttachedObject);
            // Position should remain as it was from the last attached object or manual set
            Assert.Equal(worldObject.X, camera.X); 
            Assert.Equal(worldObject.Y, camera.Y);
            Assert.Equal(worldObject.Z, camera.Z);
        }


        [Fact]
        public void Camera_DetachObject_ShouldClearAttachedObject()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject);

            // Act
            camera.DetachObject();

            // Assert
            Assert.Null(camera.AttachedObject);
            // Position should remain as it was from the last attached object
            Assert.Equal(10, camera.X);
            Assert.Equal(20, camera.Y);
            Assert.Equal(30, camera.Z);
        }

        [Fact]
        public void Camera_UpdatePosition_WhenAttached_ShouldSyncWithObject()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject);

            // Act
            worldObject.SetPosition(40, 50, 60);
            camera.UpdatePosition();

            // Assert
            Assert.Equal(40, camera.X);
            Assert.Equal(50, camera.Y);
            Assert.Equal(60, camera.Z);
        }

        [Fact]
        public void Camera_UpdatePosition_WhenNotAttached_ShouldNotChangePosition()
        {
            // Arrange
            var camera = new Camera(5, 15, 25);
            var initialX = camera.X;
            var initialY = camera.Y;
            var initialZ = camera.Z;

            // Act
            camera.UpdatePosition(); // No object attached

            // Assert
            Assert.Equal(initialX, camera.X);
            Assert.Equal(initialY, camera.Y);
            Assert.Equal(initialZ, camera.Z);
        }

        [Fact]
        public void Camera_SetPosition_WhenNotAttached_ShouldUpdatePosition()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);

            // Act
            camera.SetPosition(100, 200, 300);

            // Assert
            Assert.Equal(100, camera.X);
            Assert.Equal(200, camera.Y);
            Assert.Equal(300, camera.Z);
        }

        [Fact]
        public void Camera_SetPosition_WhenAttached_ShouldNotUpdatePosition()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("obj1", "TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject); // Attached

            var initialX = camera.X; // Should be 10
            var initialY = camera.Y; // Should be 20
            var initialZ = camera.Z; // Should be 30


            // Act
            camera.SetPosition(100, 200, 300); // Attempt to manually set position

            // Assert
            // Position should remain synced with the attached object, not the manual set values
            Assert.Equal(initialX, camera.X);
            Assert.Equal(initialY, camera.Y);
            Assert.Equal(initialZ, camera.Z);
        }
    }
}
