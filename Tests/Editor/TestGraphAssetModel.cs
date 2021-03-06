using System;
using System.IO;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests
{
    [Serializable]
    class TestGraphAssetModel : GraphAssetModel
    {
        protected override Type GraphModelType => typeof(TestGraphModel);
    }

    [Serializable]
    class OtherTestGraphAssetModel : GraphAssetModel
    {
        protected override Type GraphModelType => typeof(OtherTestGraphModel);
    }
}
