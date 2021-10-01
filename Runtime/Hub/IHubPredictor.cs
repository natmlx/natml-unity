/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub {

    /// <summary>
    /// Hub ML predictor.
    /// The predictor reports prediction analytics data to NatML Hub.
    /// </summary>
    public interface IHubPredictor { // CHECK

        /// <summary>
        /// Event raised when the model completes a prediction.
        /// </summary>
        event MLPredictionHandler Prediction;
    }

    /// <summary>
    /// Prediction handler.
    /// </summary>
    /// <param name="latency">Prediction latency in milliseconds.</param>
    public delegate void MLPredictionHandler (double latency);
}