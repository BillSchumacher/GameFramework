using System;
using System.Collections.Generic; // Required for List
using System.Linq; // Required for FirstOrDefault

namespace GameFramework
{
    public class WorldObject
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; } // Added Z-coordinate

        /// <summary>
        /// Gets the list of components attached to this WorldObject.
        /// </summary>
        public List<IComponent> Components { get; private set; }

        public WorldObject(string name, int initialX, int initialY, int initialZ) // Added initialZ
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            X = initialX;
            Y = initialY;
            Z = initialZ; // Initialize Z
            Components = new List<IComponent>(); // Initialize components list
        }

        public void SetPosition(int x, int y, int z) // Added z parameter
        {
            X = x;
            Y = y;
            Z = z; // Set Z
        }

        /// <summary>
        /// Adds a component to this WorldObject.
        /// </summary>
        /// <param name="component">The component to add.</param>
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

        /// <summary>
        /// Removes a component from this WorldObject.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <returns>True if the component was removed, false otherwise.</returns>
        public bool RemoveComponent(IComponent component)
        {
            if (component == null) return false; // Or throw ArgumentNullException

            if (Components.Contains(component))
            {
                component.OnDetach();
                component.Parent = null;
                return Components.Remove(component);
            }
            return false;
        }

        /// <summary>
        /// Gets the first component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the component to retrieve.</typeparam>
        /// <returns>The component if found, otherwise null.</returns>
        public T? GetComponent<T>() where T : class, IComponent // Changed T to T?
        {
            return Components.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Updates all components attached to this WorldObject.
        /// </summary>
        public void UpdateComponents()
        {
            foreach (var component in Components)
            {
                component.Update();
            }
        }
    }
}
