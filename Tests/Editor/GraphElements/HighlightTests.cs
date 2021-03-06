using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Overdrive.Tests.UI;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Tests.GraphElements
{
    class HighlightTests : BaseUIFixture
    {
        protected override bool CreateGraphOnStartup => true;

        IEnumerator RunTestFor<M, E>(TypeHandle typeHandle, Func<IVariableDeclarationModel, Vector2, M> creator)
            where M : IGraphElementModel
            where E : GraphElement
        {
            var declarationModel = GraphModel.CreateGraphVariableDeclaration(typeHandle, "Foo", ModifierFlags.None, true);
            var model1 = creator(declarationModel, Vector2.zero);
            var model2 = creator(declarationModel, Vector2.one * 50);

            MarkGraphViewStateDirty();
            yield return null;

            var token1 = model1.GetUI<E>(GraphView);
            var token2 = model2.GetUI<E>(GraphView);

            Assert.IsNotNull(token1);
            Assert.IsNotNull(token2);

            var selectionBorder1 = token1.SafeQ(null, "ge-selection-border");
            var selectionBorder2 = token2.SafeQ(null, "ge-selection-border");

            Assert.IsNotNull(selectionBorder1);
            Assert.IsNotNull(selectionBorder2);

            // There should be no selection at this point.
            Assert.AreEqual(Color.clear, selectionBorder1.resolvedStyle.borderBottomColor);
            Assert.AreEqual(Color.clear, selectionBorder2.resolvedStyle.borderBottomColor);

            CommandDispatcher.Dispatch(new SelectElementsCommand(SelectElementsCommand.SelectionMode.Add, model1));
            yield return null;

            token1 = model1.GetUI<E>(GraphView);
            token2 = model2.GetUI<E>(GraphView);

            selectionBorder1 = token1.SafeQ(null, "ge-selection-border");
            selectionBorder2 = token2.SafeQ(null, "ge-selection-border");

            // There should be a selection at this point.
            // The borders should not be black and should be different from one another (one selected, one highlighted).
            Assert.AreNotEqual(Color.clear, selectionBorder1.resolvedStyle.borderBottomColor);
            Assert.AreNotEqual(Color.clear, selectionBorder2.resolvedStyle.borderBottomColor);
            Assert.AreNotEqual(selectionBorder1.resolvedStyle.borderBottomColor, selectionBorder2.resolvedStyle.borderBottomColor);
        }

        [UnityTest]
        public IEnumerator HighlightIsAppliedToVariables()
        {
            var actions = RunTestFor<VariableNodeModel, TokenNode>(TypeHandle.Int,
                (m, p) => (VariableNodeModel)GraphModel.CreateVariableNode(m, p));

            while (actions.MoveNext())
                yield return null;
        }

        [UnityTest]
        public IEnumerator HighlightIsAppliedToPortals()
        {
            var actions = RunTestFor<ExecutionEdgePortalEntryModel, TokenNode>(TypeHandle.Unknown,
                (m, p) =>
                {
                    var portal = GraphModel.CreateNode<ExecutionEdgePortalEntryModel>("foo", p);
                    portal.DeclarationModel = m;
                    return portal;
                });

            while (actions.MoveNext())
                yield return null;
        }
    }
}
