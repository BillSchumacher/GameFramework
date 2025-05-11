using System;

namespace GameFramework.UI // Changed namespace
{
    public class CheckboxWidget : Widget
    {
        public string Label { get; private set; }
        public bool IsChecked { get; private set; }
        public event Action<bool>? OnCheckedChanged; // Made nullable

        public CheckboxWidget(string id, int x, int y, string label, bool initialIsChecked = false) : base(id, x, y)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Checkbox label cannot be null or empty.", nameof(label));
            }
            Label = label;
            IsChecked = initialIsChecked;
        }

        public void Toggle()
        {
            IsChecked = !IsChecked;
            OnCheckedChanged?.Invoke(IsChecked);
        }

        public void SetChecked(bool isChecked)
        {
            if (IsChecked != isChecked)
            {
                IsChecked = isChecked;
                OnCheckedChanged?.Invoke(IsChecked);
            }
        }

        public override void Draw()
        {
            if (IsVisible)
            {
                Console.WriteLine($"Drawing CheckboxWidget {Id} with label \"{Label}\" [{(IsChecked ? "X" : " " )}] at ({X}, {Y})");
            }
        }
    }
}
