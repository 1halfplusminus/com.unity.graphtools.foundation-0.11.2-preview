using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor.GraphToolsFoundation.Overdrive.Tests.TestModels;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GTFO.UIFromModelTests
{
    public class PortCommandTests : BaseTestFixture
    {
        [UnityTest]
        public IEnumerator DraggingFromPortCreateGhostEdge()
        {
            var nodeModel = GraphModel.CreateNode<SingleOutputNodeModel>();
            MarkGraphViewStateDirty();
            yield return null;

            var portModel = nodeModel.Ports.First();
            var port = portModel.GetUI<Port>(GraphView);
            Assert.IsNotNull(port);
            Assert.IsNull(port.EdgeConnector.edgeDragHelper.edgeCandidateModel);

            var portConnector = port.SafeQ(PortConnectorPart.connectorUssName);
            var clickPosition = portConnector.parent.LocalToWorld(portConnector.layout.center);
            Vector2 move = new Vector2(0, 100);
            EventHelper.DragToNoRelease(clickPosition, clickPosition + move);
            yield return null;

            // edgeCandidateModel != null is the sign that we have a ghost edge
            Assert.IsNotNull(port.EdgeConnector.edgeDragHelper.edgeCandidateModel);

            EventHelper.MouseUpEvent(clickPosition + move);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReleasingInPortCallsDelegate()
        {
            var nodeModel1 = GraphModel.CreateNode<SingleOutputNodeModel>();
            nodeModel1.Position = Vector2.zero;

            var nodeModel2 = GraphModel.CreateNode<SingleInputNodeModel>();
            nodeModel2.Position = new Vector2(0, 200);
            MarkGraphViewStateDirty();
            yield return null;

            var outPortModel = nodeModel1.Ports.First();
            var outPort = outPortModel.GetUI<Port>(GraphView);
            Assert.IsNotNull(outPort);

            var inPortModel = nodeModel2.Ports.First();
            var inPort = inPortModel.GetUI<Port>(GraphView);
            Assert.IsNotNull(inPort);

            bool insideOutputPortDelegateCalled = false;
            bool insideInputPortDelegateCalled = false;
            bool outsideOutputPortDelegateCalled = false;
            bool outsideInputPortDelegateCalled = false;

            outPort.EdgeConnector.SetDropDelegate((s, e) => insideOutputPortDelegateCalled = true);
            outPort.EdgeConnector.SetDropOutsideDelegate((s, e, p, v) => outsideOutputPortDelegateCalled = true);
            inPort.EdgeConnector.SetDropDelegate((s, e) => insideInputPortDelegateCalled = true);
            inPort.EdgeConnector.SetDropOutsideDelegate((s, e, p, v) => outsideInputPortDelegateCalled = true);

            var outPortConnector = outPort.SafeQ(PortConnectorPart.connectorUssName);
            var inPortConnector = inPort.SafeQ(PortConnectorPart.connectorUssName);
            var clickPosition = outPortConnector.parent.LocalToWorld(outPortConnector.layout.center);
            var releasePosition = inPortConnector.parent.LocalToWorld(inPortConnector.layout.center);
            EventHelper.DragTo(clickPosition, releasePosition);
            yield return null;

            Assert.IsFalse(insideInputPortDelegateCalled);
            Assert.IsTrue(insideOutputPortDelegateCalled);
            Assert.IsFalse(outsideInputPortDelegateCalled);
            Assert.IsFalse(outsideOutputPortDelegateCalled);
        }

        [UnityTest]
        public IEnumerator ReleasingOutsidePortCallsDelegate()
        {
            var nodeModel1 = GraphModel.CreateNode<SingleOutputNodeModel>();
            nodeModel1.Position = Vector2.zero;

            var nodeModel2 = GraphModel.CreateNode<SingleInputNodeModel>();
            nodeModel2.Position = new Vector2(0, 200);
            MarkGraphViewStateDirty();
            yield return null;

            var outPortModel = nodeModel1.Ports.First();
            var outPort = outPortModel.GetUI<Port>(GraphView);
            Assert.IsNotNull(outPort);

            var inPortModel = nodeModel2.Ports.First();
            var inPort = inPortModel.GetUI<Port>(GraphView);
            Assert.IsNotNull(inPort);

            bool insideOutputPortDelegateCalled = false;
            bool insideInputPortDelegateCalled = false;
            bool outsideOutputPortDelegateCalled = false;
            bool outsideInputPortDelegateCalled = false;

            outPort.EdgeConnector.SetDropDelegate((s, e) => insideOutputPortDelegateCalled = true);
            outPort.EdgeConnector.SetDropOutsideDelegate((s, e, p, v) => outsideOutputPortDelegateCalled = true);
            inPort.EdgeConnector.SetDropDelegate((s, e) => insideInputPortDelegateCalled = true);
            inPort.EdgeConnector.SetDropOutsideDelegate((s, e, p, v) => outsideInputPortDelegateCalled = true);

            var outPortConnector = outPort.SafeQ(PortConnectorPart.connectorUssName);
            var inPortConnector = inPort.SafeQ(PortConnectorPart.connectorUssName);
            var clickPosition = outPortConnector.parent.LocalToWorld(outPortConnector.layout.center);
            var releasePosition = inPortConnector.parent.LocalToWorld(inPortConnector.layout.center);
            EventHelper.DragTo(clickPosition, releasePosition + 400 * Vector2.down);
            yield return null;

            Assert.IsFalse(insideInputPortDelegateCalled);
            Assert.IsFalse(insideOutputPortDelegateCalled);
            Assert.IsFalse(outsideInputPortDelegateCalled);
            Assert.IsTrue(outsideOutputPortDelegateCalled);
        }
    }
}
