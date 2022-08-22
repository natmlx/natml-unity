/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    /// <summary>
    /// ML feature which can create Edge ML features.
    /// </summary>
    public interface IMLEdgeFeature {

        /// <summary>
        /// Create an Edge ML feature that is ready for prediction with Edge ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create the Edge ML feature.</param>
        /// <returns>Edge ML feature.</returns>
        MLEdgeFeature Create (MLFeatureType featureType);
    }
}