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
    /// Manage predictor endpoints.
    /// </summary>
    public sealed class EndpointService {

        #region --Client API--
        /// <summary>
        /// Retrieve a predictor endpoint.
        /// </summary>
        /// <param name="tag">Endpoint tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <returns>Predictor endpoint.</returns>
        public async Task<Endpoint?> Retrieve (string tag) {
            // Ensure this is a predictor tag
            Tag.TryParse(tag, out var parsedTag);
            var variant = parsedTag.variant ?? @"main";
            var predictorTag = new Tag(parsedTag.username, parsedTag.name);
            // There isn't a query to get a specific endpoint so just filter for now
            var endpoints = await List(predictorTag);
            var endpoint = endpoints?.FirstOrDefault(endpoint => endpoint.variant == variant);
            // Return
            return endpoint;
        }

        /// <summary>
        /// List all predictor endpoints.
        /// </summary>
        /// <param name="tag">Predictor tag. This MUST NOT be a variant tag.</param>
        /// <returns>Predictor endpoints.</returns>
        public async Task<Endpoint[]?> List (string tag) {
            var predictor = await client.Query<Predictor>(
                @$"query ($input: PredictorInput!) {{
                    predictor (input: $input) {{
                        endpoints {{
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
            return predictor?.endpoints;
        }

        /// <summary>
        /// Create a predicor endpoint.
        /// </summary>
        /// <param name="tag">Endpoint tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <param name="notebook">Notebook URL.</param>
        /// <param name="type">Endpoint type.</param>
        /// <param name="acceleration">Endpoint acceleration.</param>
        /// <returns>Created endpoint.</returns>
        public Task<Endpoint> Create (
            string tag,
            string notebook,
            EndpointType type,
            EndpointAcceleration acceleration
        ) => client.Query<Endpoint>(
            @$"mutation ($input: CreateEndpointInput!) {{
                createEndpoint (input: $input) {{
                    {Fields}
                }}
            }}",
            @"createEndpoint",
            new () {
                ["input"] = new CreateEndpointInput {
                    tag = tag,
                    notebook = notebook,
                    type = type,
                    acceleration = acceleration
                }
            }
        );

        /// <summary>
        /// Delete a predictor endpoint.
        /// </summary>
        /// <param name="tag">Endpoint tag. If the tag does not contain a variant then the variant defaults to `main`.</param>
        /// <returns>Whether the endpoint was successfully deleted.</returns>
        public Task<bool> Delete (string tag) => client.Query<bool>(
            @$"mutation ($input: DeleteEndpointInput!) {{
                deleteEndpoint (input: $input)
            }}",
            @"deleteEndpoint",
            new () {
                ["input"] = new PredictorInput {
                    tag  = tag
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        private const string Fields = @"
            variant
            url
            type
            acceleration
            status
            created
            signature {
                inputs {
                    name
                    type
                    description
                    range
                    optional
                    stringDefault
                    floatDefault
                    intDefault
                    boolDefault
                }
                outputs {
                    name
                    type
                    description
                    range
                    optional
                    stringDefault
                    floatDefault
                    intDefault
                    boolDefault
                }
            }
            error
        ";

        internal EndpointService (IGraphClient client) => this.client = client;
        #endregion


        #region --Input Types--

        internal sealed class CreateEndpointInput {
            public string tag;
            public string notebook;
            [JsonConverter(typeof(API.Graph.GraphEnum<EndpointType>))]
            public EndpointType type;
            [JsonConverter(typeof(API.Graph.GraphEnum<EndpointAcceleration>))]
            public EndpointAcceleration acceleration;
        }
        #endregion
    }
}