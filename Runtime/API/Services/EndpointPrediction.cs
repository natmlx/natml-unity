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
    /// Create endpoint predictions.
    /// </summary>
    public sealed class EndpointPredictionService {

        #region --Client API--
        /// <summary>
        /// Create an endpoint prediction.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="inputs">Input features.</param>
        public Task<EndpointPrediction> Create (
            string tag,
            params FeatureInput[] inputs
        ) => Create(tag, inputs, false, 0);

        /// <summary>
        /// Create an endpoint prediction.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="inputs">Input features.</param>
        /// <param name="options">Prediction options.</param>
        /// <param name="rawOutputs">Skip creating convenience fields in output features.</param>
        /// <param name="dataUrlLimit">Return a data URL if the output feature is smaller than this limit (in bytes).</param>
        public Task<EndpointPrediction> Create (
            string tag,
            Dictionary<string, object> inputs,
            bool rawOutputs = false,
            int? dataUrlLimit = null
        ) => Create(tag, ToFeatures(inputs), rawOutputs, dataUrlLimit);

        /// <summary>
        /// Create an endpoint prediction.
        /// </summary>
        /// <param name="tag">Endpoint tag.</param>
        /// <param name="options">Prediction options.</param>
        /// <param name="inputs">Input features.</param>
        /// <param name="rawOutputs">Skip creating convenience fields in output features.</param>
        /// <param name="dataUrlLimit">Return a data URL if the output feature is smaller than this limit (in bytes).</param>
        public Task<EndpointPrediction> Create (
            string tag,
            FeatureInput[] inputs,
            bool rawOutputs = false,
            int? dataUrlLimit = null
        ) => client.Query<EndpointPrediction>(
            @$"mutation ($input: CreateEndpointPredictionInput!) {{
                createEndpointPrediction (input: $input) {{
                    id
                    tag
                    created
                    results {{
                        data
                        type
                        shape
                        {(rawOutputs ? string.Empty : ExtraFields)}
                    }}
                    latency
                    error
                    logs
                }}
            }}",
            @"createEndpointPrediction",
            new () {
                ["input"] = new CreateEndpointPredictionInput {
                    tag = tag,
                    inputs = inputs,
                    client = @"dotnet",
                    dataUrlLimit = dataUrlLimit
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

        internal EndpointPredictionService (IGraphClient client) => this.client = client;

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

    #region --Types--

    internal sealed class CreateEndpointPredictionInput {
        public string tag;
        public FeatureInput[] inputs;
        public string? client;
        public int? dataUrlLimit;
    }
    #endregion
}