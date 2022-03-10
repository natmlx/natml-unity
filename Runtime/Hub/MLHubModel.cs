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
    /// Hub models are currently NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLHubModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Hub prediction options.
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
        /// Make a prediction on one or more Hub features.
        /// </summary>
        /// <param name="inputs">Input Hub features.</param>
        /// <returns>Output Hub features.</returns>
        public Task<MLFeatureCollection<MLHubFeature>> Predict (params MLHubFeature[] inputs) => Predict(inputs, defaultOptions);

        /// <summary>
        /// Make a prediction on one or more Hub features.
        /// </summary>
        /// <param name="inputs">Input Hub features.</param>
        /// <param name="options">Hub prediction options.</param>
        /// <returns>Output Hub features.</returns>
        public async Task<MLFeatureCollection<MLHubFeature>> Predict (MLHubFeature[] inputs, PredictionOptions options) {
            // Check
            if (options == null)
                throw new ArgumentException(@"Prediction options cannot be `null`", nameof(options));
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

        unsafe internal MLHubModel (string session) : base(session) {
            this.inputs = null;
            this.outputs = null;
            this.metadata = new Dictionary<string, string>();
        }

        private static Feature ConvertFeature (MLHubFeature feature) {
            using var stream = new MemoryStream();
            feature.data.CopyTo(stream);
            var data = Convert.ToBase64String(stream.ToArray());
            var mime = GetMime(feature.type);
            return new Feature { data = $"data:{mime};base64,{data}", type = feature.type, shape = feature.shape };
        }

        public static async Task<MLHubFeature> ConvertFeature (Feature feature) {
            if (feature.data.StartsWith("data:"))
                return ConvertDataFeature(feature);
            else
                return await ConvertRemoteFeature(feature);
        }

        private static MLHubFeature ConvertDataFeature (Feature feature) {
            var dataIdx = feature.data.LastIndexOf(",") + 1;
            var data = Convert.FromBase64String(feature.data.Substring(dataIdx));
            return new MLHubFeature { data = new MemoryStream(data, false), type = feature.type, shape = feature.shape };
        }

        private static async Task<MLHubFeature> ConvertRemoteFeature (Feature feature) {
            using var client = new HttpClient();
            using var dataStream = await client.GetStreamAsync(feature.data);
            var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            return new MLHubFeature { data = memoryStream, type = feature.type, shape = feature.shape };
        }

        private static string GetMime (string type) => type switch {
            HubDataType.String  => @"text/plain",
            HubDataType.Image   => @"image/jpeg",
            HubDataType.Audio   => @"audio/wav",
            HubDataType.Video   => @"video/mp4",
            _                   => "application/octet-stream"
        };
        #endregion
    }
}