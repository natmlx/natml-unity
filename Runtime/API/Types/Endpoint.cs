/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using Newtonsoft.Json;
    using GraphEnumOptions = API.Graph.GraphEnumOptions;

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
        [JsonConverter(typeof(API.Graph.GraphEnum<EndpointType>))]
        public EndpointType type;

        /// <summary>
        /// Endpoint acceleration.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<EndpointAcceleration>))]
        public EndpointAcceleration acceleration;

        /// <summary>
        /// Endpoint status.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<EndpointStatus>))]
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
        [JsonConverter(typeof(API.Graph.GraphEnum<Dtype>), GraphEnumOptions.Lowercase)]
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
        Serverless  = 0,
        /// <summary>
        /// Dedicated endpoint.
        /// </summary>
        Dedicated   = 1
    }

    /// <summary>
    /// Endpoint acceleration.
    /// </summary>
    public enum EndpointAcceleration : int {
        /// <summary>
        /// Endpoint is run on the CPU.
        /// </summary>
        CPU = 0,
        /// <summary>
        /// Endpoint is run on an Nvidia A40 GPU.
        /// </summary>
        A40 = 1,
        /// <summary>
        /// Endpoint is run on an Nvidia A100 GPU.
        /// </summary>
        A100 = 2
    }

    /// <summary>
    /// Endpoint status.
    /// </summary>
    public enum EndpointStatus : int {
        /// <summary>
        /// Endpoint is being provisioned.
        /// </summary>
        Provisioning    = 0,
        /// <summary>
        /// Endpoint is active.
        /// </summary>
        Active          = 1,
        /// <summary>
        /// Endpoint is invalid.
        /// </summary>
        Invalid         = 2
    }
}