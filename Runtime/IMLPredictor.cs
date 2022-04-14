/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML {

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Lightweight primitive for making predictions with a model.
    /// Predictors transform raw model outputs to types that are easily usable by applications.
    /// </summary>
    public interface IMLPredictor<TOutput> : IDisposable {

        /// <summary>
        /// Make a prediction on one or more input features.
        /// </summary>
        /// <param name="inputs">Input features.</param>
        /// <returns>Prediction output.</returns>
        TOutput Predict (params MLFeature[] inputs);
    }

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