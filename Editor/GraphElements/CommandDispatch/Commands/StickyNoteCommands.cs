using System.Collections.Generic;
using System.Linq;
using UnityEngine.GraphToolsFoundation.CommandStateObserver;
using UnityEngine;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Command to create a new sticky note.
    /// </summary>
    public class CreateStickyNoteCommand : UndoableCommand
    {
        /// <summary>
        /// The position and size of the new sticky note.
        /// </summary>
        public readonly Rect Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStickyNoteCommand"/> class.
        /// </summary>
        public CreateStickyNoteCommand()
        {
            UndoString = "Create Sticky Note";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateStickyNoteCommand"/> class.
        /// </summary>
        /// <param name="position">The position and size of the new sticky note.</param>
        public CreateStickyNoteCommand(Rect position) : this()
        {
            Position = position;
        }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, CreateStickyNoteCommand command)
        {
            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                var stickyNote = graphToolState.GraphViewState.GraphModel.CreateStickyNote(command.Position);
                graphUpdater.MarkNew(stickyNote);
            }
        }
    }

    /// <summary>
    /// Command to update the title and content of a sticky note.
    /// </summary>
    public class UpdateStickyNoteCommand : UndoableCommand
    {
        /// <summary>
        /// The new title, or null if the title should not be updated.
        /// </summary>
        public readonly string Title;
        /// <summary>
        /// The new content, or null if the content should not be updated.
        /// </summary>
        public readonly string Contents;
        /// <summary>
        /// The sticky note model to update.
        /// </summary>
        public readonly IStickyNoteModel StickyNoteModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteCommand"/> class.
        /// </summary>
        public UpdateStickyNoteCommand()
        {
            UndoString = "Update Sticky Note Content";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteCommand"/> class.
        /// </summary>
        /// <param name="stickyNoteModel">The sticky note model to update.</param>
        /// <param name="title">The new title, or null if the title should not be updated.</param>
        /// <param name="contents">The new content, or null if the content should not be updated.</param>
        public UpdateStickyNoteCommand(IStickyNoteModel stickyNoteModel, string title, string contents) : this()
        {
            StickyNoteModel = stickyNoteModel;
            Title = title;
            Contents = contents;
        }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, UpdateStickyNoteCommand command)
        {
            if (command.Title == null && command.Contents == null)
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                if (command.Title != null)
                    command.StickyNoteModel.Title = command.Title;

                if (command.Contents != null)
                    command.StickyNoteModel.Contents = command.Contents;

                graphUpdater.MarkChanged(command.StickyNoteModel);
            }
        }
    }

    /// <summary>
    /// Command to update the theme of sticky notes.
    /// </summary>
    public class UpdateStickyNoteThemeCommand : ModelCommand<IStickyNoteModel, string>
    {
        const string k_UndoStringSingular = "Change Sticky Note Theme";
        const string k_UndoStringPlural = "Change Sticky Notes Theme";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteThemeCommand"/> class.
        /// </summary>
        public UpdateStickyNoteThemeCommand()
            : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteThemeCommand"/> class.
        /// </summary>
        /// <param name="theme">The new theme.</param>
        /// <param name="stickyNoteModels">The sticky notes to update.</param>
        public UpdateStickyNoteThemeCommand(string theme, IReadOnlyList<IStickyNoteModel> stickyNoteModels)
            : base(k_UndoStringSingular, k_UndoStringPlural, theme, stickyNoteModels) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteThemeCommand"/> class.
        /// </summary>
        /// <param name="theme">The new theme.</param>
        /// <param name="stickyNoteModels">The sticky notes to update.</param>
        public UpdateStickyNoteThemeCommand(string theme, params IStickyNoteModel[] stickyNoteModels)
            : this(theme, (IReadOnlyList<IStickyNoteModel>)stickyNoteModels) { }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, UpdateStickyNoteThemeCommand command)
        {
            if (!command.Models.Any())
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var noteModel in command.Models)
                {
                    noteModel.Theme = command.Value;
                }

                graphUpdater.MarkChanged(command.Models);
            }
        }
    }

    /// <summary>
    /// Command to update the text size of a sticky note.
    /// </summary>
    public class UpdateStickyNoteTextSizeCommand : ModelCommand<IStickyNoteModel, string>
    {
        const string k_UndoStringSingular = "Change Sticky Note Font Size";
        const string k_UndoStringPlural = "Change Sticky Notes Font Size";

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteTextSizeCommand"/> class.
        /// </summary>
        public UpdateStickyNoteTextSizeCommand()
            : base(k_UndoStringSingular) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteTextSizeCommand"/> class.
        /// </summary>
        /// <param name="textSize">The new text size.</param>
        /// <param name="stickyNoteModels">The sticky note models to update.</param>
        public UpdateStickyNoteTextSizeCommand(string textSize, IReadOnlyList<IStickyNoteModel> stickyNoteModels)
            : base(k_UndoStringSingular, k_UndoStringPlural, textSize, stickyNoteModels) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateStickyNoteTextSizeCommand"/> class.
        /// </summary>
        /// <param name="textSize">The new text size.</param>
        /// <param name="stickyNoteModels">The sticky note models to update.</param>
        public UpdateStickyNoteTextSizeCommand(string textSize, params IStickyNoteModel[] stickyNoteModels)
            : this(textSize, (IReadOnlyList<IStickyNoteModel>)stickyNoteModels) { }

        /// <summary>
        /// Default command handler.
        /// </summary>
        /// <param name="graphToolState">The state.</param>
        /// <param name="command">The command.</param>
        public static void DefaultCommandHandler(GraphToolState graphToolState, UpdateStickyNoteTextSizeCommand command)
        {
            if (!command.Models.Any())
                return;

            graphToolState.PushUndo(command);

            using (var graphUpdater = graphToolState.GraphViewState.UpdateScope)
            {
                foreach (var noteModel in command.Models)
                {
                    noteModel.TextSize = command.Value;
                }

                graphUpdater.MarkChanged(command.Models);
            }
        }
    }
}
