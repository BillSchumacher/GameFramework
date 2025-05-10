using System.Collections.Generic;
using System.Linq;

namespace GameFramework
{
    public class World
    {
        private readonly List<WorldObject> _objects;

        public World()
        {
            _objects = new List<WorldObject>();
        }

        public void AddObject(WorldObject worldObject)
        {
            if (worldObject != null && !_objects.Contains(worldObject))
            {
                _objects.Add(worldObject);
            }
        }

        public void RemoveObject(WorldObject worldObject)
        {
            _objects.Remove(worldObject);
        }

        public WorldObject? GetObjectById(string id) // Changed to WorldObject?
        {
            return _objects.FirstOrDefault(obj => obj.Id == id);
        }

        public IEnumerable<WorldObject> GetObjects()
        {
            return _objects.AsReadOnly();
        }
    }
}
