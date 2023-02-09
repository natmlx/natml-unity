/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hub;
    using Hub.Internal;
    using Hub.Requests;
    using Hub.Types;
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
            // Check endpoint
            if (endpoint == null)
                throw new InvalidOperationException(@"Endpoint info is missing. Specify input feature names by using `Predict` overload that accepts a dictionary");
            // Check signature
            if (endpoint.signature.inputs.Length == 0)
                throw new InvalidOperationException(@"Endpoint does not have signature. Specify input feature names by using `Predict` overload that accepts a dictionary");
            // Predict
            var features = await Task.WhenAll(inputs.Zip(endpoint.signature.inputs, (feature, parameter) => feature.ToFeatureInput(parameter.name)));
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
            var features = await Task.WhenAll(inputs.Select(pair => pair.Value.ToFeatureInput(pair.Key)));
            var results = await Predict(features);
            return results;
        }

        /// <summary>
        /// Create a cloud ML model.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key.</param>
        public static async Task<MLCloudModel> Create (string tag, string accessKey = null) {
            // Fetch predictor
            var predictor = await NatMLHub.GetPredictor(tag, HubSettings.Instance.AccessKey);
            if (predictor == null)
                throw new InvalidOperationException($"Cannot create cloud model because predictor tag '{tag}' is invalid");
            // Get endpoint
            var variant = new Tag(tag).variant ?? Tag.DefaultVariant;
            var endpoint = predictor.endpoints.FirstOrDefault(endpoint => endpoint.variant == variant && endpoint.status == EndpointStatus.Active);
            // Create model
            accessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : HubSettings.Instance?.AccessKey;
            var model = new MLCloudModel(tag, endpoint, accessKey);
            return model;
        }
        #endregion


        #region --Operations--
        private readonly Tag tag;
        private readonly Endpoint endpoint;
        private readonly string accessKey;

        private MLCloudModel (Tag tag, Endpoint endpoint, string accessKey) {
            this.tag = tag;
            this.endpoint = endpoint;
            this.accessKey = accessKey;
        }

        private async Task<MLFeatureCollection<MLCloudFeature>> Predict (FeatureInput[] inputs) {
            // Predict
            var input = new CreatePredictionSessionRequest.Input { tag = tag, inputs = inputs };
            var session = await NatMLHub.CreatePredictionSession(input, accessKey);
            // Check for errors
            if (!string.IsNullOrEmpty(session.error))
                throw new InvalidOperationException(session.error);
            // Return
            var results = await Task.WhenAll(session.results.Select(MLEndpointUtils.ToFeature));
            return results;
        }
        #endregion
    }
}