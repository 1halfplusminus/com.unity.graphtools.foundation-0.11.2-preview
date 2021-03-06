using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GTFO.UIToolkitTests
{
    public class EditableLabelTests : BaseTestFixture
    {
        static readonly string k_SomeText = "Some text";

        [Test]
        public void SetValueWithoutNotifyDoesNotTriggerChangeCallback()
        {
            var editableLabel = new EditableLabel();
            bool called = false;
            editableLabel.RegisterCallback<ChangeEvent<string>>(e => called = true);
            m_Window.rootVisualElement.Add(editableLabel);
            editableLabel.SetValueWithoutNotify("Blah");

            Assert.IsFalse(called, "CollapsedButton called our callback.");
        }

        [UnityTest]
        public IEnumerator SingleClickOnEditableLabelDoesNotShowTextField()
        {
            var editableLabel = new EditableLabel();
            m_Window.rootVisualElement.Add(editableLabel);
            yield return null;

            var label = editableLabel.SafeQ(EditableLabel.labelName);
            var textField = editableLabel.SafeQ(EditableLabel.textFieldName);

            Assert.AreEqual(DisplayStyle.Flex, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, textField.resolvedStyle.display);

            var center = label.parent.LocalToWorld(label.layout.center);
            EventHelper.Click(center);

            Assert.AreEqual(DisplayStyle.Flex, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, textField.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator DoubleClickOnEditableLabelShowsTextField()
        {
            var editableLabel = new EditableLabel();
            m_Window.rootVisualElement.Add(editableLabel);
            yield return null;

            var label = editableLabel.SafeQ(EditableLabel.labelName);
            var textField = editableLabel.SafeQ(EditableLabel.textFieldName);
            var center = label.parent.LocalToWorld(label.layout.center);

            Assert.AreEqual(DisplayStyle.Flex, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, textField.resolvedStyle.display);

            EventHelper.Click(center, clickCount: 2);

            Assert.AreEqual(DisplayStyle.None, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, textField.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator EscapeCancelsEditing()
        {
            var editableLabel = new EditableLabel();
            editableLabel.SetValueWithoutNotify("My Text");
            string newValue = null;
            editableLabel.RegisterCallback<ChangeEvent<string>>(e => newValue = e.newValue);
            m_Window.rootVisualElement.Add(editableLabel);
            // Compute layout
            yield return null;

            var label = editableLabel.SafeQ(EditableLabel.labelName);
            var textField = editableLabel.SafeQ(EditableLabel.textFieldName);
            var center = label.parent.LocalToWorld(label.layout.center);

            // Activate text field
            EventHelper.Click(center, clickCount: 2);

            // Type some text
            EventHelper.Type(k_SomeText);

            // Type Escape
            EventHelper.KeyPressed(KeyCode.Escape);

            Assert.IsNull(newValue);
            Assert.AreEqual(DisplayStyle.Flex, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, textField.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator ReturnCommitsEditing()
        {
            var editableLabel = new EditableLabel();
            editableLabel.SetValueWithoutNotify("My Text");
            string newValue = null;
            editableLabel.RegisterCallback<ChangeEvent<string>>(e => newValue = e.newValue);
            m_Window.rootVisualElement.Add(editableLabel);
            // Compute layout
            yield return null;

            var label = editableLabel.SafeQ(EditableLabel.labelName);
            var textField = editableLabel.SafeQ(EditableLabel.textFieldName);
            var center = label.parent.LocalToWorld(label.layout.center);

            // Activate text field
            EventHelper.Click(center, clickCount: 2);

            // Type some text
            EventHelper.Type(k_SomeText);

            EventHelper.KeyPressed(KeyCode.Return);

            Assert.AreEqual(k_SomeText, newValue);
        }

        [UnityTest]
        public IEnumerator BlurCommitsEditing()
        {
            var editableLabel = new EditableLabel();
            editableLabel.SetValueWithoutNotify("My Text");
            string newValue = null;
            editableLabel.RegisterCallback<ChangeEvent<string>>(e => newValue = e.newValue);
            m_Window.rootVisualElement.Add(editableLabel);
            // Compute layout
            yield return null;

            var label = editableLabel.SafeQ(EditableLabel.labelName);
            var textField = editableLabel.SafeQ(EditableLabel.textFieldName);
            var center = label.parent.LocalToWorld(label.layout.center);

            // Activate text field
            EventHelper.Click(center, clickCount: 2);
            yield return null;

            // Type some text
            EventHelper.Type(k_SomeText);
            yield return null;

            // Blur the field
            EventHelper.Click(Vector2.zero);

            Assert.AreEqual(k_SomeText, newValue);
            Assert.AreEqual(DisplayStyle.Flex, label.resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, textField.resolvedStyle.display);
        }
    }
}
