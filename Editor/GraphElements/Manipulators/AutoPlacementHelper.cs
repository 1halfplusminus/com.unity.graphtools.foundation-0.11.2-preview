using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    abstract class AutoPlacementHelper
    {
        protected GraphView m_GraphView;

        HashSet<IGraphElementModel> m_SelectedElementModels = new HashSet<IGraphElementModel>();
        HashSet<IGraphElementModel> m_LeftOverElementModels;

        internal Dictionary<INodeModel, HashSet<INodeModel>> NodeDependencies { get; } = new Dictionary<INodeModel, HashSet<INodeModel>>(); // All parent nodes and their children nodes
        Dictionary<INodeModel, INodeModel> m_DependenciesToMerge = new Dictionary<INodeModel, INodeModel>();  // All parent nodes that will merge into another parent node

        protected void SendPlacementCommand(List<IGraphElementModel> updatedModels, List<Vector2> updatedDeltas)
        {
            var models = updatedModels.OfType<IMovable>();
            m_GraphView.CommandDispatcher.Dispatch(new AutoPlaceElementsCommand(updatedDeltas, models.ToList()));
        }

        protected Dictionary<IGraphElementModel, Vector2> GetElementDeltaResults()
        {
            GetSelectedElementModels();

            // Elements will be moved by a delta depending on their bounding rect
            List<Tuple<Rect, List<IGraphElementModel>>> boundingRects = GetBoundingRects();

            return GetDeltas(boundingRects);
        }

        protected abstract void UpdateReferencePosition(ref float referencePosition, Rect currentElementRect);

        protected abstract Vector2 GetDelta(Rect elementPosition, float referencePosition);

        protected abstract float GetStartingPosition(List<Tuple<Rect, List<IGraphElementModel>>> boundingRects);

        void GetSelectedElementModels()
        {
            m_SelectedElementModels.Clear();
            m_SelectedElementModels.AddRangeInternal(m_GraphView.GetSelection().Where(element => !(element is IEdgeModel)));
            m_LeftOverElementModels = new HashSet<IGraphElementModel>(m_SelectedElementModels);
        }

        List<Tuple<Rect, List<IGraphElementModel>>> GetBoundingRects()
        {
            List<Tuple<Rect, List<IGraphElementModel>>> boundingRects = new List<Tuple<Rect, List<IGraphElementModel>>>();

            GetConnectedNodesBoundingRects(ref boundingRects);
            GetPlacematsBoundingRects(ref boundingRects);
            GetLeftOversBoundingRects(ref boundingRects);

            return boundingRects;
        }

        void GetConnectedNodesBoundingRects(ref List<Tuple<Rect, List<IGraphElementModel>>> boundingRects)
        {
            ComputeNodeDependencies();
            boundingRects.AddRange(NodeDependencies.Keys.Select(parent =>
                new Tuple<Rect, List<IGraphElementModel>>(GetConnectedNodesBoundingRect(NodeDependencies, parent), new List<IGraphElementModel>(NodeDependencies[parent]))));
        }

        void GetPlacematsBoundingRects(ref List<Tuple<Rect, List<IGraphElementModel>>> boundingRects)
        {
            List<IPlacematModel> selectedPlacemats = m_SelectedElementModels.OfType<IPlacematModel>().ToList();
            foreach (var placemat in selectedPlacemats.Where(placemat => m_LeftOverElementModels.Contains(placemat)))
            {
                Placemat placematUI = placemat.GetUI<Placemat>(m_GraphView);
                if (placematUI != null)
                {
                    var boundingRect = GetPlacematBoundingRect(ref boundingRects, placematUI, selectedPlacemats);
                    boundingRects.Add(new Tuple<Rect, List<IGraphElementModel>>(boundingRect.Key, boundingRect.Value));
                }
            }
        }

        void GetLeftOversBoundingRects(ref List<Tuple<Rect, List<IGraphElementModel>>> boundingRects)
        {
            foreach (IGraphElementModel element in m_LeftOverElementModels)
            {
                GraphElement elementUI = element.GetUI<GraphElement>(m_GraphView);
                if (elementUI != null)
                {
                    boundingRects.Add(new Tuple<Rect, List<IGraphElementModel>>(elementUI.GetPosition(), new List<IGraphElementModel> { element }));
                }
            }
        }

        Rect GetConnectedNodesBoundingRect(Dictionary<INodeModel, HashSet<INodeModel>> parentsWithChildren, INodeModel parent)
        {
            Rect boundingRect = Rect.zero;
            foreach (Node childUI in parentsWithChildren[parent].Select(child => child.GetUI<Node>(m_GraphView)).Where(childUI => childUI != null))
            {
                if (boundingRect == Rect.zero)
                {
                    boundingRect = childUI.GetPosition();
                }
                AdjustBoundingRect(ref boundingRect, childUI.GetPosition());
            }

            return boundingRect;
        }

        void ComputeNodeDependencies()
        {
            NodeDependencies.Clear();
            m_DependenciesToMerge.Clear();

            foreach (IEdgeModel edgeModel in GetSortedSelectedEdgeModels())
            {
                INodeModel parentModel = edgeModel.ToPort.NodeModel;
                INodeModel childModel = edgeModel.FromPort.NodeModel;

                if (parentModel == childModel)
                {
                    // Node is its own parent
                    continue;
                }

                MergeNodeDependencies(parentModel, childModel);

                AddChildToParent(parentModel, childModel);
            }

            // Remove parents that were merged
            foreach (INodeModel parentToRemove in m_DependenciesToMerge.Keys)
            {
                NodeDependencies.Remove(parentToRemove);
            }
        }

        IEnumerable<IEdgeModel> GetSortedSelectedEdgeModels()
        {
            return m_GraphView.Edges.ToList().Where(edge => m_SelectedElementModels.Contains(edge.Input.NodeModel) && m_SelectedElementModels.Contains(edge.Output.NodeModel))
                .Select(edge => edge.EdgeModel).OrderBy(edge => edge.ToPort.NodeModel.Position.x);
        }

        void MergeNodeDependencies(INodeModel currentParent, INodeModel currentChild)
        {
            // Current parent and current child have a child in common
            MergeSameOtherChild(currentParent, currentChild);

            // Current parent and any of its children have the current child in common
            MergeSameCurrentChild(currentParent, currentChild);

            // Current parent and current child have a parent in common
            MergeSameOtherParent(currentParent, currentChild);
        }

        void MergeSameOtherChild(INodeModel currentParent, INodeModel currentChild)
        {
            // Visual example
            // +---------------+                              +----------------+
            // | other child X o--+-----------------------+---o current parent |
            // +---------------+  |   +---------------+   |   +----------------+
            //                    +---o current child o---+
            //                    |   +---------------+
            // +---------------+  |
            // |               o--+
            // +---------------+

            if (NodeDependencies.TryGetValue(currentChild, out HashSet<INodeModel> otherChildren))
            {
                if (otherChildren.ToList().Any(otherChild => NodeDependencies.ContainsKey(currentParent) && NodeDependencies[currentParent].Contains(otherChild)))
                {
                    MergeDependencies(currentParent, currentChild);
                }
            }
        }

        void MergeSameCurrentChild(INodeModel currentParent, INodeModel currentChild)
        {
            // Visual example
            // +---------------+                              +----------------+
            // | current child o--+-----------------------+---o current parent |
            // +---------------+  |   +---------------+   |   +----------------+
            //                    +---o other child X o---+
            //                    |   +---------------+
            // +---------------+  |
            // |               o--+
            // +---------------+

            if (NodeDependencies.ContainsKey(currentParent))
            {
                foreach (INodeModel otherChild in NodeDependencies[currentParent].ToList())
                {
                    if (NodeDependencies.TryGetValue(otherChild, out HashSet<INodeModel> otherChildren) && otherChildren.Contains(currentChild))
                    {
                        // current parent and one of its children (X) are both parent to current child, merge current parent with X : X's children will all become current parent's children
                        MergeDependencies(currentParent, otherChild);
                    }
                }
            }
        }

        void MergeSameOtherParent(INodeModel currentParent, INodeModel currentChild)
        {
            // Visual example
            // +---------------+                              +----------------+
            // | current child o--+-----------------------+---o other parent X |
            // +---------------+  |   +---------------+   |   +----------------+
            //                    +---o currentparent o---+
            //                    |   +---------------+
            // +---------------+  |
            // |               o--+
            // +---------------+

            foreach (INodeModel otherParent in NodeDependencies.Keys.ToList().Where(otherParent =>
                NodeDependencies[otherParent].Contains(currentChild) && NodeDependencies[otherParent].Contains(currentParent)))
            {
                // current child and current parent both have the same parent (X), merge current parent with X : current parent's children will all become X's children
                MergeDependencies(otherParent, currentParent);
            }
        }

        void MergeDependencies(INodeModel parentToMergeInto, INodeModel parentToRemove)
        {
            if (NodeDependencies.ContainsKey(parentToMergeInto) && NodeDependencies.ContainsKey(parentToRemove))
            {
                NodeDependencies[parentToMergeInto].AddRangeInternal(NodeDependencies[parentToRemove]);
                m_DependenciesToMerge[parentToRemove] = parentToMergeInto;
            }
        }

        void AddChildToParent(INodeModel currentParent, INodeModel currentChild)
        {
            if (!NodeDependencies.ContainsKey(currentParent))
            {
                NodeDependencies.Add(currentParent, new HashSet<INodeModel>());
            }

            NodeDependencies[GetParent(currentParent)].Add(currentChild);
            m_LeftOverElementModels.Remove(currentChild);
        }

        INodeModel GetParent(INodeModel currentParent)
        {
            INodeModel newParent = currentParent;

            while (m_DependenciesToMerge.ContainsKey(newParent))
            {
                // Get the parent that the current dependency will be merged into
                newParent = m_DependenciesToMerge[currentParent];
            }

            return newParent;
        }

        KeyValuePair<Rect, List<IGraphElementModel>> GetPlacematBoundingRect(ref List<Tuple<Rect, List<IGraphElementModel>>> boundingRects, Placemat placematUI, List<IPlacematModel> selectedPlacemats)
        {
            Rect boundingRect = placematUI.GetPosition();
            List<IGraphElementModel> elementsOnBoundingRect = new List<IGraphElementModel>();
            List<Placemat> placematsOnBoundingRect = GetPlacematsOnBoundingRect(ref boundingRect, ref elementsOnBoundingRect, selectedPlacemats);

            // Adjust the bounding rect with elements overlapping any of the placemats on the bounding rect
            AdjustPlacematBoundingRect(ref boundingRect, ref elementsOnBoundingRect, placematsOnBoundingRect);

            foreach (var otherRect in boundingRects.ToList())
            {
                Rect otherBoundingRect = otherRect.Item1;
                List<IGraphElementModel> otherBoundingRectElements = otherRect.Item2;
                if (otherBoundingRectElements.Any(element => IsOnPlacemats(element.GetUI<GraphElement>(m_GraphView), placematsOnBoundingRect)))
                {
                    AdjustBoundingRect(ref boundingRect, otherBoundingRect);
                    elementsOnBoundingRect.AddRange(otherBoundingRectElements);
                    boundingRects.Remove(otherRect);
                }
            }

            return new KeyValuePair<Rect, List<IGraphElementModel>>(boundingRect, elementsOnBoundingRect);
        }

        protected virtual List<Tuple<Rect, List<IGraphElementModel>>> GetBoundingRectsList(List<Tuple<Rect, List<IGraphElementModel>>> boundingRects)
        {
            return boundingRects;
        }

        Dictionary<IGraphElementModel, Vector2> GetDeltas(List<Tuple<Rect, List<IGraphElementModel>>> boundingRects)
        {
            List<Tuple<Rect, List<IGraphElementModel>>> boundingRectsList = GetBoundingRectsList(boundingRects);

            float referencePosition = GetStartingPosition(boundingRectsList);

            Dictionary<IGraphElementModel, Vector2> deltas = new Dictionary<IGraphElementModel, Vector2>();

            foreach (var (boundingRect, elements) in boundingRectsList)
            {
                Vector2 delta = GetDelta(boundingRect, referencePosition);
                foreach (var element in elements.Where(element => !deltas.ContainsKey(element)))
                {
                    deltas[element] = delta;
                }
                UpdateReferencePosition(ref referencePosition, boundingRect);
            }

            return deltas;
        }

        List<Placemat> GetPlacematsOnBoundingRect(ref Rect boundingRect, ref List<IGraphElementModel> elementsOnBoundingRect, List<IPlacematModel> selectedPlacemats)
        {
            List<Placemat> placematsOnBoundingRect = new List<Placemat>();

            foreach (IPlacematModel placemat in selectedPlacemats.Where(placemat => m_LeftOverElementModels.Contains(placemat)))
            {
                Placemat placematUI = placemat.GetUI<Placemat>(m_GraphView);
                if (placematUI != null && placematUI.layout.Overlaps(boundingRect))
                {
                    AdjustBoundingRect(ref boundingRect, placematUI.GetPosition());

                    placematsOnBoundingRect.Add(placematUI);
                    elementsOnBoundingRect.Add(placemat);
                    m_LeftOverElementModels.Remove(placemat);
                }
            }

            return placematsOnBoundingRect;
        }

        void AdjustPlacematBoundingRect(ref Rect boundingRect, ref List<IGraphElementModel> elementsOnBoundingRect, List<Placemat> placematsOnBoundingRect)
        {
            foreach (GraphElement elementUI in m_GraphView.GraphElements.ToList().Where(element => !(element is Placemat)))
            {
                if (elementUI != null && IsOnPlacemats(elementUI, placematsOnBoundingRect))
                {
                    AdjustBoundingRect(ref boundingRect, elementUI.GetPosition());
                    elementsOnBoundingRect.Add(elementUI.Model);
                    m_LeftOverElementModels.Remove(elementUI.Model);
                }
            }
        }

        static void AdjustBoundingRect(ref Rect boundingRect, Rect otherRect)
        {
            if (otherRect.yMin < boundingRect.yMin)
            {
                boundingRect.yMin = otherRect.yMin;
            }
            if (otherRect.xMin < boundingRect.xMin)
            {
                boundingRect.xMin = otherRect.xMin;
            }
            if (otherRect.yMax > boundingRect.yMax)
            {
                boundingRect.yMax = otherRect.yMax;
            }
            if (otherRect.xMax > boundingRect.xMax)
            {
                boundingRect.xMax = otherRect.xMax;
            }
        }

        static bool IsOnPlacemats(GraphElement element, List<Placemat> placemats)
        {
            return placemats.Any(placemat => !element.Equals(placemat) && element.layout.Overlaps(placemat.layout));
        }
    }
}
