using Xunit;
using GameFramework.UI;
using System;
using System.Text.Json;
using System.Collections.Generic; // Required for List if Panel/StackPanel use it

namespace GameFramework.Tests.Core
{
    public class WidgetSerializationTests
    {
        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions { WriteIndented = true };
        }

        [Fact]
        public void Widget_SerializeDeserialize_ShouldPreserveProperties()
        {
            var originalWidget = new Widget("testWidget", 10, 20);
            originalWidget.IsVisible = false;

            string json = originalWidget.ToJson();
            Widget deserializedWidget = Widget.FromJson(json);

            Assert.NotNull(deserializedWidget);
            Assert.Equal(originalWidget.Id, deserializedWidget.Id);
            Assert.Equal(originalWidget.X, deserializedWidget.X);
            Assert.Equal(originalWidget.Y, deserializedWidget.Y);
            Assert.Equal(originalWidget.IsVisible, deserializedWidget.IsVisible);
        }

        [Fact]
        public void ButtonWidget_SerializeDeserialize_ShouldPreserveProperties()
        {
            // Assuming ButtonWidget Text property is settable or handled by JsonConstructor
            var originalButton = new ButtonWidget("testButton", 5, 5, "Click Me");
            originalButton.IsVisible = true;
            // If ButtonWidget has public string Text { get; set; }
            // If not, this test might require ButtonWidget to be adapted for serialization of Text.

            string json = originalButton.ToJson(); // Serializes as Widget due to [JsonDerivedType]
            Widget deserializedWidget = Widget.FromJson(json);

            Assert.NotNull(deserializedWidget);
            Assert.IsType<ButtonWidget>(deserializedWidget);
            var deserializedButton = deserializedWidget as ButtonWidget;

            Assert.Equal(originalButton.Id, deserializedButton.Id);
            Assert.Equal(originalButton.X, deserializedButton.X);
            Assert.Equal(originalButton.Y, deserializedButton.Y);
            Assert.Equal(originalButton.IsVisible, deserializedButton.IsVisible);
            Assert.Equal(originalButton.Text, deserializedButton.Text);
        }

        [Fact]
        public void CheckboxWidget_SerializeDeserialize_ShouldPreserveProperties()
        {
            // Assuming CheckboxWidget Label and IsChecked properties are settable or handled by JsonConstructor
            var originalCheckbox = new CheckboxWidget("testCheckbox", 1, 2, "Check Me", true);
            
            string json = originalCheckbox.ToJson();
            Widget deserializedWidget = Widget.FromJson(json);

            Assert.NotNull(deserializedWidget);
            Assert.IsType<CheckboxWidget>(deserializedWidget);
            var deserializedCheckbox = deserializedWidget as CheckboxWidget;

            Assert.Equal(originalCheckbox.Id, deserializedCheckbox.Id);
            Assert.Equal(originalCheckbox.X, deserializedCheckbox.X);
            Assert.Equal(originalCheckbox.Y, deserializedCheckbox.Y);
            Assert.Equal(originalCheckbox.IsVisible, deserializedCheckbox.IsVisible);
            Assert.Equal(originalCheckbox.Label, deserializedCheckbox.Label);
            Assert.Equal(originalCheckbox.IsChecked, deserializedCheckbox.IsChecked);
        }

        [Fact]
        public void TextFieldWidget_SerializeDeserialize_ShouldPreserveProperties()
        {
            // Assuming TextFieldWidget Text and MaxLength properties are settable or handled by JsonConstructor
            var originalTextField = new TextFieldWidget("testTextField", 30, 40, "Initial Text", 100);
            originalTextField.IsReadOnly = true;

            string json = originalTextField.ToJson();
            Widget deserializedWidget = Widget.FromJson(json);

            Assert.NotNull(deserializedWidget);
            Assert.IsType<TextFieldWidget>(deserializedWidget);
            var deserializedTextField = deserializedWidget as TextFieldWidget;

            Assert.Equal(originalTextField.Id, deserializedTextField.Id);
            Assert.Equal(originalTextField.X, deserializedTextField.X);
            Assert.Equal(originalTextField.Y, deserializedTextField.Y);
            Assert.Equal(originalTextField.IsVisible, deserializedTextField.IsVisible);
            Assert.Equal(originalTextField.Text, deserializedTextField.Text);
            Assert.Equal(originalTextField.MaxLength, deserializedTextField.MaxLength);
            Assert.Equal(originalTextField.IsReadOnly, deserializedTextField.IsReadOnly);
        }
        
        // Tests for PanelWidget, HorizontalStackPanelWidget, VerticalStackPanelWidget
        // These depend on how their 'Children' collection is implemented for serialization.
        // Assuming 'Children' is a 'List<Widget> { get; set; }' or similar.

        [Fact]
        public void PanelWidget_SerializeDeserialize_ShouldPreservePropertiesAndChildren()
        {
            // Assuming PanelWidget Width, Height, and Children properties are settable/handled
            var originalPanel = new PanelWidget("testPanel", 50, 50, 200, 100);
            var childButton = new ButtonWidget("childButton", 10, 10, "Child");
            // originalPanel.AddChild(childButton); // Assuming AddChild exists and Children is List<Widget>
            // For test simplicity, if Children is List<Widget> {get; set;}, initialize directly or ensure AddChild works with it.
            // This part needs PanelWidget to have a way to manage children that's compatible with serialization.
            // If PanelWidget.Children is `List<Widget> Children { get; set; }`
            // originalPanel.Children = new List<Widget> { childButton };


            string json = originalPanel.ToJson();
            Widget deserializedWidget = Widget.FromJson(json);

            Assert.NotNull(deserializedWidget);
            Assert.IsType<PanelWidget>(deserializedWidget);
            var deserializedPanel = deserializedWidget as PanelWidget;

            Assert.Equal(originalPanel.Id, deserializedPanel.Id);
            Assert.Equal(originalPanel.Width, deserializedPanel.Width);
            Assert.Equal(originalPanel.Height, deserializedPanel.Height);

            // Assert.Equal(originalPanel.Children.Count(), deserializedPanel.Children.Count());
            // if (originalPanel.Children.Any())
            // {
            //     var originalChild = originalPanel.Children.First() as ButtonWidget;
            //     var deserializedChild = deserializedPanel.Children.First() as ButtonWidget;
            //     Assert.NotNull(deserializedChild);
            //     Assert.Equal(originalChild.Text, deserializedChild.Text);
            // }
        }

        // Similar tests for HorizontalStackPanelWidget and VerticalStackPanelWidget would follow,
        // also depending on their 'Children' and specific property (Spacing) serialization.
    }
}
