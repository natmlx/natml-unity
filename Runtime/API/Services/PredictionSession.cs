/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Services {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
    using Types;

    /// <summary>
    /// Create endpoint prediction sessions.
    /// </summary>
    public sealed class PredictionSessionService {

        #region --Client API--
        /// <summary>
        /// Create an endpoint prediction session.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="inputs">Input features.</param>
        public Task<PredictionSession> Create (
            string tag,
            params FeatureInput[] inputs
        ) => Create(tag, inputs, null);

        /// <summary>
        /// Create an endpoint prediction session.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="inputs">Input features.</param>
        /// <param name="options">Prediction options.</param>
        public Task<PredictionSession> Create (
            string tag,
            Dictionary<string, object> inputs,
            PredictionOptions? options = null
        ) => Create(tag, ToFeatures(inputs), options);

        /// <summary>
        /// Create an endpoint prediction session.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="options">Prediction options.</param>
        /// <param name="inputs">Input features.</param>
        public Task<PredictionSession> Create (
            string tag,
            FeatureInput[] inputs,
            PredictionOptions? options
        ) => client.Query<PredictionSession>(
            @$"mutation ($input: CreatePredictionSessionInput!) {{
                createPredictionSession (input: $input) {{
                    id
                    created
                    results {{
                        data
                        type
                        shape
                        {((options?.rawOutputs ?? false) ? string.Empty : ExtraFields)}
                    }}
                    latency
                    error
                    logs
                }}
            }}",
            @"createPredictionSession",
            new () {
                ["input"] = new CreatePredictionSessionInput {
                    tag = tag,
                    inputs = inputs,
                    client = @"dotnet"
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        private const string ExtraFields = @"
            stringValue
            floatValue
            floatArray
            intValue
            intArray
            boolValue
            boolArray
            listValue
            dictValue
        ";

        internal PredictionSessionService (IGraphClient client) => this.client = client;

        private static FeatureInput[] ToFeatures (Dictionary<string, object> inputs) => inputs
            .Select(pair => ToFeature(pair.Key, pair.Value))
            .ToArray();

        private static FeatureInput ToFeature (string name, object value) => value switch {
            string x        => new FeatureInput { name = name, stringValue = x },
            float x         => new FeatureInput { name = name, floatValue = x },
            float[] x       => new FeatureInput { name = name, floatArray = x },
            int x           => new FeatureInput { name = name, intValue = x },
            int[] x         => new FeatureInput { name = name, intArray = x },
            bool x          => new FeatureInput { name = name, boolValue = x },
            FeatureInput x  => x,
            _               => throw new InvalidOperationException(@"Cannot automatically serialize input feature of type {typeof(value)}"),
        };
        #endregion
    }

    #region --Input Types--

    internal sealed class CreatePredictionSessionInput {
        public string tag;
        public FeatureInput[] inputs;
        public string? client;
    }
    #endregion
}