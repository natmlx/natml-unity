/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    /// <summary>
    /// Endpoint prediction.
    /// </summary>
    public sealed class EndpointPrediction {

        /// <summary>
        /// Session ID.
        /// </summary>
        public string id;

        /// <summary>
        /// Endpoint tag.
        /// </summary>
        public string tag;

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// Prediction results.
        /// </summary>
        public Feature[]? results;

        /// <summary>
        /// Prediction latency in milliseconds.
        /// </summary>
        public float? latency;

        /// <summary>
        /// Prediction error.
        /// This is `null` if the prediction completed successfully.
        /// </summary>
        public string? error;

        /// <summary>
        /// Prediction logs.
        /// </summary>
        public string? logs;
    }
}