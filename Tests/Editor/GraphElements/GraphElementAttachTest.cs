using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Bridge;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GraphElements
{
    class GraphElementAttachTests : GraphViewTester
    {
        private static readonly Rect k_NodeRect = new Rect(SelectionDragger.panAreaWidth * 2, SelectionDragger.panAreaWidth * 3, 50, 50);

        Attacher CreateAttachedElement<T>(TestGraphViewWindow window) where T : VisualElement
        {
            T target = graphView.SafeQ<T>();

            Attacher attacher = null;
            if (target != null)
            {
                VisualElement attached = new VisualElement
                {
                    style =
                    {
                        position = Position.Absolute,
                        bottom = 10,
                        right = 10,
                        backgroundColor = Color.blue
                    }
                };

                target.parent.Add(attached);
                attacher = new Attacher(attached, target, SpriteAlignment.LeftCenter);
                attached.userData = attacher;
            }

            return attacher;
        }

        [UnityTest]
        public IEnumerator AttachedElementIsPlacedProperlyAndFollowsNode()
        {
            // Create node.
            var nodeModel = CreateNode("Node 1", k_NodeRect.position);
            CommandDispatcher.Dispatch(new ReframeGraphViewCommand(Vector3.zero, Vector3.one));
            yield return null;

            var node = nodeModel.GetUI<Node>(graphView);
            node.style.width = k_NodeRect.width;
            node.style.height = k_NodeRect.height;

            var attacher = CreateAttachedElement<Node>(window);
            Assert.AreNotEqual(null, attacher);
            yield return null;

            var initialPosition = nodeModel.Position;
            Assert.AreEqual(attacher.Target.layout.center.y, attacher.Element.layout.center.y);
            Assert.AreNotEqual(attacher.Target.layout.center.x, attacher.Element.layout.center.x);

            var mouseDownPosition = initialPosition + k_NodeRect.size / 2;
            mouseDownPosition = graphView.contentContainer.LocalToWorld(mouseDownPosition);
            var delta = Vector2.one * 10;
            // Move the movable node.
            helpers.MouseDownEvent(mouseDownPosition);
            yield return null;

            helpers.MouseDragEvent(mouseDownPosition, mouseDownPosition + delta);
            yield return null;

            helpers.MouseUpEvent(mouseDownPosition + delta);
            yield return null;

            Assert.AreNotEqual(initialPosition, nodeModel.Position);
            Assert.AreEqual(attacher.Target.layout.center.y, attacher.Element.layout.center.y);
            Assert.AreNotEqual(attacher.Target.layout.center.x, attacher.Element.layout.center.x);

            yield return null;
        }
    }
}
