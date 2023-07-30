/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using API.Graph;

    /// <summary>
    /// Predictor graph.
    /// </summary>
    [Preserve, Serializable]
    public sealed class Graph {

        /// <summary>
        /// Graph variant.
        /// </summary>
        public string variant;

        /// <summary>
        /// Graph format.
        /// </summary>
        public GraphFormat format;

        /// <summary>
        /// Graph status.
        /// </summary>
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GraphFormat {
        /// <summary>
        /// Apple CoreML.
        /// </summary>
        [EnumMember(Value = @"COREML")]
        CoreML  = 0,
        /// <summary>
        /// Open Neural Network Exchange.
        /// </summary>
        [EnumMember(Value = @"ONNX")]
        ONNX    = 1,
        /// <summary>
        /// TensorFlow Lite.
        /// </summary>
        [EnumMember(Value = @"TFLITE")]
        TFLite  = 2,
    }

    /// <summary>
    /// Graph status.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GraphStatus {
        /// <summary>
        /// Graph is being provisioned.
        /// </summary>
        [EnumMember(Value = @"PROVISIONING")]
        Provisioning = 0,
        /// <summary>
        /// Graph is active.
        /// </summary>
        [EnumMember(Value = @"ACTIVE")]
        Active = 1,
        /// <summary>
        /// Graph is invalid.
        /// </summary>
        [EnumMember(Value = @"INVALID")]
        Invalid = 2,
    }
}