using Xunit;
using GameFramework;
using GameFramework.UI;
using System.Text.Json;
using System.Linq;

namespace GameFramework.Tests.Core
{
    public class UserInterfaceSerializationTests
    {
        [Fact]
        public void UserInterface_SerializeDeserialize_ShouldPreserveWidgets()
        {
            var originalUI = new UserInterface();
            var button = new ButtonWidget("btn1", 10, 10, "My Button");
            var checkbox = new CheckboxWidget("chk1", 20, 20, "My Checkbox", true);
            
            originalUI.AddWidget(button);
            originalUI.AddWidget(checkbox);

            string json = originalUI.ToJson();
            UserInterface deserializedUI = UserInterface.FromJson(json);

            Assert.NotNull(deserializedUI);
            Assert.Equal(2, deserializedUI.Widgets.Count);

            var deserializedButton = deserializedUI.GetWidgetById("btn1") as ButtonWidget;
            Assert.NotNull(deserializedButton);
            Assert.Equal(button.Text, deserializedButton.Text);
            Assert.Equal(button.X, deserializedButton.X);

            var deserializedCheckbox = deserializedUI.GetWidgetById("chk1") as CheckboxWidget;
            Assert.NotNull(deserializedCheckbox);
            Assert.Equal(checkbox.Label, deserializedCheckbox.Label);
            Assert.Equal(checkbox.IsChecked, deserializedCheckbox.IsChecked);
        }

        [Fact]
        public void UserInterface_SerializeDeserialize_PanelWithChildren_ShouldPreserveHierarchy()
        {
            var originalUI = new UserInterface();
            var panel = new PanelWidget("panel1", 0, 0, 200, 100);
            var childButton = new ButtonWidget("childBtn", 5, 5, "Inside Panel");
            
            // Assuming PanelWidget.Children is a List<Widget> { get; set; } 
            // or PanelWidget has an AddChild method that populates such a list.
            // For this test, we'll assume direct manipulation or an AddChild method that works with the settable Children list.
            panel.Children.Add(childButton); // Direct add if Children is List<Widget>
            // If PanelWidget.AddChild(Widget) is the way:
            // panel.AddChild(childButton);

            originalUI.AddWidget(panel);

            string json = originalUI.ToJson();
            UserInterface deserializedUI = UserInterface.FromJson(json);

            Assert.NotNull(deserializedUI);
            var deserializedPanel = deserializedUI.GetWidgetById("panel1") as PanelWidget;
            Assert.NotNull(deserializedPanel);
            Assert.Single(deserializedPanel.Children);
            
            var deserializedChildButton = deserializedPanel.Children.First() as ButtonWidget;
            Assert.NotNull(deserializedChildButton);
            Assert.Equal(childButton.Text, deserializedChildButton.Text);
            Assert.Equal(childButton.Id, deserializedChildButton.Id);
        }
    }
}
