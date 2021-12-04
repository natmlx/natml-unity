/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;

    /// <summary>
    /// Feature used for making Hub predictions.
    /// </summary>
    [Serializable]
    public sealed class MLHubFeature {
        /// <summary>
        /// Feature type.
        /// Must be one of the `NMLHubDataType` constants.
        /// </summary>
        public string type;
        /// <summary>
        /// Feature data.
        /// This is either base64-encoded feature data or a URL to such data.
        /// </summary>
        public string data;
        /// <summary>
        /// Feature shape.
        /// This should only be populated for array features.
        /// </summary>
        public int[] shape;
    }
}