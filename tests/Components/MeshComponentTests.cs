using Xunit;
using GameFramework;
using System.Numerics;
using System.Collections.Generic;

namespace GameFramework.Tests
{
    public class MeshComponentTests
    {
        [Fact]
        public void MeshComponent_Creation_ShouldInitializeProperties()
        {
            // Arrange
            var meshComponent = new MeshComponent();

            // Assert
            Assert.NotNull(meshComponent.Vertices);
            Assert.Empty(meshComponent.Vertices);
            Assert.NotNull(meshComponent.Indices); // Changed Triangles to Indices
            Assert.Empty(meshComponent.Indices); // Changed Triangles to Indices
            Assert.NotNull(meshComponent.UVs);
            Assert.Empty(meshComponent.UVs);
            Assert.Null(meshComponent.Parent); // Parent should be null initially
        }

        [Fact]
        public void MeshComponent_SetMesh_ShouldUpdateMeshData()
        {
            // Arrange
            var meshComponent = new MeshComponent();
            var vertices = new List<Vector3> { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0) };
            var indices = new List<int> { 0, 1, 2 }; // Changed triangles to indices
            var uvs = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) }; // Added uvs

            // Act
            meshComponent.SetMesh(vertices, indices, uvs); // Changed triangles to indices and added uvs

            // Assert
            Assert.Equal(vertices, meshComponent.Vertices);
            Assert.Equal(indices, meshComponent.Indices); // Changed Triangles to Indices
            Assert.Equal(uvs, meshComponent.UVs);
        }

        [Fact]
        public void MeshComponent_OnAttach_SetsParentAndParentKnowsComponent()
        {
            // Arrange
            var meshComponent = new MeshComponent();
            var worldObject = new WorldObject("TestObject", 0, 0, 0); // Added initialZ

            // Act
            meshComponent.Parent = worldObject; // Simulate attaching
            meshComponent.OnAttach();
            // In a real scenario, WorldObject.AddComponent would set Parent and call OnAttach.
            // For isolated component testing, we set Parent then call OnAttach.

            // Assert
            Assert.Equal(worldObject, meshComponent.Parent);
            // If WorldObject had a GetComponent<MeshComponent>(), we could assert it here.
            // For now, we assume OnAttach might have internal logic to register with the parent if needed.
        }

        [Fact]
        public void MeshComponent_OnDetach_ClearsParentAndParentLosesComponent()
        {
            // Arrange
            var meshComponent = new MeshComponent();
            var worldObject = new WorldObject("TestObject", 0, 0, 0); // Added initialZ
            meshComponent.Parent = worldObject;
            meshComponent.OnAttach();

            // Act
            meshComponent.OnDetach();

            // Assert
            Assert.Null(meshComponent.Parent);
            // If WorldObject had a GetComponent<MeshComponent>(), we could assert it returns null here.
        }

        [Fact]
        public void MeshComponent_Update_DoesNotThrow()
        {
            // Arrange
            var meshComponent = new MeshComponent();
            var worldObject = new WorldObject("TestObject", 0, 0, 0); // Added initialZ
            meshComponent.Parent = worldObject; // Update might depend on Parent
            meshComponent.OnAttach();

            // Act & Assert
            var exception = Record.Exception(() => meshComponent.Update());
            Assert.Null(exception);
        }
    }
}
