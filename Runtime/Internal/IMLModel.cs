/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ML model capable of making predictions on features.
    /// All implementations must provide metadata, input types, output types, and prediction.
    /// Models are NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public interface IMLModel : IDisposable, IReadOnlyDictionary<string, string> {

        /// <summary>
        /// Model input feature types.
        /// </summary>
        MLFeatureType[] inputs { get; }

        /// <summary>
        /// Model output feature types.
        /// </summary>
        MLFeatureType[] outputs { get; }

        /// <summary>
        /// Make a prediction on one or more native input features.
        /// Input and output features MUST be released when no longer needed.
        /// </summary>
        /// <param name="inputs">Native input features.</param>
        /// <returns>Native output features.</returns>
        IntPtr[] Predict (params IntPtr[] inputs);
    }
}