using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using Object = UnityEngine.Object;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// A component to hold the editor state of the <see cref="GraphView"/> for a graph asset.
    /// </summary>
    [Serializable]
    public class GraphViewStateComponent : AssetStateComponent<GraphViewStateComponent.StateUpdater>
    {
        /// <summary>
        /// Updater for the <see cref="GraphViewStateComponent"/>.
        /// </summary>
        public class StateUpdater : BaseUpdater<GraphViewStateComponent>
        {
            IGraphElementModel[] m_Single = new IGraphElementModel[1];

            /// <summary>
            /// Loads a graph asset in the graph view.
            /// </summary>
            /// <param name="assetModel">The graph asset to load.</param>
            public void LoadGraphAsset(IGraphAssetModel assetModel)
            {
                m_State.m_CurrentGraph = new OpenedGraph(assetModel, null);

                m_State.SetUpdateType(UpdateType.Complete);
            }

            /// <summary>
            /// The scale factor of the <see cref="GraphView"/>.
            /// </summary>
            public Vector3 Scale
            {
                set
                {
                    if (m_State.m_Scale != value)
                    {
                        m_State.m_Scale = value;
                        m_State.SetUpdateType(UpdateType.Partial);
                    }
                }
            }

            /// <summary>
            /// The position of the <see cref="GraphView"/>.
            /// </summary>
            public Vector3 Position
            {
                set
                {
                    if (m_State.m_Position != value)
                    {
                        m_State.m_Position = value;
                        m_State.SetUpdateType(UpdateType.Partial);
                    }
                }
            }

            /// <summary>
            /// Marks graph element models as newly created.
            /// </summary>
            /// <param name="models">The newly created models.</param>
            public void MarkNew(IEnumerable<IGraphElementModel> models)
            {
                bool somethingChanged = false;

                foreach (var model in models ?? Enumerable.Empty<IGraphElementModel>())
                {
                    if (model == null || m_State.CurrentChangeset.DeletedModels.Contains(model))
                        continue;

                    m_State.CurrentChangeset.ChangedModels.Remove(model);
                    m_State.CurrentChangeset.NewModels.Add(model);

                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    m_State.SetUpdateType(UpdateType.Partial);

                    var assetModel = m_State.AssetModel as Object;
                    if (assetModel)
                    {
                        m_State.AssetModel.Dirty = true;
                        EditorUtility.SetDirty(assetModel);
                    }
                }
            }

            /// <summary>
            /// Marks a graph element model as newly created.
            /// </summary>
            /// <param name="model">The newly created model.</param>
            public void MarkNew(IGraphElementModel model)
            {
                m_Single[0] = model;
                MarkNew(m_Single);
            }

            /// <summary>
            /// Marks graph element models as changed.
            /// </summary>
            /// <param name="models">The changed models.</param>
            public void MarkChanged(IEnumerable<IGraphElementModel> models)
            {
                bool somethingChanged = false;

                foreach (var model in models ?? Enumerable.Empty<IGraphElementModel>())
                {
                    if (model == null ||
                        m_State.CurrentChangeset.NewModels.Contains(model) ||
                        m_State.CurrentChangeset.DeletedModels.Contains(model))
                        continue;

                    m_State.CurrentChangeset.ChangedModels.Add(model);

                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    m_State.SetUpdateType(UpdateType.Partial);

                    var assetModel = m_State.AssetModel as Object;
                    if (assetModel)
                    {
                        m_State.AssetModel.Dirty = true;
                        EditorUtility.SetDirty(assetModel);
                    }
                }
            }

            /// <summary>
            /// Marks a graph element model as changed.
            /// </summary>
            /// <param name="model">The changed model.</param>
            public void MarkChanged(IGraphElementModel model)
            {
                m_Single[0] = model;
                MarkChanged(m_Single);
            }

            /// <summary>
            /// Marks graph element models as deleted.
            /// </summary>
            /// <param name="models">The deleted models.</param>
            public void MarkDeleted(IEnumerable<IGraphElementModel> models)
            {
                bool somethingChanged = false;

                foreach (var model in models ?? Enumerable.Empty<IGraphElementModel>())
                {
                    if (model == null)
                        continue;

                    m_State.CurrentChangeset.NewModels.Remove(model);
                    m_State.CurrentChangeset.ChangedModels.Remove(model);

                    m_State.CurrentChangeset.DeletedModels.Add(model);

                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    m_State.SetUpdateType(UpdateType.Partial);

                    var assetModel = m_State.AssetModel as Object;
                    if (assetModel)
                    {
                        m_State.AssetModel.Dirty = true;
                        EditorUtility.SetDirty(assetModel);
                    }
                }
            }

            /// <summary>
            /// Marks a graph element model as deleted.
            /// </summary>
            /// <param name="model">The deleted model.</param>
            public void MarkDeleted(IGraphElementModel model)
            {
                m_Single[0] = model;
                MarkDeleted(m_Single);
            }

            /// <summary>
            /// Marks a model as needing to be aligned.
            /// </summary>
            /// <param name="model">The model to align.</param>
            public void MarkModelToAutoAlign(IGraphElementModel model)
            {
                m_State.CurrentChangeset.ModelsToAutoAlign.Add(model);
            }

            /// <summary>
            /// Tells the state component that the graph asset was modified externally.
            /// </summary>
            public void AssetChangedOnDisk()
            {
                m_State.SetUpdateType(UpdateType.Complete);
            }
        }

        /// <summary>
        /// The class that describes what changed in the <see cref="GraphViewStateComponent"/>.
        /// </summary>
        public class Changeset : IChangeset
        {
            /// <summary>
            /// The new models.
            /// </summary>
            public HashSet<IGraphElementModel> NewModels { get; }

            /// <summary>
            /// The changed models.
            /// </summary>
            public HashSet<IGraphElementModel> ChangedModels { get; }

            /// <summary>
            /// The deleted models.
            /// </summary>
            public HashSet<IGraphElementModel> DeletedModels { get; }

            /// <summary>
            /// The models that need to be aligned.
            /// </summary>
            public HashSet<IGraphElementModel> ModelsToAutoAlign { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Changeset" /> class.
            /// </summary>
            public Changeset()
            {
                NewModels = new HashSet<IGraphElementModel>();
                ChangedModels = new HashSet<IGraphElementModel>();
                DeletedModels = new HashSet<IGraphElementModel>();
                ModelsToAutoAlign = new HashSet<IGraphElementModel>();
            }

            /// <inheritdoc/>
            public void Clear()
            {
                NewModels.Clear();
                ChangedModels.Clear();
                DeletedModels.Clear();
                ModelsToAutoAlign.Clear();
            }

            /// <inheritdoc/>
            public void AggregateFrom(IEnumerable<IChangeset> changesets)
            {
                Clear();

                foreach (var changeset in changesets.OfType<Changeset>())
                {
                    NewModels.AddRangeInternal(changeset.NewModels);
                    ChangedModels.AddRangeInternal(changeset.ChangedModels);
                    DeletedModels.AddRangeInternal(changeset.DeletedModels);
                    ModelsToAutoAlign.AddRangeInternal(changeset.ModelsToAutoAlign);
                }

                NewModels.RemoveWhere(m => DeletedModels.Contains(m));

                ChangedModels.RemoveWhere(m => NewModels.Contains(m));
                ChangedModels.RemoveWhere(m => DeletedModels.Contains(m));

                ModelsToAutoAlign.RemoveWhere(m => DeletedModels.Contains(m));
            }
        }

        ChangesetManager<Changeset> m_ChangesetManager = new ChangesetManager<Changeset>();
        Changeset CurrentChangeset => m_ChangesetManager.CurrentChangeset;

        [SerializeField]
        OpenedGraph m_CurrentGraph;

        [SerializeField]
        Vector3 m_Scale = Vector3.one;

        [SerializeField]
        Vector3 m_Position = Vector3.zero;

        /// <summary>
        /// The scale (zoom factor) of the graph view.
        /// </summary>
        public Vector3 Scale => m_Scale;

        /// <summary>
        /// The position of the graph view.
        /// </summary>
        public Vector3 Position => m_Position;

        /// <summary>
        /// The graph asset model to display.
        /// </summary>
        public IGraphAssetModel AssetModel => m_CurrentGraph.GetGraphAssetModel();

        /// <summary>
        /// The graph asset GUID.
        /// </summary>
        public string AssetModelGUID => m_CurrentGraph.GraphModelAssetGUID;

        /// <summary>
        /// The <see cref="IGraphModel"/> contained in <see cref="AssetModel"/>.
        /// <remarks>This method is virtual for tests.</remarks>
        /// </summary>
        public virtual IGraphModel GraphModel => AssetModel?.GraphModel;

        /// <inheritdoc/>
        protected override void PushChangeset(uint version)
        {
            base.PushChangeset(version);

            // If update type is Complete, there is no need to push the changeset, as they cannot be used for an update.
            if (UpdateType != UpdateType.Complete)
                m_ChangesetManager.PushChangeset(version);
        }

        /// <inheritdoc/>
        public override void PurgeOldChangesets(uint untilVersion)
        {
            EarliestChangeSetVersion = m_ChangesetManager.PurgeOldChangesets(untilVersion, CurrentVersion);
            ResetUpdateType();
        }

        /// <inheritdoc />
        public override void SetUpdateType(UpdateType type, bool force = false)
        {
            base.SetUpdateType(type, force);

            // If update type is Complete, there is no need to keep the changesets, as they cannot be used for an update.
            if (UpdateType == UpdateType.Complete)
            {
                m_ChangesetManager.PurgeOldChangesets(CurrentVersion, CurrentVersion);
            }
        }

        /// <inheritdoc  cref="ChangesetManager{TChangeset}"/>
        public Changeset GetAggregatedChangeset(uint sinceVersion)
        {
            return m_ChangesetManager.GetAggregatedChangeset(sinceVersion, CurrentVersion);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
        }

        /// <inheritdoc />
        public override void ValidateAfterDeserialize()
        {
            base.ValidateAfterDeserialize();

            if (!m_CurrentGraph.IsValid())
                m_CurrentGraph = new OpenedGraph(null, null);
        }
    }
}
