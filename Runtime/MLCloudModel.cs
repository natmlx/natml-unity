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
            /// Create feature inputs
            var featureUploads = endpoint.signature.inputs.Zip(inputs, async (parameter, feature) => new FeatureInput {
                name = parameter.name,
                data = await client.Storage.Upload(parameter.name, feature.data, UploadType.Feature),
                type = feature.type,
                shape = feature.shape
            });
            var features = await Task.WhenAll(featureUploads);
            // Predict
            var session = await client.PredictionSessions.Create(tag, features, options);
            // Check for errors
            if (!string.IsNullOrEmpty(session.error))
                throw new InvalidOperationException(session.error);
            // Download output features
            var results = await Task.WhenAll(session.results.Select(async feature => new MLCloudFeature(
                await client.Storage.Download(feature.data),
                feature.type,
                feature.shape
            )));
            // Return
            return results;
        }

        /// <summary>
        /// Create a cloud ML model.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key.</param>
        public static Task<MLCloudModel> Create (
            string tag,
            string? accessKey = null
        ) => Create(tag, MLUnityExtensions.CreateClient(accessKey));

        /// <summary>
        /// Create a cloud ML model.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="client">NatML access key.</param>
        public static async Task<MLCloudModel> Create (string tag, NatMLClient client) {
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
        private static readonly PredictionOptions options = new () { rawOutputs = true };

        private MLCloudModel (NatMLClient client, string tag, Endpoint endpoint) {
            this.client = client;
            this.tag = tag;
            this.endpoint = endpoint;
        }
        #endregion
    }
}