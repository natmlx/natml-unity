/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;

    /// <summary>
    /// ML feature which can create prediction-ready edge features.
    /// </summary>
    public interface IMLEdgeFeature {

        /// <summary>
        /// Create a native feature that is ready for prediction with ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create native feature.</param>
        /// <returns>Prediction-ready native feature.</returns>
        IntPtr Create (in MLFeatureType featureType);
    }
    
    [Obsolete(@"Deprecated in NatML 1.0.6. Use `IMLEdgeFeature` instead.", false)]
    public interface IMLFeature : IMLEdgeFeature { }
}