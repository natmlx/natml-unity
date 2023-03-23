/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/


namespace NatML.API.Types {

    /// <summary>
    /// Endpoint prediction options.
    /// </summary>
    public sealed class PredictionOptions {

        /// <summary>
        /// Skip creating convenience fields in output features.
        /// </summary>
        public bool rawOutputs = false;
    }
}