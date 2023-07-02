/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Services {

    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
    using Types;

    /// <summary>
    /// Manage predictor graphs.
    /// </summary>
    public sealed class GraphService {
        
        #region --Client API--
        /// <summary>
        /// Retrieve a predictor graph.
        /// </summary>
        /// <param name="tag">Graph tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <returns>Predictor graph.</returns>
        public async Task<Graph> Retrieve (string tag, GraphFormat format) {
            // Ensure this is a predictor tag
            Tag.TryParse(tag, out var parsedTag);
            var variant = parsedTag.variant ?? @"main";
            var predictorTag = new Tag(parsedTag.username, parsedTag.name);
            // There isn't a query to get a specific graph so just filter for now
            var graphs = await List(predictorTag);
            var graph = graphs?.FirstOrDefault(graph => graph.variant == variant && graph.format == format);
            // Return
            return graph;
        }

        /// <summary>
        /// List all predictor graphs.
        /// </summary>
        /// <param name="tag">Predictor tag. This MUST NOT be a variant tag.</param>
        /// <returns>Predictor graphs.</returns>
        public async Task<Graph[]?> List (string tag) {
            var predictor = await client.Query<Predictor>(
                @$"query ($input: PredictorInput!) {{
                    predictor (input: $input) {{
                        graphs {{
                            {Fields}
                        }}
                    }}
                }}",
                @"predictor",
                new () {
                    ["input"] = new PredictorInput {
                        tag = tag
                    }
                }
            );
            return predictor?.graphs;
        }

        /// <summary>
        /// Create a predictor graph.
        /// </summary>
        /// <param name="tag">Graph tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <param name="graph">Graph URL.</param>
        /// <param name="format">Graph format.</param>
        public Task<Graph> Create (
            string tag,
            string graph,
            GraphFormat format
        ) => client.Query<Graph>(
            @$"mutation ($input: CreateGraphInput!) {{
                createGraph (input: $input) {{
                    {Fields}
                }}
            }}",
            @"createGraph",
            new () {
                ["input"] = new CreateGraphInput {
                    tag = tag,
                    graph = graph,
                    format = format
                }
            }
        );

        /// <summary>
        /// Delete a predictor graph.
        /// </summary>
        /// <param name="tag">Graph tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <param name="format">Graph format.</param>
        /// <returns>Whether the graph was successfully deleted.</returns>
        public Task<bool> Delete (
            string tag,
            GraphFormat format
        ) => client.Query<bool>(
            @$"mutation ($input: DeleteGraphInput!) {{
                deleteGraph (input: $input)
            }}",
            @"deleteGraph",
            new () {
                ["input"] = new DeleteGraphInput {
                    tag = tag,
                    format = format
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        private const string Fields = @"
        variant
        format
        status
        encrypted
        created
        error
        ";

        internal GraphService (IGraphClient client) => this.client = client;
        #endregion
    }

    #region --Types--

    internal sealed class CreateGraphInput {
        public string tag;
        public string graph;
        public GraphFormat format;
    }

    internal sealed class DeleteGraphInput {
        public string tag;
        public string graph;
        public GraphFormat format;
    }
    #endregion
}