using Xunit;
using GameFramework.Core;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using GameFramework; // For IComponent concrete types

namespace GameFramework.Tests.Core
{
    public class WorldSerializationTests
    {
        private JsonSerializerOptions GetJsonOptions()
        {
            // Options can be configured here if needed, e.g., for converters
            return new JsonSerializerOptions { WriteIndented = true };
        }

        [Fact]
        public void WorldObject_SerializeDeserialize_ShouldPreservePropertiesAndComponents()
        {
            var originalObject = new WorldObject("testObj", "Test Object", 1, 2, 3);
            var meshComp = new MeshComponent(new List<System.Numerics.Vector3> { new System.Numerics.Vector3(1,1,1) }, new List<int> { 0,1,2 }, new List<System.Numerics.Vector2> { new System.Numerics.Vector2(0,0) });
            originalObject.AddComponent(meshComp);

            string json = originalObject.ToJson();
            WorldObject deserializedObject = WorldObject.FromJson(json);

            Assert.NotNull(deserializedObject);
            Assert.Equal(originalObject.Id, deserializedObject.Id);
            Assert.Equal(originalObject.Name, deserializedObject.Name);
            Assert.Equal(originalObject.X, deserializedObject.X);
            Assert.Equal(originalObject.Y, deserializedObject.Y);
            Assert.Equal(originalObject.Z, deserializedObject.Z);
            Assert.Single(deserializedObject.Components);
            Assert.IsType<MeshComponent>(deserializedObject.Components.First());
            var deserializedMeshComp = deserializedObject.Components.First() as MeshComponent;
            Assert.NotNull(deserializedMeshComp);
            Assert.Equal(meshComp.Vertices.Count, deserializedMeshComp.Vertices.Count);
            Assert.Equal(meshComp.Vertices.First(), deserializedMeshComp.Vertices.First());
        }

        [Fact]
        public void Player_SerializeDeserialize_ShouldPreserveProperties()
        {
            var originalPlayer = new Player("player1", "Hero", 1000);
            originalPlayer.AddComponent(new SpriteComponent("sprites/hero.png", System.Drawing.Color.Blue));

            string json = originalPlayer.ToJson();
            Player deserializedPlayer = Player.FromJson(json);

            Assert.NotNull(deserializedPlayer);
            Assert.Equal(originalPlayer.Id, deserializedPlayer.Id);
            Assert.Equal(originalPlayer.Name, deserializedPlayer.Name);
            Assert.Equal(originalPlayer.Score, deserializedPlayer.Score);
            Assert.Single(deserializedPlayer.Components);
            Assert.IsType<SpriteComponent>(deserializedPlayer.Components.First());
            var spriteComp = deserializedPlayer.GetComponent<SpriteComponent>();
            Assert.NotNull(spriteComp);
            Assert.Equal("sprites/hero.png", spriteComp.SpritePath);
        }

        [Fact]
        public void World_SerializeDeserialize_ShouldPreserveObjectsAndPlayers()
        {
            var originalWorld = new World();
            var obj1 = new WorldObject("obj1", "Scenery", 10, 10, 0);
            obj1.AddComponent(new LightComponent(new GameFramework.Core.Light(LightType.Point, System.Drawing.Color.Yellow, System.Numerics.Vector3.Zero)));
            var player1 = new Player("p1", "Player One", 50);

            originalWorld.AddObject(obj1);
            originalWorld.AddPlayer(player1);

            string json = originalWorld.ToJson();
            World deserializedWorld = World.FromJson(json);

            Assert.NotNull(deserializedWorld);
            Assert.Single(deserializedWorld.Objects);
            Assert.Single(deserializedWorld.Players);

            var deserializedObj1 = deserializedWorld.GetObjectById("obj1");
            Assert.NotNull(deserializedObj1);
            Assert.Equal(obj1.Name, deserializedObj1.Name);
            Assert.Single(deserializedObj1.Components);
            Assert.IsType<LightComponent>(deserializedObj1.Components.First());

            var deserializedPlayer1 = deserializedWorld.GetPlayerById("p1");
            Assert.NotNull(deserializedPlayer1);
            Assert.Equal(player1.Name, deserializedPlayer1.Name);
            Assert.Equal(player1.Score, deserializedPlayer1.Score);
        }
    }
}
