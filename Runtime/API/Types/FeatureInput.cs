/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Types {

    using Newtonsoft.Json;

    /// <summary>
    /// Prediction feature input.
    /// </summary>
    public sealed class FeatureInput {

        /// <summary>
        /// Feature name.
        /// This MUST match the input parameter name defined by the predictor endpoint.
        /// </summary>
        public string name;

        /// <summary>
        /// Feature data URL.
        /// </summary>
        public string? data;

        /// <summary>
        /// Feature data type.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<Dtype>))]
        public Dtype? type;

        /// <summary>
        /// Feature shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;

        /// <summary>
        /// Feature string value.
        /// </summary>
        public string? stringValue;

        /// <summary>
        /// Feature float value.
        /// </summary>
        public float? floatValue;

        /// <summary>
        /// Feature float array.
        /// The feature will be one-dimensional unless `shape` is specified.
        /// </summary>
        public float[]? floatArray;

        /// <summary>
        /// Feature integer value.
        /// </summary>
        public int? intValue;

        /// <summary>
        /// Feature integer array.
        /// The feature will be one-dimensional unless `shape` is specified.
        /// </summary>
        public int[]? intArray;

        /// <summary>
        /// Feature boolean value.
        /// </summary>
        public bool? boolValue;
    }
}