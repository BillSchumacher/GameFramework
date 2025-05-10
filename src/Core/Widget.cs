using System;

namespace GameFramework
{
    public class Widget
    {
        public string Id { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsVisible { get; private set; }

        public Widget(string id, int x, int y)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Widget ID cannot be null or empty.", nameof(id));
            }
            Id = id;
            X = x;
            Y = y;
            IsVisible = true; // Widgets are visible by default
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Placeholder for a virtual Draw method, to be overridden by specific widget types
        public virtual void Draw()
        {
            if (IsVisible)
            {
                // Basic drawing logic, could be extended in derived classes
                Console.WriteLine($"Drawing Widget {Id} at ({X}, {Y})");
            }
        }
    }
}
