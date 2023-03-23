/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Types {

    using Newtonsoft.Json;

    /// <summary>
    /// Prediction feature.
    /// </summary>
    public sealed class Feature {

        /// <summary>
        /// Feature data URL.
        /// </summary>
        public string data;

        /// <summary>
        /// Feature data type.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<Dtype>))]
        public Dtype type;

        /// <summary>
        /// Feature shape.
        /// This is `null` if shape information is not available or applicable.
        /// </summary>
        public int[]? shape;

        /// <summary>
        /// Feature data as a `string`.
        /// This is a convenience property and is only populated for `string` features.
        /// </summary>
        public string? stringValue;

        /// <summary>
        /// Feature data as a `float`.
        /// This is a convenience property and is only populated for `float32` or `float64` scalar features.
        /// </summary>
        public float? floatValue;

        /// <summary>
        /// Feature data as a flattened `float` array.
        /// This is a convenience property and is only populated for `float32` tensor features.
        /// </summary>
        public float[]? floatArray;

        /// <summary>
        /// Feature data as an integer.
        /// This is a convenience property and is only populated for integer scalar features.
        /// </summary>
        public int? intValue;

        /// <summary>
        /// Feature data as a flattened `int32` array.
        /// This is a convenience property and is only populated for `int32` tensor features.
        /// </summary>
        public int[]? intArray;

        /// <summary>
        /// Feature data as a boolean.
        /// This is a convenience property and is only populated for `bool` scalar features.
        /// </summary>
        public bool? boolValue;

        /// <summary>
        /// Feature data as a flattened boolean array.
        /// This is a convenience property and is only populated for `bool` tensor features.
        /// </summary>
        public bool[]? boolArray;
    }
}