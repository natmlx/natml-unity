/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using UnityEngine;
    using Features;

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

        public static implicit operator MLFeature (float value) => new MLArrayFeature<float>(new [] { value }, new int[0]);

        public static implicit operator MLFeature (int value) => new MLArrayFeature<int>(new [] { value }, new int[0]);

        public static implicit operator MLFeature (bool value) => new MLArrayFeature<bool>(new [] { value }, new int[0]);

        public static implicit operator MLFeature (float[] array) => new MLArrayFeature<float>(array, new int[array.Length]);

        public static implicit operator MLFeature (int[] array) => new MLArrayFeature<int>(array, new [] { array.Length });

        public static implicit operator MLFeature (Texture2D texture) => new MLImageFeature(texture);

        public static implicit operator MLFeature (WebCamTexture texture) => new MLImageFeature(texture.GetPixels32(), texture.width, texture.height);

        public static implicit operator MLFeature (AudioClip clip) => new MLAudioFeature(clip);

        public static implicit operator MLFeature (string text) => new MLStringFeature(text);
        #endregion
    }
}