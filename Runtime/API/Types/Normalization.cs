/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    using System;
    using API.Graph;

    /// <summary>
    /// Feature normalization.
    /// </summary>
    [Preserve, Serializable]
    public sealed class Normalization {
        
        /// <summary>
        /// Mean.
        /// </summary>
        public float[] mean;

        /// <summary>
        /// Standard deviation.
        /// </summary>
        public float[] std;
    }
}