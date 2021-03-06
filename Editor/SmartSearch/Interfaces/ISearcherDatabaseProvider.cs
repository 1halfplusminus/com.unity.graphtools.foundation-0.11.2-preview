using System.Collections.Generic;
using UnityEditor.GraphToolsFoundation.Searcher;

namespace UnityEditor.GraphToolsFoundation.Overdrive
{
    /// <summary>
    /// Interface to provide different <see cref="SearcherDatabaseBase"/> depending on context.
    /// </summary>
    public interface ISearcherDatabaseProvider
    {
        /// <summary>
        /// Gets a database when searching for a graph element.
        /// </summary>
        /// <param name="graphModel">The graph in which to search for elements.</param>
        /// <returns>A <see cref="SearcherDatabaseBase"/> containing graph elements.</returns>
        List<SearcherDatabaseBase> GetGraphElementsSearcherDatabases(IGraphModel graphModel);

        /// <summary>
        /// Gets a database when searching for variable types.
        /// </summary>
        /// <returns>A <see cref="SearcherDatabaseBase"/> containing variable types.</returns>
        List<SearcherDatabaseBase> GetVariableTypesSearcherDatabases();

        /// <summary>
        /// Gets a database when searching for graph variables.
        /// </summary>
        /// <param name="graphModel">The graph in which to search for variables.</param>
        /// <returns>A <see cref="SearcherDatabaseBase"/> containing variable.</returns>
        List<SearcherDatabaseBase> GetGraphVariablesSearcherDatabases(IGraphModel graphModel);

        /// <summary>
        /// Gets a database when searching for elements that can be linked to a port.
        /// </summary>
        /// <param name="portModel">The <see cref="IPortModel"/> to link the search result to.</param>
        /// <returns>A <see cref="SearcherDatabaseBase"/> containing elements that can be linked to the port.</returns>
        List<SearcherDatabaseBase> GetDynamicSearcherDatabases(IPortModel portModel);

        /// <summary>
        /// Gets a database when searching for elements that can be linked to certain ports.
        /// </summary>
        /// <param name="portModel">The ports to link the search result to.</param>
        /// <returns>A <see cref="SearcherDatabaseBase"/> containing elements that can be linked to the port.</returns>
        List<SearcherDatabaseBase> GetDynamicSearcherDatabases(IEnumerable<IPortModel> portModel);
    }
}
