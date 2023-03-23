/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using API;
    using API.Graph;
    using API.Services;
    using API.Types;
    using Features;
    using Internal;

    /// <summary>
    /// ML model that makes predictions with a predictor endpoint.
    /// </summary>
    public sealed class MLCloudModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Make a prediction on one or more cloud ML features.
        /// Input and output features MUST be disposed when no longer needed.
        /// </summary>
        /// <param name="inputs">Input cloud ML features.</param>
        /// <returns>Output cloud ML features.</returns>
        public async Task<MLFeatureCollection<MLCloudFeature>> Predict (params MLCloudFeature[] inputs) {
            var featureUploads = endpoint.signature.inputs.Zip(inputs, (parameter, feature) => CreateInputFeature(parameter.name, feature));
            var features = await Task.WhenAll(featureUploads);
            var results = await Predict(features);
            return results;
        }

        /// <summary>
        /// Make a prediction on one or more cloud ML features.
        /// Input and output features MUST be disposed when no longer needed.
        /// </summary>
        /// <param name="inputs">Input cloud ML features.</param>
        /// <returns>Output cloud ML features.</returns>
        public async Task<MLFeatureCollection<MLCloudFeature>> Predict (IReadOnlyDictionary<string, MLCloudFeature> inputs) {
            var featureUploads = inputs.Select(pair => CreateInputFeature(pair.Key, pair.Value));
            var features = await Task.WhenAll(featureUploads);
            var results = await Predict(features);
            return results;
        }

        /// <summary>
        /// Create a cloud ML model.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key.</param>
        public static async Task<MLCloudModel> Create (string tag, string? accessKey = null) {
            // Fetch endpoint
            var client = MLUnityExtensions.CreateClient(accessKey);
            var endpoint = await client.Endpoints.Retrieve(tag);
            if (endpoint == null)
                throw new InvalidOperationException($"Cannot create cloud model because endpoint tag '{tag}' is invalid");
            // Check signature
            if (endpoint.signature == null)
                throw new InvalidOperationException(@"Cannot create cloud model because endpoint does not have valid signature");
            // Create model
            var model = new MLCloudModel(client, tag, endpoint);
            return model;
        }
        #endregion


        #region --Operations--
        private readonly NatMLClient client;
        private readonly string tag;
        private readonly Endpoint endpoint;

        private MLCloudModel (NatMLClient client, string tag, Endpoint endpoint) {
            this.client = client;
            this.tag = tag;
            this.endpoint = endpoint;
        }

        private async Task<MLFeatureCollection<MLCloudFeature>> Predict (FeatureInput[] inputs) {
            // Predict
            var options = new PredictionOptions { rawOutputs = true };
            var session = await client.PredictionSessions.Create(tag, options, inputs);
            // Check for errors
            if (!string.IsNullOrEmpty(session.error))
                throw new InvalidOperationException(session.error);
            // Return
            var results = await Task.WhenAll(session.results.Select(CreateFeature));
            return results;
        }

        /// <summary>
        /// Create an endpoint prediction input feature from a cloud feature.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <param name="feature">Cloud feature.</param>
        private async Task<FeatureInput> CreateInputFeature (string name, MLCloudFeature feature) => new FeatureInput {
            name = name,
            data = await client.Storage.Upload(name, feature.data, UploadType.Feature),
            type = feature.type,
            shape = feature.shape
        };

        /// <summary>
        /// Create a cloud feature from an endpoint prediction output feature.
        /// This will download the output feature data into memory.
        /// </summary>
        /// <param name="feature">Prediction output feature.</param>
        private async Task<MLCloudFeature> CreateFeature (Feature feature) => new MLCloudFeature(
            await client.Storage.Download(feature.data),
            feature.type,
            feature.shape
        );
        #endregion
    }
}