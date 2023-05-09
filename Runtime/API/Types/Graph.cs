/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using Newtonsoft.Json;

    /// <summary>
    /// Predictor graph.
    /// </summary>
    public sealed class Graph {

        /// <summary>
        /// Graph variant.
        /// </summary>
        public string variant;

        /// <summary>
        /// Graph format.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<GraphFormat>))]
        public GraphFormat format;

        /// <summary>
        /// Graph status.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<GraphStatus>))]
        public GraphStatus status;

        /// <summary>
        /// Whether the graph is encrypted.
        /// </summary>
        public bool encrypted;

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// Graph provisioning error.
        /// This is populated when the graph is invalid.
        /// </summary>
        public string? error;
    }

    /// <summary>
    /// Graph format.
    /// </summary>
    public enum GraphFormat {
        /// <summary>
        /// Apple CoreML.
        /// </summary>
        CoreML  = 0,
        /// <summary>
        /// Open Neural Network Exchange.
        /// </summary>
        ONNX    = 1,
        /// <summary>
        /// TensorFlow Lite.
        /// </summary>
        TFLite  = 2,
    }

    /// <summary>
    /// Graph status.
    /// </summary>
    public enum GraphStatus {
        /// <summary>
        /// Graph is being provisioned.
        /// </summary>
        Provisioning = 0,
        /// <summary>
        /// Graph is active.
        /// </summary>
        Active = 1,
        /// <summary>
        /// Graph is invalid.
        /// </summary>
        Invalid = 2,
    }
}