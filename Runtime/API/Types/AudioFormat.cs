/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    using System;
    using API.Graph;

    /// <summary>
    /// Audio format.
    /// </summary>
    [Preserve, Serializable]
    public sealed class AudioFormat {

        /// <summary>
        /// Sample rate.
        /// </summary>
        public int sampleRate;

        /// <summary>
        /// Channel count.
        /// </summary>
        public int channelCount;
    }
}