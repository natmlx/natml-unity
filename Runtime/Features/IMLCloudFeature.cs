/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {
    
    /// <summary>
    /// ML feature which can create cloud features.
    /// </summary>
    public interface IMLCloudFeature {

        /// <summary>
        /// Create a Ccoud ML feature that is ready for prediction with cloud ML models.
        /// </summary>
        /// <param name="featureType">Feature type used to create the cloud ML feature.</param>
        /// <returns>Cloud ML feature.</returns>
        MLCloudFeature Create (MLFeatureType featureType);
    }
}