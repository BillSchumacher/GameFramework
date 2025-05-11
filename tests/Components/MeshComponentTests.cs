using Xunit;
using GameFramework;
using GameFramework.Core; // For WorldObject
using System.Numerics; // For Vector3, Vector2
using System.Collections.Generic; // For List
using System; // For Exception

namespace GameFramework.Tests.Components
{
    public class MeshComponentTests
    {
        // Helper to create a default mesh component for tests
        private MeshComponent CreateDefaultMeshComponent()
        {
            var vertices = new List<Vector3> { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(0,1,0) };
            var indices = new List<int> { 0, 1, 2 };
            var uvs = new List<Vector2> { new Vector2(0,0), new Vector2(1,0), new Vector2(0,1) };
            return new MeshComponent(vertices, indices, uvs);
        }

        [Fact]
        public void MeshComponent_Creation_ShouldInitializeCorrectly()
        {
            // Arrange
            var vertices = new List<Vector3> { new Vector3(0,0,0) };
            var indices = new List<int> { 0 };
            var uvs = new List<Vector2> { new Vector2(0,0) };

            // Act
            var meshComponent = new MeshComponent(vertices, indices, uvs);

            // Assert
            Assert.Same(vertices, meshComponent.Vertices);
            Assert.Same(indices, meshComponent.Indices);
            Assert.Same(uvs, meshComponent.UVs);
            Assert.Null(meshComponent.Parent);
        }
        
        [Fact]
        public void MeshComponent_Creation_NullLists_ShouldInitializeEmptyLists()
        {
            // Act
            var meshComponent = new MeshComponent(null!, null!, null!);

            // Assert
            Assert.NotNull(meshComponent.Vertices);
            Assert.Empty(meshComponent.Vertices);
            Assert.NotNull(meshComponent.Indices);
            Assert.Empty(meshComponent.Indices);
            Assert.NotNull(meshComponent.UVs);
            Assert.Empty(meshComponent.UVs);
        }


        [Fact]
        public void MeshComponent_OnAttach_ShouldSetParent()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var meshComponent = CreateDefaultMeshComponent();

            // Act
            worldObject.AddComponent(meshComponent);

            // Assert
            Assert.Same(worldObject, meshComponent.Parent);
        }

        [Fact]
        public void MeshComponent_OnDetach_ShouldClearParent()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var meshComponent = CreateDefaultMeshComponent();
            worldObject.AddComponent(meshComponent);

            // Act
            worldObject.RemoveComponent(meshComponent);

            // Assert
            Assert.Null(meshComponent.Parent);
        }

        [Fact]
        public void MeshComponent_Update_ShouldNotThrowException()
        {
            // Arrange
            var worldObject = new WorldObject("obj1", "TestObject", 0, 0, 0);
            var meshComponent = CreateDefaultMeshComponent();
            worldObject.AddComponent(meshComponent);

            // Act & Assert
            Exception? ex = Record.Exception(() => meshComponent.Update());
            Assert.Null(ex);
        }

        [Fact]
        public void MeshComponent_SetMesh_ShouldUpdateProperties()
        {
            // Arrange
            var meshComponent = CreateDefaultMeshComponent();
            var newVertices = new List<Vector3> { new Vector3(1,1,1) };
            var newIndices = new List<int> { 0 };
            var newUvs = new List<Vector2> { new Vector2(1,1) };

            // Act
            meshComponent.SetMesh(newVertices, newIndices, newUvs);

            // Assert
            Assert.Same(newVertices, meshComponent.Vertices);
            Assert.Same(newIndices, meshComponent.Indices);
            Assert.Same(newUvs, meshComponent.UVs);
        }
    }
}
