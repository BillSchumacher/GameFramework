using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using OpenTK.Windowing.GraphicsLibraryFramework; // Added for MouseButton
using OpenTK.Mathematics; // Added for Matrix4

namespace GameFramework.UI
{
    public abstract class StackPanelWidget : Widget
    {
        public int Spacing { get; set; } 
        public enum Orientation { Vertical, Horizontal }
        public Orientation PanelOrientation { get; set; }
        public List<Widget> Children { get; private set; } = new List<Widget>();

        protected StackPanelWidget() : this("default_stackpanel_id", 0, 0, 100, 100, Orientation.Vertical) 
        {
        }

        [JsonConstructor]
        protected StackPanelWidget(string id, int x, int y, int width, int height, Orientation orientation = Orientation.Vertical, AnchorPoint anchor = AnchorPoint.Manual, int offsetX = 0, int offsetY = 0)
            : base(id, x, y, anchor, offsetX, offsetY)
        {
            WidgetWidth = width;
            WidgetHeight = height;
            PanelOrientation = orientation;
        }

        public virtual void AddChild(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }
            if (Children.Contains(widget))
            {
                throw new InvalidOperationException($"Widget '{widget.Id}' is already a child of '{this.Id}'.");
            }
            Children.Add(widget);
            RecalculateLayout();
        }

        public virtual bool RemoveChild(Widget widget)
        {
            if (widget != null && Children.Remove(widget))
            {
                RecalculateLayout();
                return true;
            }
            return false;
        }

        public abstract void RecalculateLayout();

        public override void Draw(float elapsedTime, Matrix4 projectionMatrix) // Added projectionMatrix
        {
            base.Draw(elapsedTime, projectionMatrix); // Call base to draw background if any
            
            // Children are drawn by the PanelWidget's Draw method if this StackPanel is a child of a Panel
            // Or, if StackPanel directly manages drawing its children (which it should for layout)
            foreach (var child in Children.Where(c => c.IsVisible))
            {
                // child.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight); // Ensure child positions are relative to stack panel
                child.Draw(elapsedTime, projectionMatrix); // Pass projectionMatrix
            }
        }

        public override void UpdateActualPosition(float parentActualX, float parentActualY, float containerWidth, float containerHeight)
        {
            // Update this panel's position first based on its parent container and anchor settings.
            base.UpdateActualPosition(parentActualX, parentActualY, containerWidth, containerHeight);

            // RecalculateLayout is responsible for setting the relative X, Y of children within this panel
            // and potentially adjusting this panel's WidgetWidth/WidgetHeight based on its content.
            RecalculateLayout();

            // After layout, update the final screen coordinates of children.
            foreach (var child in Children)
            {
                // Children's X, Y (relative to this panel) should have been set by RecalculateLayout.
                // Now, calculate their ActualX, ActualY based on this panel's ActualX, ActualY.
                child.UpdateActualPosition(this.ActualX, this.ActualY, this.WidgetWidth, this.WidgetHeight);
            }
        }

        public override bool HitTest(float screenX, float screenY)
        {
            if (!IsVisible) return false;

            bool selfHit = screenX >= ActualX && screenX <= ActualX + WidgetWidth &&
                           screenY >= ActualY && screenY <= ActualY + WidgetHeight;

            if (selfHit)
            {
                foreach (var child in Children)
                {
                    if (child.HitTest(screenX, screenY))
                    {
                        return true; // A child was hit
                    }
                }
                return true; // Panel itself was hit
            }
            return false;
        }

        public override bool OnMouseDown(float mouseX, float mouseY, MouseButton mouseButton)
        {
            if (HitTest(mouseX, mouseY))
            {
                foreach (var child in Children)
                {
                    if (child.OnMouseDown(mouseX, mouseY, mouseButton))
                    {
                        return true; // Event handled by a child
                    }
                }
                // If no child handled it, the panel itself handles the event as it was within bounds.
                return true; 
            }
            return false;
        }

        public override bool OnMouseUp(float mouseX, float mouseY, MouseButton mouseButton)
        {
            bool handledByChild = false;
            foreach (var child in Children)
            {
                if (child.OnMouseUp(mouseX, mouseY, mouseButton))
                {
                    handledByChild = true;
                    // Do not break; allow multiple children to process MouseUp if needed (e.g., drag release)
                }
            }
            
            if (HitTest(mouseX, mouseY)) // Check if the mouse up occurred on the panel itself
            {
                // Panel specific mouse up logic can go here
                return true; // Panel considers event handled if it occurred within its bounds
            }
            return handledByChild; // Return true if any child handled it, or if panel handled it.
        }

        public override void OnMouseMove(float mouseX, float mouseY, float deltaX, float deltaY)
        {
            foreach (var child in Children)
            {
                child.OnMouseMove(mouseX, mouseY, deltaX, deltaY);
            }
            // Panel specific mouse move logic can go here (e.g. if panel is draggable)
        }

        protected int GetMaxChildWidth()
        {
            if (Children.Count == 0) return 0;
            return Children.Max(child => child.WidgetWidth);
        }

        protected int GetMaxChildHeight()
        {
            if (Children.Count == 0) return 0;
            return Children.Max(child => child.WidgetHeight);
        }
    }
}
