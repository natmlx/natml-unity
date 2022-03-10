/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Internal {

    /// <summary>
    /// ML feature which can create prediction-ready Edge features.
    /// </summary>
    public interface IMLEdgeFeature {

        /// <summary>
        /// Create an Edge feature that is ready for prediction with Edge ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create Edge feature.</param>
        /// <returns>Prediction-ready Edge feature.</returns>
        MLEdgeFeature Create (in MLFeatureType featureType);
    }
}