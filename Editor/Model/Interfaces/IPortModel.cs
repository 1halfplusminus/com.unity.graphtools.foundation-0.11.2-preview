using System;
using System.Collections.Generic;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// The number of connections a port can accept.
    /// </summary>
    public enum PortCapacity
    {
        /// <summary>
        /// The port can only accept a single connection.
        /// </summary>
        Single,

        /// <summary>
        /// The port can accept multiple connections.
        /// </summary>
        Multi
    }

    /// <summary>
    /// Options for the port.
    /// </summary>
    [Flags]
    public enum PortModelOptions
    {
        /// <summary>
        /// No option set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The port has no constant to set its value when not connected.
        /// </summary>
        NoEmbeddedConstant = 1,

        /// <summary>
        /// The port is hidden.
        /// </summary>
        Hidden = 2,

        /// <summary>
        /// Default port options.
        /// </summary>
        Default = None,
    }

    /// <summary>
    /// Interface for ports.
    /// </summary>
    public interface IPortModel : IGraphElementModel
    {
        IPortNodeModel NodeModel { get; set; }
        PortDirection Direction { get; set; }
        PortType PortType { get; set; }
        PortOrientation Orientation { get; set; }
        PortCapacity Capacity { get; }
        Type PortDataType { get; }
        PortModelOptions Options { get; set; }

        TypeHandle DataTypeHandle { get; set; }
        string ToolTip { get; set; }

        bool CreateEmbeddedValueIfNeeded { get; }

        IEnumerable<IPortModel> GetConnectedPorts();
        IEnumerable<IEdgeModel> GetConnectedEdges();
        bool IsConnectedTo(IPortModel toPort);

        PortCapacity GetDefaultCapacity();
        IConstant EmbeddedValue { get; }
        bool DisableEmbeddedValueEditor { get; }

        string UniqueName { get; }
    }
}
