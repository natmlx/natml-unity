/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// Predictor endpoint.
    /// </summary>
    public sealed class Endpoint {

        /// <summary>
        /// Endpoint variant.
        /// </summary>
        public string variant;

        /// <summary>
        /// Endpoint URL.
        /// </summary>
        public string url;

        /// <summary>
        /// Endpoint type.
        /// </summary>
        public EndpointType type;

        /// <summary>
        /// Endpoint acceleration.
        /// </summary>
        public EndpointAcceleration acceleration;

        /// <summary>
        /// Endpoint status.
        /// </summary>
        public EndpointStatus status;

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// Endpoint prediction signature.
        /// This is populated for active endpoints.
        /// </summary>
        public EndpointSignature? signature;

        /// <summary>
        /// Endpoint error.
        /// This is populated for invalid endpoints.
        /// </summary>
        public string? error;
    }

    /// <summary>
    /// Endpoint signature.
    /// </summary>
    public sealed class EndpointSignature {

        /// <summary>
        /// Input parameters.
        /// </summary>
        public EndpointParameter[] inputs;

        /// <summary>
        /// Output parameters.
        /// </summary>
        public EndpointParameter[] outputs;
    }

    /// <summary>
    /// Endpoint parameter.
    /// </summary>
    public sealed class EndpointParameter {

        /// <summary>
        /// Parameter name.
        /// This is only populated for input parameters.
        /// </summary>
        public string? name;

        /// <summary>
        /// Parameter type.
        /// This is `null` if the type is unknown or unsupported by NatML.
        /// </summary>
        public Dtype? type;

        /// <summary>
        /// Parameter description.
        /// </summary>
        public string? description;

        /// <summary>
        /// Whether parameter is optional.
        /// </summary>
        public bool? optional;

        /// <summary>
        /// Parameter value range for numeric parameters.
        /// </summary>
        public float[]? range;

        /// <summary>
        /// Parameter default string value.
        /// </summary>
        public string? stringDefault;

        /// <summary>
        /// Parameter default float value.
        /// </summary>
        public float? floatDefault;

        /// <summary>
        /// Parameter default integer value.
        /// </summary>
        public int? intDefault;

        /// <summary>
        /// Parameter default boolean value.
        /// </summary>
        public bool? boolDefault;
    }

    /// <summary>
    /// Endpoint type.
    /// </summary>
    public enum EndpointType : int {
        /// <summary>
        /// Serverless endpoint.
        /// </summary>
        [EnumMember(Value = @"SERVERLESS")]
        Serverless  = 0,
        /// <summary>
        /// Dedicated endpoint.
        /// </summary>
        [EnumMember(Value = @"DEDICATED")]
        Dedicated   = 1
    }

    /// <summary>
    /// Endpoint acceleration.
    /// </summary>
    public enum EndpointAcceleration : int {
        /// <summary>
        /// Endpoint is run on the CPU.
        /// </summary>
        [EnumMember(Value = @"CPU")]
        CPU = 0,
        /// <summary>
        /// Endpoint is run on an Nvidia A40 GPU.
        /// </summary>
        [EnumMember(Value = @"A40")]
        A40 = 1,
        /// <summary>
        /// Endpoint is run on an Nvidia A100 GPU.
        /// </summary>
        [EnumMember(Value = @"A100")]
        A100 = 2
    }

    /// <summary>
    /// Endpoint status.
    /// </summary>
    public enum EndpointStatus : int {
        /// <summary>
        /// Endpoint is being provisioned.
        /// </summary>
        [EnumMember(Value = @"PROVISIONING")]
        Provisioning    = 0,
        /// <summary>
        /// Endpoint is active.
        /// </summary>
        [EnumMember(Value = @"ACTIVE")]
        Active          = 1,
        /// <summary>
        /// Endpoint is invalid.
        /// </summary>
        [EnumMember(Value = @"INVALID")]
        Invalid         = 2
    }
}