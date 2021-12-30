/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;

    /// <summary>
    /// ML feature which can create prediction-ready Edge features.
    /// </summary>
    public interface IMLEdgeFeature {

        /// <summary>
        /// Create an Edge feature that is ready for prediction with Edge ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create Edge feature.</param>
        /// <returns>Prediction-ready Edge feature.</returns>
        IntPtr Create (in MLFeatureType featureType);
    }
    
    [Obsolete(@"Deprecated in NatML 1.0.6. Use `IMLEdgeFeature` instead.", false)]
    public interface IMLFeature : IMLEdgeFeature { }
}