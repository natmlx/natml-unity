/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Lightweight primitive for making predictions asynchronously.
    /// Async predictors can be used to make server-side ML predictions that require heavier processing.
    /// </summary>
    public interface IMLAsyncPredictor<TOutput> : IDisposable {

        /// <summary>
        /// Make a prediction on one or more input features.
        /// </summary>
        /// <param name="inputs">Input features.</param>
        /// <returns>Prediction output.</returns>
        Task<TOutput> Predict (params MLFeature[] inputs);
    }
}