using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// A part to build the UI for a text bubble on an edge.
    /// </summary>
    public class EdgeBubblePart : BaseModelUIPart
    {
        public static readonly string ussClassName = "ge-edge-bubble-part";

        /// <summary>
        /// Creates a new instance of the <see cref="EdgeBubblePart"/> class.
        /// </summary>
        /// <param name="name">The name of the part.</param>
        /// <param name="model">The model displayed in this part.</param>
        /// <param name="ownerElement">The owner of the part.</param>
        /// <param name="parentClassName">The class name of the parent.</param>
        /// <returns>A new instance of <see cref="EdgeBubblePart"/>.</returns>
        public static EdgeBubblePart Create(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
        {
            if (model is IEdgeModel)
            {
                return new EdgeBubblePart(name, model, ownerElement, parentClassName);
            }

            return null;
        }

        protected EdgeBubble m_EdgeBubble;

        /// <inheritdoc />
        public override VisualElement Root => m_EdgeBubble;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeBubblePart"/> class.
        /// </summary>
        /// <param name="name">The name of the part.</param>
        /// <param name="model">The model displayed in this part.</param>
        /// <param name="ownerElement">The owner of the part.</param>
        /// <param name="parentClassName">The class name of the parent.</param>
        protected EdgeBubblePart(string name, IGraphElementModel model, IModelUI ownerElement, string parentClassName)
            : base(name, model, ownerElement, parentClassName) { }

        /// <inheritdoc />
        protected override void BuildPartUI(VisualElement container)
        {
            m_EdgeBubble = new EdgeBubble { name = PartName };
            m_EdgeBubble.AddToClassList(ussClassName);
            m_EdgeBubble.AddToClassList(m_ParentClassName.WithUssElement(PartName));
            container.Add(m_EdgeBubble);
        }

        /// <inheritdoc />
        protected override void PostBuildPartUI()
        {
            base.PostBuildPartUI();
            m_EdgeBubble.AddStylesheet("EdgeBubblePart.uss");
        }

        /// <inheritdoc />
        protected override void UpdatePartFromModel()
        {
            if (!(m_Model is IEdgeModel edgeModel) || !(m_OwnerElement is VisualElement edge))
                return;

            if (ShouldShow())
            {
                VisualElement attachPoint = edge.SafeQ<EdgeControl>() ?? edge;
                var offset = Vector2.zero;
                if (attachPoint is EdgeControl)
                {
                    offset = ComputePosition() - new Vector2(attachPoint.layout.xMin + attachPoint.layout.width / 2, attachPoint.layout.yMin + attachPoint.layout.height / 2);
                    if (float.IsNaN(offset.x))
                        offset.x = 0;
                    if (float.IsNaN(offset.y))
                        offset.y = 0;
                }

                m_EdgeBubble.SetAttacherOffset(offset);
                m_EdgeBubble.text = edgeModel.EdgeLabel;
                m_EdgeBubble.AttachTo(attachPoint, SpriteAlignment.Center);
                m_EdgeBubble.style.visibility = StyleKeyword.Null;
            }
            else
            {
                m_EdgeBubble.Detach();
                m_EdgeBubble.style.visibility = Visibility.Hidden;
            }
        }

        protected virtual bool ShouldShow()
        {
            var edgeModel = m_Model as IEdgeModel;
            var toPortNodeModel = edgeModel?.ToPort?.NodeModel;
            var fromPortNodeModel = edgeModel?.FromPort?.NodeModel;
            var portType = edgeModel?.FromPort?.PortType ?? PortType.Data;

            return portType == PortType.Execution && (fromPortNodeModel != null || toPortNodeModel != null) &&
                !string.IsNullOrEmpty(edgeModel.EdgeLabel);
        }

        protected Vector2 ComputePosition()
        {
            var edge = m_OwnerElement as Edge;
            var edgeControl = edge?.SafeQ<EdgeControl>();

            if (edgeControl == null)
                return Vector2.zero;

            if (edgeControl.RenderPoints.Count > 0)
            {
                const int intersectionSquaredRadius = 10000;

                // Find the segment that intersect a circle of radius sqrt(targetSqDistance) centered at `from`.
                float targetSqDistance = Mathf.Min(intersectionSquaredRadius, (edge.To - edge.From).sqrMagnitude / 4);
                var localFrom = edge.ChangeCoordinatesTo(edgeControl, edge.From);
                for (var index = 0; index < edgeControl.RenderPoints.Count; index++)
                {
                    var point = edgeControl.RenderPoints[index];
                    if ((point - localFrom).sqrMagnitude >= targetSqDistance)
                    {
                        return edgeControl.ChangeCoordinatesTo(edge, edgeControl.RenderPoints[index]);
                    }
                }
            }

            return edgeControl.ChangeCoordinatesTo(edge, Vector2.zero);
        }
    }
}
