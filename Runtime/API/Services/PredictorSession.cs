/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Services {

    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
    using Types;

    /// <summary>
    /// Create graph prediction sessions.
    /// </summary>
    [Obsolete(@"Deprecated in NatML 1.1.6")]
    public sealed class PredictorSessionService {

        #region --Client API--
        /// <summary>
        /// Create a graph prediction session.
        /// </summary>
        /// <param name="tag">Graph tag.</param>
        /// <param name="format">Graph format.</param>
        public Task<PredictorSession> Create (
            string tag,
            GraphFormat format,
            string? secret = null,
            string? device = null
        ) => client.Query<PredictorSession>(
            @$"mutation ($input: CreatePredictorSessionInput!) {{
                createPredictorSession (input: $input) {{
                    id
                    predictor {{
                        tag
                        owner {{
                            username
                        }}
                        name
                        description
                        status
                        access
                        license
                        labels
                        normalization {{
                            mean
                            std
                        }}
                        aspectMode
                        audioFormat {{
                            sampleRate
                            channelCount
                        }}
                    }}
                    graph
                    format
                    flags
                    fingerprint
                    created
                    secret
                }}
            }}
            ",
            "createPredictorSession",
            new () {
                ["input"] = new CreatePredictorSessionInput {
                    tag = tag,
                    format = format,
                    secret = secret,
                    client = @"dotnet",
                    device = device
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;

        internal PredictorSessionService (IGraphClient client) => this.client = client;
        #endregion
    }

    #region --Types--

    internal sealed class CreatePredictorSessionInput {
        public string tag;
        public GraphFormat format;
        public string? secret;
        public string? client;
        public string? device;
    }
    #endregion
}