using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GraphElements
{
    class GraphElementCopyPasteTests : GraphViewTester
    {
        const int k_DefaultNodeCount = 4;

        int m_SelectedNodeCount;

        static string SerializeGraphElementsImplementation(IEnumerable<IGraphElementModel> elements)
        {
            // A real implementation would serialize all necessary GraphElement data.
            var count = elements.Count();
            if (count > 0)
            {
                return count + " serialized elements";
            }

            return string.Empty;
        }

        static bool CanPasteSerializedDataImplementation(string data)
        {
            // Check if the data starts with an int. That's what we need for pasting.
            int count = int.Parse(data.Split(' ')[0]);
            return count > 0;
        }

        void UnserializeAndPasteImplementation(string operationName, string data)
        {
            int count = int.Parse(data.Split(' ')[0]);

            for (int i = 0; i < count; ++i)
            {
                CreateNode("Pasted element " + i);
            }
        }

        void SelectThreeElements()
        {
            List<GraphElement> list = graphView.GraphElements.ToList();
            m_SelectedNodeCount = 3;
            CommandDispatcher.Dispatch(new SelectElementsCommand(SelectElementsCommand.SelectionMode.Add, list[0].Model, list[1].Model, list[2].Model));
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            for (int i = 0; i < k_DefaultNodeCount; ++i)
            {
                CreateNode("Deletable element " + i);
            }

            graphView.SerializeGraphElementsCallback = SerializeGraphElementsImplementation;
            graphView.CanPasteSerializedDataCallback = CanPasteSerializedDataImplementation;
            graphView.UnserializeAndPasteCallback = UnserializeAndPasteImplementation;

            graphView.UseInternalClipboard = true;
            m_SelectedNodeCount = 0;
        }

        [UnityTest]
        public IEnumerator CopyWithoutSelectedElementsLeavesCopyBufferUntouched()
        {
            MarkGraphViewStateDirty();
            yield return null;

            graphView.Clipboard = "Unknown data";
            CommandDispatcher.Dispatch(new ClearSelectionCommand());
            graphView.Focus();
            yield return null;

            bool used = helpers.ValidateCommand("Copy");
            Assert.IsFalse(used);
            yield return null;

            helpers.ExecuteCommand("Copy");
            yield return null;

            Assert.AreEqual("Unknown data", graphView.Clipboard);
        }

        //[Ignore("sometimes the graphView.clipboard is still Unknown data after the Copy execute")]
        [UnityTest]
        public IEnumerator SelectedElementsCanBeCopyPasted()
        {
            graphView.RebuildUI(GraphModel, CommandDispatcher);
            yield return null;

            graphView.Clipboard = "Unknown data";
            SelectThreeElements();
            MouseCaptureController.ReleaseMouse();
            graphView.Focus();
            yield return null;

            bool used = helpers.ValidateCommand("Copy");
            Assert.IsTrue(used);
            yield return null;

            helpers.ExecuteCommand("Copy");
            yield return null;

            Assert.AreNotEqual("Unknown data", graphView.Clipboard);

            used = helpers.ValidateCommand("Paste");
            Assert.IsTrue(used);
            yield return null;

            helpers.ExecuteCommand("Paste");
            graphView.RebuildUI(GraphModel, CommandDispatcher);
            yield return null;

            Assert.AreEqual(k_DefaultNodeCount + m_SelectedNodeCount, graphView.GraphElements.ToList().Count);
        }

        [UnityTest]
        public IEnumerator SelectedElementsCanBeCut()
        {
            MarkGraphViewStateDirty();
            yield return null;

            graphView.Clipboard = "Unknown data";
            SelectThreeElements();
            MouseCaptureController.ReleaseMouse();
            graphView.Focus();
            yield return null;

            bool used = helpers.ValidateCommand("Cut");
            Assert.IsTrue(used);
            yield return null;

            helpers.ExecuteCommand("Cut");
            yield return null;

            Assert.AreNotEqual("Unknown data", graphView.Clipboard);
            Assert.AreEqual(k_DefaultNodeCount - m_SelectedNodeCount, graphView.GraphElements.ToList().Count);
        }

        [UnityTest]
        public IEnumerator SelectedElementsCanBeDuplicated()
        {
            graphView.RebuildUI(GraphModel, CommandDispatcher);
            yield return null;

            graphView.Clipboard = "Unknown data";
            SelectThreeElements();
            MouseCaptureController.ReleaseMouse();
            graphView.Focus();
            yield return null;

            bool used = helpers.ValidateCommand("Duplicate");
            Assert.IsTrue(used);
            yield return null;

            helpers.ExecuteCommand("Duplicate");
            graphView.RebuildUI(GraphModel, CommandDispatcher);
            yield return null;

            // Duplicate does not change the copy buffer.
            Assert.AreEqual("Unknown data", graphView.Clipboard);
            Assert.AreEqual(k_DefaultNodeCount + m_SelectedNodeCount, graphView.GraphElements.ToList().Count);
        }

        [UnityTest]
        public IEnumerator SelectedElementsCanBeDeleted()
        {
            MarkGraphViewStateDirty();
            yield return null;

            SelectThreeElements();
            MouseCaptureController.ReleaseMouse();
            graphView.Focus();

            bool used = helpers.ValidateCommand("Delete");
            Assert.IsTrue(used);
            yield return null;

            helpers.ExecuteCommand("Delete");
            yield return null;

            List<GraphElement> list = graphView.GraphElements.ToList();
            Assert.AreEqual(k_DefaultNodeCount - m_SelectedNodeCount, list.Count);
        }
    }
}
