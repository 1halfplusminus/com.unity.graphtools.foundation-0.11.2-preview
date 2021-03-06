using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.UI
{
    class WindowAssetModificationWatcherTests : BaseUIFixture
    {
        IGraphAssetModel m_Asset1;

        /// <inheritdoc />
        protected override bool CreateGraphOnStartup => false;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            m_Asset1 = GraphAssetCreationHelpers<TestGraphAssetModel>.CreateGraphAsset(CreatedGraphType, "Test1", "Assets/test1.asset");
            Window.CommandDispatcher.State.LoadGraphAsset(m_Asset1, null);
        }

        [TearDown]
        public override void TearDown()
        {
            var path = AssetDatabase.GetAssetPath(m_Asset1 as Object);
            if (!string.IsNullOrEmpty(path))
                AssetDatabase.DeleteAsset(path);

            base.TearDown();
        }

        [Test]
        public void TestDeleteAssetClearsCurrentOpenedGraph()
        {
            var state = Window.CommandDispatcher.State;
            state.LoadGraphAsset(m_Asset1, null);

            Assert.IsNotNull(state.WindowState.CurrentGraph.GetGraphAssetModel());

            var path = AssetDatabase.GetAssetPath(m_Asset1 as Object);
            Assert.IsNotNull(path);
            AssetDatabase.DeleteAsset(path);

            Assert.AreEqual("", state.WindowState.CurrentGraph.GraphModelAssetGUID);
        }

        [UnityTest]
        public IEnumerator TestRenameAssetUpdatesCurrentGraphName()
        {
            var state = Window.CommandDispatcher.State;
            state.LoadGraphAsset(m_Asset1, null);

            Assert.IsNotNull(state.WindowState.CurrentGraph.GetGraphAssetModel());

            yield return null;

            var path = AssetDatabase.GetAssetPath(m_Asset1 as Object);
            Assert.IsNotNull(path);
            AssetDatabase.RenameAsset(path, "blah");

            yield return null;

            var firstBreadcrumbButton = Window.rootVisualElement.Q<ToolbarBreadcrumbs>()?.Children().First() as ToolbarButton;
            Assert.IsNotNull(firstBreadcrumbButton);
            Assert.AreEqual("blah", firstBreadcrumbButton.text);
        }
    }
}
