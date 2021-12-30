/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hub;
    using Hub.Requests;

    /// <summary>
    /// Server-side ML model capable of making predictions on features.
    /// Hub models are currently NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLHubModel : MLModel { // INCOMPLETE // Download any remote features

        #region --Client API--
        /// <summary>
        /// Make a prediction on one or more Hub features.
        /// </summary>
        /// <param name="inputs">Input Hub features.</param>
        /// <returns>Output Hub features.</returns>
        public async Task<MLHubFeature[]> Predict (params MLHubFeature[] inputs) {
            // Request prediction
            var input = new RequestPredictionRequest.Input {
                session = session,
                inputs = inputs,
                waitUntilCompleted = true
            };
            var prediction = await NatMLHub.RequestPrediction(input);
            // Check for errors
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);
            // Return
            return prediction.results;
        }
        #endregion


        #region --Operations--

        unsafe internal MLHubModel (string session) : base(session) {
            this.inputs = null;
            this.outputs = null;
            this.metadata = new Dictionary<string, string>();
        }
        #endregion
    }
}