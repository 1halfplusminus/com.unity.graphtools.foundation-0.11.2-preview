using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.GraphToolsFoundation.Overdrive.Bridge;
using UnityEditor.GraphToolsFoundation.Overdrive.Tests.TestModels;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GraphElements
{
    class WindowStateComponent : Overdrive.WindowStateComponent
    {
        internal IGraphModel m_GraphModel;

        public override IGraphModel GraphModel => m_GraphModel;
    }

    class GraphViewStateComponent : Overdrive.GraphViewStateComponent
    {
        internal IGraphModel m_GraphModel;

        public override IGraphModel GraphModel => m_GraphModel;
    }

    class GraphToolState : Overdrive.GraphToolState
    {
        static Preferences CreatePreferences()
        {
            var prefs = Preferences.CreatePreferences("GraphToolsFoundationTests.");
            prefs.SetBoolNoEditorUpdate(BoolPref.ErrorOnRecursiveDispatch, false);
            return prefs;
        }

        IGraphModel m_GraphModel;

        private protected override Overdrive.WindowStateComponent CreateWindowStateComponent(Hash128 guid)
        {
            var state = PersistedState.GetOrCreateViewStateComponent<WindowStateComponent>(guid, nameof(WindowState));
            state.m_GraphModel = m_GraphModel;
            return state;
        }

        private protected override Overdrive.GraphViewStateComponent CreateGraphViewStateComponent()
        {
            var state = PersistedState.GetOrCreateAssetStateComponent<GraphViewStateComponent>(nameof(GraphViewState));
            state.m_GraphModel = m_GraphModel;
            return state;
        }

        public GraphToolState(SerializableGUID graphViewEditorWindowGUID, IGraphModel graphModel)
            : base(graphViewEditorWindowGUID, CreatePreferences())
        {
            m_GraphModel = graphModel;
        }

        ~GraphToolState() => Dispose(false);
    }

    class GraphViewTester
    {
        static readonly Rect k_WindowRect = new Rect(Vector2.zero, new Vector2(SelectionDragger.panAreaWidth * 8, SelectionDragger.panAreaWidth * 6));

        bool m_SnapToPortEnabled;
        bool m_SnapToBorderEnabled;
        bool m_SnapToGridEnabled;
        bool m_SnapToSpacingEnabled;
        float m_SpacingMarginValue;

        protected TestGraphViewWindow window { get; private set; }
        protected TestGraphView graphView { get; private set; }
        protected TestEventHelpers helpers { get; private set; }
        protected IGraphModel GraphModel => window.GraphModel;
        protected CommandDispatcher CommandDispatcher => window.CommandDispatcher;

        bool m_EnablePersistence;

        public GraphViewTester(bool enablePersistence = false)
        {
            m_EnablePersistence = enablePersistence;
        }

        bool m_SavedUseNewStylesheets;
        [SetUp]
        public virtual void SetUp()
        {
            m_SnapToPortEnabled = GraphViewSettings.UserSettings.EnableSnapToPort;
            m_SnapToBorderEnabled = GraphViewSettings.UserSettings.EnableSnapToBorders;
            m_SnapToGridEnabled = GraphViewSettings.UserSettings.EnableSnapToGrid;
            m_SnapToSpacingEnabled = GraphViewSettings.UserSettings.EnableSnapToSpacing;
            m_SpacingMarginValue = GraphViewSettings.UserSettings.SpacingMarginValue;

            GraphViewSettings.UserSettings.EnableSnapToPort = false;
            GraphViewSettings.UserSettings.EnableSnapToBorders = false;
            GraphViewSettings.UserSettings.EnableSnapToGrid = false;
            GraphViewSettings.UserSettings.EnableSnapToSpacing = false;

            m_SavedUseNewStylesheets = GraphElementHelper.UseNewStylesheets;
            GraphElementHelper.UseNewStylesheets = true;

            window = EditorWindow.GetWindowWithRect<TestGraphViewWindow>(k_WindowRect);

            if (!m_EnablePersistence)
                window.DisableViewDataPersistence();
            else
                window.ClearPersistentViewData();

            graphView = window.GraphView as TestGraphView;
            graphView.AddTestStylesheet("Tests.uss");

            helpers = new TestEventHelpers(window);

            Vector3 frameTranslation = Vector3.zero;
            Vector3 frameScaling = Vector3.one;
            CommandDispatcher.Dispatch(new ReframeGraphViewCommand(frameTranslation, frameScaling));
        }

        [TearDown]
        public virtual void TearDown()
        {
            GraphElementHelper.UseNewStylesheets = m_SavedUseNewStylesheets;
            UIForModel.Reset();

            if (m_EnablePersistence)
                window.ClearPersistentViewData();

            Clear();

            GraphViewSettings.UserSettings.EnableSnapToPort = m_SnapToPortEnabled;
            GraphViewSettings.UserSettings.EnableSnapToBorders = m_SnapToBorderEnabled;
            GraphViewSettings.UserSettings.EnableSnapToGrid = m_SnapToGridEnabled;
            GraphViewSettings.UserSettings.EnableSnapToSpacing = m_SnapToSpacingEnabled;
            GraphViewSettings.UserSettings.SpacingMarginValue = m_SpacingMarginValue;
        }

        void Clear()
        {
            // See case: https://fogbugz.unity3d.com/f/cases/998343/
            // Clearing the capture needs to happen before closing the window
            MouseCaptureController.ReleaseMouse();
            if (window != null)
            {
                window.Close();
            }
        }

        protected void MarkGraphViewStateDirty()
        {
            using (var updater = CommandDispatcher.State.GraphViewState.UpdateScope)
            {
                updater.ForceCompleteUpdate();
            }
        }

        protected IONodeModel CreateNode(string title = "", Vector2 position = default, int inCount = 0, int outCount = 0, int exeInCount = 0, int exeOutCount = 0, PortOrientation orientation = PortOrientation.Horizontal)
        {
            return CreateNode<IONodeModel>(title, position, inCount, outCount, exeInCount, exeOutCount, orientation);
        }

        protected ContextNodeModel CreateContext(string title = "", Vector2 position = default)
        {
            return GraphModel.CreateNode<ContextNodeModel>(title, position, initializationCallback: model => { });
        }

        protected TNodeModel CreateNode<TNodeModel>(string title, Vector2 position, int inCount = 0, int outCount = 0, int exeInCount = 0, int exeOutCount = 0, PortOrientation orientation = PortOrientation.Horizontal) where TNodeModel : IONodeModel, new()
        {
            var node = GraphModel.CreateNode<TNodeModel>(title, position, initializationCallback: model =>
            {
                model.InputCount = inCount;
                model.OuputCount = outCount;
                model.ExeInputCount = exeInCount;
                model.ExeOuputCount = exeOutCount;
            });

            foreach (var portModel in node.Ports.Cast<PortModel>())
            {
                portModel.Orientation = orientation;
            }

            return node;
        }

        protected TContextModel CreateContext<TContextModel>(string title, Vector2 position, int inCount = 0, int outCount = 0, int exeInCount = 0, int exeOutCount = 0, PortOrientation orientation = PortOrientation.Horizontal) where TContextModel : ContextNodeModel, new()
        {
            var node = GraphModel.CreateNode<TContextModel>(title, position, initializationCallback: model =>
            {
                model.InputCount = inCount;
                model.OuputCount = outCount;
                model.ExeInputCount = exeInCount;
                model.ExeOuputCount = exeOutCount;
            });

            foreach (var portModel in node.Ports.Cast<PortModel>())
            {
                portModel.Orientation = orientation;
            }

            return node;
        }

        protected IEnumerator ConnectPorts(IPortModel fromPort, IPortModel toPort)
        {
            var originalEdgeCount = GraphModel.EdgeModels.Count;
            var fromPortUI = fromPort.GetUI<Port>(graphView);
            var toPortUI = toPort.GetUI<Port>(graphView);

            Assert.IsNotNull(fromPortUI);
            Assert.IsNotNull(toPortUI);

            // Drag an edge between the two ports
            helpers.DragTo(fromPortUI.GetGlobalCenter(), toPortUI.GetGlobalCenter());
            yield return null;

            Assert.AreEqual(originalEdgeCount + 1, GraphModel.EdgeModels.Count, "Edge has not been created");
        }

        protected IPlacematModel CreatePlacemat(Rect posAndDim, string title = "")
        {
            var pm = GraphModel.CreatePlacemat(posAndDim);
            pm.Title = title;
            return pm;
        }

        protected IStickyNoteModel CreateSticky(string title = "", string contents = "", Rect stickyRect = default)
        {
            var sticky = GraphModel.CreateStickyNote(stickyRect);
            sticky.Contents = contents;
            sticky.Title = title;
            return sticky;
        }

        public static void AssertVector2AreEqualWithinDelta(Vector2 expected, Vector2 actual, float withinDelta, string message = null)
        {
            Assert.AreEqual(expected.x, actual.x, withinDelta, message);
            Assert.AreEqual(expected.y, actual.y, withinDelta, message);
        }
    }
}
