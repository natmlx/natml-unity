/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    /// <summary>
    /// ML feature which can create edge ML features.
    /// </summary>
    public interface IMLEdgeFeature {

        /// <summary>
        /// Create an edge ML feature that is ready for prediction with edge ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create the edge ML feature.</param>
        /// <returns>Edge ML feature.</returns>
        MLEdgeFeature Create (MLFeatureType featureType);
    }
}