using System.Collections.Generic;
using System.Linq;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Graph processing options.
    /// </summary>
    public enum RequestGraphProcessingOptions
    {
        /// <summary>
        /// Process the graph.
        /// </summary>
        Default,

        /// <summary>
        /// Save the graph and process it.
        /// </summary>
        SaveGraph,
    }

    /// <summary>
    /// Helper class for graph processing.
    /// </summary>
    public static class GraphProcessingHelper
    {
        static GraphProcessingOptions GetGraphProcessingOptions(bool tracingEnabled)
        {
            GraphProcessingOptions graphProcessingOptions = EditorApplication.isPlaying
                ? GraphProcessingOptions.LiveEditing
                : GraphProcessingOptions.Default;

            if (tracingEnabled)
                graphProcessingOptions |= GraphProcessingOptions.Tracing;

            return graphProcessingOptions;
        }

        /// <summary>
        /// Processes the graph using the graph processor returned by <see cref="Stencil.CreateGraphProcessor"/>.
        /// </summary>
        /// <param name="graphModel">The graph to process.</param>
        /// <param name="pluginRepository">The plugin repository.</param>
        /// <param name="options">Graph processing options.</param>
        /// <param name="tracingEnabled">True if tracing is enabled.</param>
        /// <returns>The result of a graph processing.</returns>
        public static GraphProcessingResult ProcessGraph(this IGraphModel graphModel, PluginRepository pluginRepository, RequestGraphProcessingOptions options, bool tracingEnabled)
        {
            var stencil = (Stencil)graphModel?.Stencil;
            if (stencil == null)
                return null;

            if (pluginRepository != null)
            {
                var graphProcessingOptions = GetGraphProcessingOptions(tracingEnabled);
                var plugins = stencil.GetGraphProcessingPluginHandlers(graphProcessingOptions);
                pluginRepository.RegisterPlugins(plugins);
            }

            var graphProcessor = stencil.CreateGraphProcessor();
            if (options == RequestGraphProcessingOptions.SaveGraph)
                AssetDatabase.SaveAssets();

            return graphProcessor.ProcessGraph(graphModel);
        }

        /// <summary>
        /// Converts the errors generated by the processing of the graph to instances of <see cref="IGraphProcessingErrorModel"/>.
        /// </summary>
        /// <param name="stencil">The stencil.</param>
        /// <param name="results">The graph processing results used as the source of errors to convert.</param>
        /// <returns>The converted errors.</returns>
        public static IEnumerable<IGraphProcessingErrorModel> GetErrors(Stencil stencil, GraphProcessingResult results)
        {
            if (results?.Errors != null)
                return results.Errors.Select(stencil.CreateProcessingErrorModel).Where(m => m != null);

            return Enumerable.Empty<IGraphProcessingErrorModel>();
        }
    }
}
