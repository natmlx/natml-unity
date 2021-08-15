/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML {

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

        public static implicit operator MLFeature (float[] array) => new MLArrayFeature<float>(array);

        public static implicit operator MLFeature (Texture2D texture) => new MLImageFeature(texture);

        public static implicit operator MLFeature (WebCamTexture texture) => new MLImageFeature(texture.GetPixels32(), texture.width, texture.height);

        public static implicit operator MLFeature (AudioClip clip) => new MLAudioFeature(clip);

        public static implicit operator MLFeature (string text) => new MLTextFeature(text);
        #endregion
    }
}