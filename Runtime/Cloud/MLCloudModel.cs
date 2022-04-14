/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using Hub;
    using Hub.Requests;

    /// <summary>
    /// Server-side ML model capable of making predictions on features.
    /// Cloud models are currently NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLCloudModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Cloud prediction options.
        /// </summary>
        public class PredictionOptions {
            /// <summary>
            /// Perform prediction asynchronously.
            /// This option should be enabled for predictors that take more than 10 seconds to complete a prediction.
            /// See https://docs.natml.ai/graph/workflows/hub#requesting-a-hub-prediction.
            /// </summary>
            public bool asyncPrediction;
        }
        
        /// <summary>
        /// Make a prediction on one or more Cloud ML features.
        /// </summary>
        /// <param name="inputs">Input Cloud ML features.</param>
        /// <returns>Output Cloud ML features.</returns>
        public Task<MLFeatureCollection<MLCloudFeature>> Predict (params MLCloudFeature[] inputs) => Predict(inputs, defaultOptions);

        /// <summary>
        /// Make a prediction on one or more Cloud ML features.
        /// </summary>
        /// <param name="inputs">Input Cloud ML features.</param>
        /// <param name="options">Cloud prediction options.</param>
        /// <returns>Output Cloud ML features.</returns>
        public async Task<MLFeatureCollection<MLCloudFeature>> Predict (MLCloudFeature[] inputs, PredictionOptions options) {
            // Check
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            // Request prediction
            var input = new RequestPredictionRequest.Input {
                session = session,
                inputs = inputs.Select(ConvertFeature).ToArray(),
                waitUntilCompleted = !options.asyncPrediction
            };
            var prediction = await NatMLHub.RequestPrediction(input);
            // Listen
            if (prediction.status != PredictionStatus.Completed)
                prediction = await NatMLHub.WaitForPrediction(prediction.id);
            // Check for errors
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);   
            // Return
            var outputs = await Task.WhenAll(prediction.results.Select(ConvertFeature));
            return outputs;
        }
        #endregion


        #region --Operations--
        private static readonly PredictionOptions defaultOptions = new PredictionOptions {
            asyncPrediction = false
        };

        unsafe internal MLCloudModel (string session) : base(session) {
            this.inputs = null;
            this.outputs = null;
            this.metadata = new Dictionary<string, string>();
        }

        private static Feature ConvertFeature (MLCloudFeature feature) {
            using var stream = new MemoryStream();
            feature.data.CopyTo(stream);
            var data = Convert.ToBase64String(stream.ToArray());
            var mime = GetMime(feature.type);
            return new Feature { data = $"data:{mime};base64,{data}", type = feature.type, shape = feature.shape };
        }

        public static async Task<MLCloudFeature> ConvertFeature (Feature feature) {
            if (feature.data.StartsWith("data:"))
                return ConvertDataFeature(feature);
            else
                return await ConvertRemoteFeature(feature);
        }

        private static MLCloudFeature ConvertDataFeature (Feature feature) {
            var dataIdx = feature.data.LastIndexOf(",") + 1;
            var data = Convert.FromBase64String(feature.data.Substring(dataIdx));
            return new MLCloudFeature { data = new MemoryStream(data, false), type = feature.type, shape = feature.shape };
        }

        private static async Task<MLCloudFeature> ConvertRemoteFeature (Feature feature) {
            using var client = new HttpClient();
            using var dataStream = await client.GetStreamAsync(feature.data);
            var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            return new MLCloudFeature { data = memoryStream, type = feature.type, shape = feature.shape };
        }

        private static string GetMime (string type) => type switch {
            DataType.String  => @"text/plain",
            DataType.Image   => @"image/jpeg",
            DataType.Audio   => @"audio/wav",
            DataType.Video   => @"video/mp4",
            _                   => "application/octet-stream"
        };
        #endregion
    }
}