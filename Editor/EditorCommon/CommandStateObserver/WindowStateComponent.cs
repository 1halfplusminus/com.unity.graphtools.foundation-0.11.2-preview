using System;
using System.Collections.Generic;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// The state component of the <see cref="GraphViewEditorWindow"/>.
    /// </summary>
    [Serializable]
    public class WindowStateComponent : ViewStateComponent<WindowStateComponent.StateUpdater>
    {
        /// <summary>
        /// The updater for the <see cref="WindowStateComponent"/>.
        /// </summary>
        public class StateUpdater : BaseUpdater<WindowStateComponent>
        {
            /// <summary>
            /// Loads a graph asset in the window.
            /// </summary>
            /// <param name="assetModel">The graph asset to load.</param>
            /// <param name="boundObject">The GameObject to which the asset is bound, if any.</param>
            public void LoadGraphAsset(IGraphAssetModel assetModel, GameObject boundObject)
            {
                if (!string.IsNullOrEmpty(m_State.m_CurrentGraph.GetGraphAssetModelPath()))
                    m_State.m_LastOpenedGraph = m_State.m_CurrentGraph;
                m_State.m_CurrentGraph = new OpenedGraph(assetModel, boundObject);
                m_State.m_BlackboardGraphModel = null;
                m_State.SetUpdateType(UpdateType.Complete);
            }

            /// <summary>
            /// Pushes the currently opened graph onto the graph history stack.
            /// </summary>
            public void PushCurrentGraph()
            {
                m_State.m_SubGraphStack.Add(m_State.m_CurrentGraph);
                m_State.SetUpdateType(UpdateType.Complete);
            }

            /// <summary>
            /// Removes the most recent <paramref name="length"/> elements from the graph history stack..
            /// </summary>
            /// <param name="length">The number of elements to remove.</param>
            public void TruncateHistory(int length)
            {
                m_State.m_SubGraphStack.RemoveRange(length, m_State.m_SubGraphStack.Count - length);
                m_State.SetUpdateType(UpdateType.Complete);
            }

            /// <summary>
            /// Empties the graph history stack.
            /// </summary>
            public void ClearHistory()
            {
                m_State.m_SubGraphStack.Clear();
                m_State.SetUpdateType(UpdateType.Complete);
            }

            /// <summary>
            /// Tells the state component that the graph asset was modified externally.
            /// </summary>
            public void AssetChangedOnDisk()
            {
                m_State.SetUpdateType(UpdateType.Complete);
            }
        }

        [SerializeField]
        OpenedGraph m_CurrentGraph;

        [SerializeField]
        OpenedGraph m_LastOpenedGraph;

        [SerializeField]
        List<OpenedGraph> m_SubGraphStack;

        IBlackboardGraphModel m_BlackboardGraphModel;

        /// <summary>
        /// The currently opened <see cref="IGraphAssetModel"/>.
        /// </summary>
        public IGraphAssetModel AssetModel => CurrentGraph.GetGraphAssetModel();

        /// <summary>
        /// The <see cref="IGraphModel"/> contained in <see cref="AssetModel"/>.
        /// <remarks>This method is virtual for tests.</remarks>
        /// </summary>
        public virtual IGraphModel GraphModel => CurrentGraph.GetGraphAssetModel()?.GraphModel;

        /// <summary>
        /// The <see cref="IBlackboardGraphModel"/> for the <see cref="AssetModel"/>.
        /// </summary>
        public IBlackboardGraphModel BlackboardGraphModel
        {
            get
            {
                // m_BlackboardGraphModel will be null after unserialize (open, undo) and LoadGraphAsset.
                if (m_BlackboardGraphModel == null)
                {
                    var assetModel = m_CurrentGraph.GetGraphAssetModel();
                    m_BlackboardGraphModel = assetModel?.GraphModel?.Stencil?.CreateBlackboardGraphModel(assetModel);
                }
                return m_BlackboardGraphModel;
            }
        }

        /// <summary>
        /// The currently opened graph.
        /// </summary>
        public OpenedGraph CurrentGraph => m_CurrentGraph;

        /// <summary>
        /// The previously opened graph.
        /// </summary>
        public OpenedGraph LastOpenedGraph => m_LastOpenedGraph;

        /// <summary>
        /// A stack containing the history of opened graph.
        /// </summary>
        public IReadOnlyList<OpenedGraph> SubGraphStack => m_SubGraphStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowStateComponent" /> class.
        /// </summary>
        public WindowStateComponent()
        {
            m_SubGraphStack = new List<OpenedGraph>();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public override void ValidateAfterDeserialize()
        {
            base.ValidateAfterDeserialize();

            // Check that all referenced graphs still exist (assets may have been deleted).
            if (!m_CurrentGraph.IsValid())
            {
                m_CurrentGraph = new OpenedGraph(null, null);
            }

            if (!m_LastOpenedGraph.IsValid())
            {
                m_LastOpenedGraph = new OpenedGraph(null, null);
            }

            for (var i = 0; i < m_SubGraphStack.Count; i++)
            {
                if (!m_SubGraphStack[i].IsValid())
                {
                    m_SubGraphStack[i] = new OpenedGraph(null, null);
                }
            }
        }
    }
}
