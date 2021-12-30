/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML {

    using System;
    using UnityEngine;
    using Features;
    using Hub;
    using Internal;

    /// <summary>
    /// ML feature.
    /// </summary>
    public abstract class MLFeature {

        #region --Client API--
        /// <summary>
        /// Feature type.
        /// </summary>
        public readonly MLFeatureType type;
        #endregion


        #region --Operations--

        protected MLFeature (MLFeatureType type) => this.type = type;

        public static implicit operator MLFeature (float[] array) => new MLArrayFeature<float>(array);

        public static implicit operator MLFeature (Texture2D texture) => new MLImageFeature(texture);

        public static implicit operator MLFeature (WebCamTexture texture) => new MLImageFeature(texture.GetPixels32(), texture.width, texture.height);

        public static implicit operator MLFeature (AudioClip clip) => new MLAudioFeature(clip);

        public static implicit operator MLFeature (string text) => new MLTextFeature(text);

        protected static EdgeDataType EdgeType (Type type) {
            switch (type) {
                case var t when t == typeof(byte):      return EdgeDataType.UInt8;
                case var t when t == typeof(short):     return EdgeDataType.Int16;
                case var t when t == typeof(int):       return EdgeDataType.Int32;
                case var t when t == typeof(long):      return EdgeDataType.Int64;
                case var t when t == typeof(float):     return EdgeDataType.Float;
                case var t when t == typeof(double):    return EdgeDataType.Double;
                default:                                return EdgeDataType.Undefined;
            }
        }

        protected static string HubType (Type type) {
            switch (type) {
                case var t when t == typeof(byte):      return HubDataType.Byte;
                case var t when t == typeof(short):     return HubDataType.Short;
                case var t when t == typeof(int):       return HubDataType.Int;
                case var t when t == typeof(long):      return HubDataType.Long;
                case var t when t == typeof(float):     return HubDataType.Float;
                case var t when t == typeof(double):    return HubDataType.Double;
                default: return null;
            }
        }
        #endregion
    }
}