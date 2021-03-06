using System;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Command to set the value of a field on an model.
    /// </summary>
    public class SetModelFieldCommand : ModelCommand<IGraphElementModel, object>
    {
        const string k_UndoStringSingular = "Set Property";

        /// <summary>
        /// The name of the field to set.
        /// </summary>
        public string FieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetModelFieldCommand"/> class.
        /// </summary>
        public SetModelFieldCommand() : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetModelFieldCommand"/> class.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="elementModel">The model that owns the field.</param>
        /// <param name="fieldName">The name of the field to set.</param>
        public SetModelFieldCommand(object value, IGraphElementModel elementModel, string fieldName)
            : base(k_UndoStringSingular, k_UndoStringSingular, value, new[] { elementModel })
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Default command handler
        /// </summary>
        /// <param name="graphToolState">The state to modify.</param>
        /// <param name="command">The command to apply to the state.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, SetModelFieldCommand command)
        {
            graphToolState.PushUndo(command);

            if (command.Models != null)
            {
                using (var updater = graphToolState.GraphViewState.UpdateScope)
                {
                    foreach (var model in command.Models)
                    {
                        var target = model is IHasInspectorSurrogate hasInspectorSurrogate ? hasInspectorSurrogate.Surrogate : model;
                        if (target != null)
                        {
                            var fieldInfo = SerializedFieldsInspector.GetInspectableField(target, command.FieldName);
                            fieldInfo?.SetValue(target, command.Value);
                        }
                    }

                    updater.MarkChanged(command.Models);
                }
            }
        }
    }
}
