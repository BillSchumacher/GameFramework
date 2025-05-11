using Xunit;
using GameFramework.UI;
using System;

namespace GameFramework.Tests.Core
{
    public class CheckboxWidgetTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            string id = "testCheckbox";
            int x = 10;
            int y = 20;
            string label = "Enable Feature";
            bool isChecked = true;

            // Act
            var checkbox = new CheckboxWidget(id, x, y, label, isChecked);

            // Assert
            Assert.Equal(id, checkbox.Id);
            Assert.Equal(x, checkbox.X);
            Assert.Equal(y, checkbox.Y);
            Assert.Equal(label, checkbox.Label);
            Assert.Equal(isChecked, checkbox.IsChecked);
            Assert.True(checkbox.IsVisible); // From base Widget class
        }

        [Fact]
        public void Constructor_NullOrEmptyLabel_ThrowsArgumentException()
        {
            // Arrange
            string id = "testCheckbox";
            int x = 10;
            int y = 20;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CheckboxWidget(id, x, y, null));
            Assert.Throws<ArgumentException>(() => new CheckboxWidget(id, x, y, ""));
            Assert.Throws<ArgumentException>(() => new CheckboxWidget(id, x, y, "   "));
        }

        [Fact]
        public void Toggle_ChangesCheckedState_And_InvokesOnCheckedChangedEvent()
        {
            // Arrange
            var checkbox = new CheckboxWidget("chk1", 0, 0, "Test Checkbox", false);
            bool eventFired = false;
            bool? eventState = null;

            checkbox.OnCheckedChanged += (isChecked) => 
            {
                eventFired = true;
                eventState = isChecked;
            };

            // Act: Toggle from false to true
            checkbox.Toggle();

            // Assert
            Assert.True(checkbox.IsChecked);
            Assert.True(eventFired);
            Assert.True(eventState.HasValue && eventState.Value);

            // Reset for next toggle
            eventFired = false;
            eventState = null;

            // Act: Toggle from true to false
            checkbox.Toggle();

            // Assert
            Assert.False(checkbox.IsChecked);
            Assert.True(eventFired);
            Assert.True(eventState.HasValue && !eventState.Value);
        }
        
        [Fact]
        public void Toggle_NoSubscribers_DoesNotThrow()
        {
            // Arrange
            var checkbox = new CheckboxWidget("chkNoSub", 0, 0, "No Subscribers");

            // Act
            var ex = Record.Exception(() => checkbox.Toggle());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void SetChecked_UpdatesState_And_InvokesEvent_WhenChanged()
        {
            // Arrange
            var checkbox = new CheckboxWidget("chkSet", 0, 0, "Set Checkbox", false);
            bool eventFired = false;
            bool? eventState = null;
            checkbox.OnCheckedChanged += (isChecked) =>
            {
                eventFired = true;
                eventState = isChecked;
            };

            // Act: Set to true (state changes)
            checkbox.SetChecked(true);

            // Assert
            Assert.True(checkbox.IsChecked);
            Assert.True(eventFired);
            Assert.True(eventState.HasValue && eventState.Value);

            // Reset
            eventFired = false;
            eventState = null;

            // Act: Set to true again (state does not change)
            checkbox.SetChecked(true);

            // Assert: Event should not fire again
            Assert.False(eventFired);
            Assert.Null(eventState);
            Assert.True(checkbox.IsChecked); // State remains true
        }
        
        [Fact]
        public void Draw_WhenVisible_PrintsDrawingTextWithLabelAndState()
        {
            // Arrange
            var checkboxChecked = new CheckboxWidget("chkDrawVisibleTrue", 5, 5, "Visible Checked", true);
            var checkboxUnchecked = new CheckboxWidget("chkDrawVisibleFalse", 6, 6, "Visible Unchecked", false);
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act: Checked
            checkboxChecked.Show();
            checkboxChecked.Draw();
            var outputChecked = stringWriter.ToString();

            // Assert: Checked
            Assert.Contains($"Drawing CheckboxWidget {checkboxChecked.Id} with label \"{checkboxChecked.Label}\" [X] at ({checkboxChecked.X}, {checkboxChecked.Y})", outputChecked);

            // Clear writer for next test
            stringWriter.GetStringBuilder().Clear();

            // Act: Unchecked
            checkboxUnchecked.Show();
            checkboxUnchecked.Draw();
            var outputUnchecked = stringWriter.ToString();
            
            // Assert: Unchecked
            Assert.Contains($"Drawing CheckboxWidget {checkboxUnchecked.Id} with label \"{checkboxUnchecked.Label}\" [ ] at ({checkboxUnchecked.X}, {checkboxUnchecked.Y})", outputUnchecked);
            
            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }

        [Fact]
        public void Draw_WhenNotVisible_DoesNotPrint()
        {
            // Arrange
            var checkbox = new CheckboxWidget("chkDrawHidden", 5, 5, "Hidden Checkbox");
            var stringWriter = new System.IO.StringWriter();
            Console.SetOut(stringWriter);

            // Act
            checkbox.Hide(); // Ensure it's not visible
            checkbox.Draw();
            var output = stringWriter.ToString();

            // Assert
            Assert.Empty(output.Trim());

            // Reset console output
            var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
    }
}
