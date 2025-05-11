using Xunit;
using GameFramework;

namespace GameFramework.Tests
{
    public class CameraTests
    {
        [Fact]
        public void Camera_Creation_ShouldInitializeProperties()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);

            // Assert
            Assert.Equal(0, camera.X);
            Assert.Equal(0, camera.Y);
            Assert.Equal(0, camera.Z);
            Assert.Null(camera.AttachedObject);
        }

        [Fact]
        public void Camera_Creation_ShouldInitializeFrustumProperties()
        {
            // Arrange
            var camera = new Camera(0, 0, 0, 60.0f, 16.0f / 9.0f, 0.1f, 1000.0f);

            // Assert
            Assert.Equal(60.0f, camera.FieldOfView);
            Assert.Equal(16.0f / 9.0f, camera.AspectRatio);
            Assert.Equal(0.1f, camera.NearPlaneDistance);
            Assert.Equal(1000.0f, camera.FarPlaneDistance);
        }

        [Fact]
        public void Camera_AttachToObject_ShouldSetAttachedObject()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("TestObject", 10, 20, 30);

            // Act
            camera.AttachToObject(worldObject);

            // Assert
            Assert.NotNull(camera.AttachedObject);
            Assert.Equal(worldObject.Id, camera.AttachedObject.Id);
        }

        [Fact]
        public void Camera_UpdatePosition_ShouldReflectAttachedObjectPosition()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject);

            // Act
            // Simulate game loop updating camera or camera polling object
            camera.UpdatePosition(); 

            // Assert
            Assert.Equal(10, camera.X);
            Assert.Equal(20, camera.Y);
            Assert.Equal(30, camera.Z);

            // Move the object and update again
            worldObject.SetPosition(30, 40, 50);
            camera.UpdatePosition();

            Assert.Equal(30, camera.X);
            Assert.Equal(40, camera.Y);
            Assert.Equal(50, camera.Z);
        }

        [Fact]
        public void Camera_UpdatePosition_ShouldRemainUnchangedIfNotAttached()
        {
            // Arrange
            var camera = new Camera(5, 5, 5);

            // Act
            camera.UpdatePosition();

            // Assert
            Assert.Equal(5, camera.X);
            Assert.Equal(5, camera.Y);
            Assert.Equal(5, camera.Z);
        }

        [Fact]
        public void Camera_DetachObject_ShouldClearAttachedObject()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("TestObject", 10, 20, 30);
            camera.AttachToObject(worldObject);
            camera.UpdatePosition(); // Initial position sync

            // Act
            camera.DetachObject();
            worldObject.SetPosition(100, 200, 300); // Move object after detaching
            camera.UpdatePosition(); // Try to update camera position

            // Assert
            Assert.Null(camera.AttachedObject);
            Assert.Equal(10, camera.X); // Camera position should not change
            Assert.Equal(20, camera.Y); // Camera position should not change
            Assert.Equal(30, camera.Z); // Camera position should not change
        }

        [Fact]
        public void Camera_SetPosition_ShouldUpdateXYZ_WhenNotAttached()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);

            // Act
            camera.SetPosition(10, 20, 30);

            // Assert
            Assert.Equal(10, camera.X);
            Assert.Equal(20, camera.Y);
            Assert.Equal(30, camera.Z);
        }

        [Fact]
        public void Camera_SetPosition_ShouldNotUpdateXYZ_WhenAttached()
        {
            // Arrange
            var camera = new Camera(0, 0, 0);
            var worldObject = new WorldObject("AttachedObj", 5, 5, 5);
            camera.AttachToObject(worldObject);
            camera.UpdatePosition(); // Sync with attached object

            // Act
            camera.SetPosition(10, 20, 30); // Attempt to manually set position

            // Assert
            // Position should remain that of the attached object
            Assert.Equal(5, camera.X);
            Assert.Equal(5, camera.Y);
            Assert.Equal(5, camera.Z);
        }
    }
}
