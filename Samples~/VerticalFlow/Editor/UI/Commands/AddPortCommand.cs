using System.Linq;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.Vertical
{
    class AddPortCommand : ModelCommand<VerticalNodeModel>
    {
        const string k_UndoStringSingular = "Add Port";

        readonly PortDirection m_PortDirection;
        readonly PortOrientation m_PortOrientation;

        public AddPortCommand(PortDirection direction, PortOrientation orientation, params VerticalNodeModel[] nodes)
            : base(k_UndoStringSingular, k_UndoStringSingular, nodes)
        {
            m_PortDirection = direction;
            m_PortOrientation = orientation;
        }

        public static void DefaultHandler(GraphToolState state, AddPortCommand command)
        {
            if (!command.Models.Any() || command.m_PortDirection == PortDirection.None)
                return;

            state.PushUndo(command);

            using (var updater = state.GraphViewState.UpdateScope)
            {
                foreach (var nodeModel in command.Models)
                    nodeModel.AddPort(command.m_PortOrientation, command.m_PortDirection);

                updater.MarkChanged(command.Models);
            }
        }
    }
}
