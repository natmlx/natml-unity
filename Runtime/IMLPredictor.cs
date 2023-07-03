/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;

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
}