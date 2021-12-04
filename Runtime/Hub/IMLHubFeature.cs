/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {
    
    /// <summary>
    /// ML feature which can create prediction-ready NatML Hub features.
    /// </summary>
    public interface IMLHubFeature {

        /// <summary>
        /// Serialize the Hub feature for prediction with NatML Hub.
        /// </summary>
        /// <returns>Prediction-ready NatML Hub feature.</returns>
        MLHubFeature Serialize ();
    }
}