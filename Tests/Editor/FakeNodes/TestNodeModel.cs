using System;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests
{
    [Serializable]
    class TestNodeModel : NodeModel
    {
        protected override void OnDefineNode()
        {
            this.AddDataInputPort<float>("one");
        }
    }
}
