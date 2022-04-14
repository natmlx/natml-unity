/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Internal {
    
    /// <summary>
    /// ML feature which can create Cloud features.
    /// </summary>
    public interface IMLCloudFeature {

        /// <summary>
        /// Create a Cloud ML feature that is ready for prediction with Cloud ML models.
        /// </summary>
        /// <param name="featureType">Optional feature type used to create the Cloud ML feature.</param>
        /// <returns>Cloud ML feature.</returns>
        MLCloudFeature Create (in MLFeatureType featureType = default);
    }
}