using System.Collections.Generic;
using System.Linq;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Command to create a new variable node.
    /// </summary>
    public class CreateVariableNodesCommand : UndoableCommand
    {
        /// <summary>
        /// The variable for which to create nodes, their GUID and position.
        /// </summary>
        public List<(IVariableDeclarationModel, SerializableGUID, Vector2)> VariablesToCreate;
        /// <summary>
        /// The port to which to connect.
        /// </summary>
        public IPortModel ConnectAfterCreation;
        /// <summary>
        /// Edges to delete.
        /// </summary>
        public IReadOnlyList<IEdgeModel> EdgeModelsToDelete;
        /// <summary>
        /// True if the new node should be aligned to the connected port.
        /// </summary>
        public bool AutoAlign;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateVariableNodesCommand"/> class.
        /// </summary>
        public CreateVariableNodesCommand()
        {
            UndoString = "Create Variable Node";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateVariableNodesCommand"/> class.
        /// </summary>
        /// <param name="variablesToCreate">The variables for which to create nodes.</param>
        public CreateVariableNodesCommand(IReadOnlyList<(IVariableDeclarationModel, SerializableGUID, Vector2)> variablesToCreate) : this()
        {
            VariablesToCreate = variablesToCreate?.ToList() ?? new List<(IVariableDeclarationModel, SerializableGUID, Vector2)>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateVariableNodesCommand"/> class.
        /// </summary>
        /// <param name="graphElementModel">The variable for which to create nodes.</param>
        /// <param name="mousePosition">The location of the new node.</param>
        /// <param name="edgeModelsToDelete">Edges to delete.</param>
        /// <param name="connectAfterCreation">The port to which to connect.</param>
        /// <param name="autoAlign">True if the new node should be aligned to the connected port.</param>
        public CreateVariableNodesCommand(IVariableDeclarationModel graphElementModel, Vector2 mousePosition,
                                          IReadOnlyList<IEdgeModel> edgeModelsToDelete = null, IPortModel connectAfterCreation = null,
                                          bool autoAlign = false) : this()
        {
            VariablesToCreate = new List<(IVariableDeclarationModel, SerializableGUID, Vector2)>
            {
                (graphElementModel, SerializableGUID.Generate(), mousePosition)
            };
            EdgeModelsToDelete = edgeModelsToDelete;
            ConnectAfterCreation = connectAfterCreation;
            AutoAlign = autoAlign;
        }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateVariableNodesCommand command)
        {
            if (command.VariablesToCreate.Count > 0)
            {
                graphToolState.PushUndo(command);

                using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
                {
                    var edgesToDelete = command.EdgeModelsToDelete ?? new List<IEdgeModel>();

                    // Delete previous connections
                    var portToConnect = command.ConnectAfterCreation;
                    if (portToConnect != null && portToConnect.Capacity != PortCapacity.Multi)
                    {
                        var existingEdges = portToConnect.GetConnectedEdges();
                        edgesToDelete = edgesToDelete.Concat(existingEdges).ToList();
                    }

                    // Delete previous connections
                    if (edgesToDelete.Any())
                    {
                        graphToolState.GraphViewState.GraphModel.DeleteEdges(edgesToDelete);
                        graphUpdater.MarkDeleted(edgesToDelete);
                    }

                    foreach (var (variableDeclarationModel, guid, position) in command.VariablesToCreate)
                    {
                        var vsGraphModel = graphToolState.GraphViewState.GraphModel;

                        var newVariable = vsGraphModel.CreateVariableNode(variableDeclarationModel, position, guid: guid);
                        graphUpdater.MarkNew(newVariable);

                        if (portToConnect != null)
                        {
                            var newEdge =
                                graphToolState.GraphViewState.GraphModel.CreateEdge(portToConnect, newVariable.OutputPort);
                            graphUpdater.MarkNew(newEdge);
                            if (command.AutoAlign)
                            {
                                graphUpdater.MarkModelToAutoAlign(newEdge);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A command to convert variables to constants and vice versa.
    /// </summary>
    public class ConvertConstantNodesAndVariableNodesCommand : UndoableCommand
    {
        /// <summary>
        /// The constant nodes to convert to variable nodes.
        /// </summary>
        public IReadOnlyList<IConstantNodeModel> ConstantNodeModels;
        /// <summary>
        /// The variable nodes to convert to constant nodes.
        /// </summary>
        public IReadOnlyList<IVariableNodeModel> VariableNodeModels;

        const string k_UndoString = "Convert Constants And Variables";
        const string k_UndoStringCToVSingular = "Convert Constant To Variable";
        const string k_UndoStringCToVPlural = "Convert Constants To Variables";
        const string k_UndoStringVToCSingular = "Convert Variable To Constant";
        const string k_UndoStringVToCPlural = "Convert Variables To Constants";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertConstantNodesAndVariableNodesCommand" /> class.
        /// </summary>
        public ConvertConstantNodesAndVariableNodesCommand()
        {
            UndoString = k_UndoString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertConstantNodesAndVariableNodesCommand" /> class.
        /// </summary>
        /// <param name="constantNodeModels">The constants to convert to variables.</param>
        /// <param name="variableNodeModels">The variables to convert to constants.</param>
        public ConvertConstantNodesAndVariableNodesCommand(
            IReadOnlyList<IConstantNodeModel> constantNodeModels,
            IReadOnlyList<IVariableNodeModel> variableNodeModels)
        {
            ConstantNodeModels = constantNodeModels;
            VariableNodeModels = variableNodeModels;

            var constantCount = ConstantNodeModels?.Count ?? 0;
            var variableCount = VariableNodeModels?.Count ?? 0;

            if (constantCount == 0)
            {
                if (variableCount == 1)
                {
                    UndoString = k_UndoStringVToCSingular;
                }
                else
                {
                    UndoString = k_UndoStringVToCPlural;
                }
            }
            else if (variableCount == 0)
            {
                if (constantCount == 1)
                {
                    UndoString = k_UndoStringCToVSingular;
                }
                else
                {
                    UndoString = k_UndoStringCToVPlural;
                }
            }
            else
            {
                UndoString = k_UndoString;
            }
        }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state to modify.</param>
        /// <param name="command">The command to apply to the state.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, ConvertConstantNodesAndVariableNodesCommand command)
        {
            if ((command.ConstantNodeModels?.Count ?? 0) == 0 && (command.VariableNodeModels?.Count ?? 0) == 0)
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            using (var selectionUpdater = graphToolState.SelectionState.UpdateScope)
            {
                var graphModel = graphToolState.GraphViewState.GraphModel;

                foreach (var constantModel in command.ConstantNodeModels ?? Enumerable.Empty<IConstantNodeModel>())
                {
                    var declarationModel = graphModel.CreateGraphVariableDeclaration(
                        constantModel.Type.GenerateTypeHandle(),
                        constantModel.Type.FriendlyName().CodifyStringInternal(), ModifierFlags.None, true,
                        constantModel.Value.CloneConstant());
                    graphUpdater.MarkNew(declarationModel);

                    var variableModel = graphModel.CreateVariableNode(declarationModel, constantModel.Position);
                    if (variableModel != null)
                    {
                        graphUpdater.MarkNew(variableModel);
                        selectionUpdater.SelectElement(variableModel, true);

                        variableModel.State = constantModel.State;
                        if (constantModel.HasUserColor)
                            variableModel.Color = constantModel.Color;
                        foreach (var edge in graphModel.GetEdgesConnections(constantModel.OutputPort).ToList())
                        {
                            var newEdge = graphModel.CreateEdge(edge.ToPort, variableModel.OutputPort);
                            var deletedModels = graphModel.DeleteEdge(edge);

                            graphUpdater.MarkNew(newEdge);
                            graphUpdater.MarkDeleted(deletedModels);
                            selectionUpdater.SelectElements(deletedModels, false);
                        }
                    }

                    var deletedElements = graphModel.DeleteNode(constantModel, deleteConnections: false);
                    graphUpdater.MarkDeleted(deletedElements);
                    selectionUpdater.SelectElements(deletedElements, false);
                }

                foreach (var variableModel in command.VariableNodeModels ?? Enumerable.Empty<IVariableNodeModel>())
                {
                    if (graphModel.Stencil.GetConstantNodeValueType(variableModel.GetDataType()) == null)
                        continue;
                    var constantModel = graphModel.CreateConstantNode(variableModel.GetDataType(), variableModel.Title, variableModel.Position);
                    constantModel.ObjectValue = variableModel.VariableDeclarationModel?.InitializationModel?.ObjectValue;
                    constantModel.State = variableModel.State;
                    if (variableModel.HasUserColor)
                        constantModel.Color = variableModel.Color;
                    graphUpdater.MarkNew(constantModel);
                    selectionUpdater.SelectElement(constantModel, true);

                    var edgeModels = graphModel.GetEdgesConnections(variableModel.OutputPort).ToList();
                    foreach (var edge in edgeModels)
                    {
                        var newEdge = graphModel.CreateEdge(edge.ToPort, constantModel.OutputPort);
                        var deletedModels = graphModel.DeleteEdge(edge);
                        graphUpdater.MarkNew(newEdge);
                        graphUpdater.MarkDeleted(deletedModels);
                        selectionUpdater.SelectElements(deletedModels, false);
                    }

                    var deletedElements = graphModel.DeleteNode(variableModel, deleteConnections: false);
                    graphUpdater.MarkDeleted(deletedElements);
                    selectionUpdater.SelectElements(deletedElements, false);
                }
            }
        }
    }

    /// <summary>
    /// Command to itemize a node.
    /// </summary>
    public class ItemizeNodeCommand : ModelCommand<ISingleOutputPortNodeModel>
    {
        const string k_UndoStringSingular = "Itemize Node";
        const string k_UndoStringPlural = "Itemize Nodes";

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemizeNodeCommand"/> class.
        /// </summary>
        public ItemizeNodeCommand()
            : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemizeNodeCommand"/> class.
        /// </summary>
        /// <param name="models">The nodes to itemize.</param>
        public ItemizeNodeCommand(IReadOnlyList<ISingleOutputPortNodeModel> models)
            : base(k_UndoStringSingular, k_UndoStringPlural, models) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemizeNodeCommand"/> class.
        /// </summary>
        /// <param name="models">The nodes to itemize.</param>
        public ItemizeNodeCommand(params ISingleOutputPortNodeModel[] models)
            : this((IReadOnlyList<ISingleOutputPortNodeModel>)models) { }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, ItemizeNodeCommand command)
        {
            bool undoPushed = false;

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                var graphModel = graphToolState.GraphViewState.GraphModel;
                foreach (var model in command.Models.Where(m => m is IVariableNodeModel || m is IConstantNodeModel))
                {
                    var edges = graphModel.GetEdgesConnections(model.OutputPort).ToList();

                    for (var i = 1; i < edges.Count; i++)
                    {
                        if (!undoPushed)
                        {
                            undoPushed = true;
                            graphToolState.PushUndo(command);
                        }

                        var newModel = (ISingleOutputPortNodeModel)graphModel.DuplicateNode(model, i * 50 * Vector2.up);
                        graphUpdater.MarkNew(newModel);
                        var edge = edges[i];
                        var newEdge = graphModel.CreateEdge(edge.ToPort, newModel.OutputPort);
                        var deletedModels = graphModel.DeleteEdge(edge);
                        graphUpdater.MarkNew(newEdge);
                        graphUpdater.MarkDeleted(deletedModels);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Command to set the locked state of constant nodes.
    /// </summary>
    public class LockConstantNodeCommand : ModelCommand<IConstantNodeModel, bool>
    {
        const string k_UndoStringSingular = "Toggle Lock Constant";
        const string k_UndoStringPlural = "Toggle Lock Constants";

        /// <summary>
        /// Initializes a new instance of the <see cref="LockConstantNodeCommand"/> class.
        /// </summary>
        public LockConstantNodeCommand()
            : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockConstantNodeCommand"/> class.
        /// </summary>
        /// <param name="constantNodeModels">The constant nodes for which the locked state should be toggled.</param>
        /// <param name="locked">Whether to lock or unlock the constant nodes.</param>
        public LockConstantNodeCommand(IReadOnlyList<IConstantNodeModel> constantNodeModels, bool locked)
            : base(k_UndoStringSingular, k_UndoStringPlural, locked, constantNodeModels) { }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, LockConstantNodeCommand command)
        {
            if (!command.Models.Any())
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var constantNodeModel in command.Models)
                {
                    constantNodeModel.IsLocked = command.Value;
                }

                graphUpdater.MarkChanged(command.Models);
            }
        }
    }

    /// <summary>
    /// Command to change the variable declaration of variable nodes.
    /// </summary>
    public class ChangeVariableDeclarationCommand : ModelCommand<IVariableNodeModel>
    {
        const string k_UndoStringSingular = "Change Variable";

        /// <summary>
        /// The new variable declaration for the nodes.
        /// </summary>
        public readonly IVariableDeclarationModel Variable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeVariableDeclarationCommand"/> class.
        /// </summary>
        public ChangeVariableDeclarationCommand()
            : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeVariableDeclarationCommand"/> class.
        /// </summary>
        /// <param name="models">The variable node for which to change the variable declaration.</param>
        /// <param name="variable">The new variable declaration.</param>
        public ChangeVariableDeclarationCommand(IReadOnlyList<IVariableNodeModel> models, IVariableDeclarationModel variable)
            : base(k_UndoStringSingular, k_UndoStringSingular, models)
        {
            Variable = variable;
        }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, ChangeVariableDeclarationCommand command)
        {
            if (!command.Models.Any())
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var model in command.Models)
                {
                    model.DeclarationModel = command.Variable;

                    var references = graphToolState.GraphViewState.GraphModel.FindReferencesInGraph<IVariableNodeModel>(command.Variable);
                    graphUpdater.MarkChanged(references);
                }

                graphUpdater.MarkChanged(command.Models);
            }
        }
    }
}
