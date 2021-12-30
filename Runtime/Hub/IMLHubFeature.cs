/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {
    
    /// <summary>
    /// ML feature which can create prediction-ready Hub features.
    /// </summary>
    public interface IMLHubFeature {

        /// <summary>
        /// Serialize the Hub feature for prediction with Hub ML models.
        /// </summary>
        /// <returns>Prediction-ready Hub feature.</returns>
        MLHubFeature Serialize ();
    }
}