/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub {

    /// <summary>
    /// Model event reporter.
    /// </summary>
    internal interface INMLReporter {
        
        /// <summary>
        /// Event raised when the model completes a prediction.
        /// </summary>
        event MLPredictionHandler onPrediction;
    }

    /// <summary>
    /// Prediction handler.
    /// </summary>
    /// <param name="latency">Prediction latency in milliseconds.</param>
    delegate void MLPredictionHandler (double latency);
}