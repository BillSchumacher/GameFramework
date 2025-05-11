using System;
using System.Collections.Generic; // Required for List
using System.Linq; // Required for FirstOrDefault
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameFramework.Core
{
    public class WorldObject
    {
        public string Id { get; set; } // Made settable
        public string Name { get; set; } // Made settable
        public int X { get; set; } // Made settable
        public int Y { get; set; } // Made settable
        public int Z { get; set; } // Made settable
        public List<IComponent> Components { get; set; } // Made settable

        // Parameterless constructor for JSON deserialization
        public WorldObject() : this(Guid.NewGuid().ToString(), "Default Name", 0, 0, 0)
        {
        }

        [JsonConstructor]
        public WorldObject(string id, string name, int x, int y, int z)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));
            }
            Id = id;
            Name = name;
            X = x;
            Y = y;
            Z = z;
            Components = new List<IComponent>();
        }

        public void SetPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void AddComponent(IComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (!Components.Contains(component))
            {
                Components.Add(component);
                component.Parent = this;
                component.OnAttach();
            }
        }

        public bool RemoveComponent(IComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (Components.Contains(component))
            {
                component.OnDetach();
                component.Parent = null;
                return Components.Remove(component);
            }
            return false;
        }

        public T? GetComponent<T>() where T : class, IComponent
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        public void UpdateComponents()
        {
            foreach (var component in Components)
            {
                component.Update();
            }
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            return JsonSerializer.Serialize(this, options);
        }

        public static WorldObject? FromJson(string json)
        {
            var options = new JsonSerializerOptions();
            var worldObject = JsonSerializer.Deserialize<WorldObject>(json, options);

            if (worldObject != null)
            {
                foreach (var component in worldObject.Components)
                {
                    component.Parent = worldObject;
                }
            }
            return worldObject;
        }
    }
}
