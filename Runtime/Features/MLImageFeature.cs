/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using UnityEngine;
    using Internal;
    using Types;

    /// <summary>
    /// ML image feature.
    /// This feature will perform any necessary conversions to a model's desired input feature type.
    /// Pixel buffers used to create image features MUST have an RGBA8888 pixel layout.
    /// </summary>
    public sealed class MLImageFeature : MLFeature, IMLFeature {

        #region --Client API--
        /// <summary>
        /// Image aspect mode for scaling image features for prediction.
        /// </summary>
        public enum AspectMode : int { // CHECK // Must match `NatML.h`
            /// <summary>
            /// Image will be scaled to fit the required size.
            /// This scale mode DOES NOT preserve the aspect ratio of the image.
            /// </summary>
            ScaleToFit = 0,
            /// <summary>
            /// Image will be aspect-filled to the required size.
            /// </summary>
            AspectFill = 1,
            /// <summary>
            /// Image will be aspect-fit (letterboxed) to the required size.
            /// </summary>
            AspectFit = 2
        }
        
        /// <summary>
        /// Color conversion mode for prediction.
        /// </summary>
        internal enum ColorMode : int { // INCOMPLETE // CHECK // Must match `NatML.h`
            /// <summary>
            /// RGB color.
            /// The output shape must have 3 channels.
            /// </summary>
            RGB = 0,
            /// <summary>
            /// RGBA color.
            /// The output shape must have 4 channels.
            /// </summary>
            RGBA = 0,
            /// <summary>
            /// BGR color.
            /// The output shape must have 3 channels.
            /// </summary>
            BGR = 0,
            /// <summary>
            /// BGRA color.
            /// The output shape must have 4 channels.
            /// </summary>
            BGRA = 0,
            /// <summary>
            /// Greyscale.
            /// The output shape must have 1 channel.
            /// </summary>
            Grey = 0
        }

        /// <summary>
        /// Normalization mean.
        /// </summary>
        public Vector4 mean = Vector3.zero;

        /// <summary>
        /// Normalization standard deviation.
        /// </summary>
        public Vector4 std = Vector3.one;

        /// <summary>
        /// Aspect mode.
        /// </summary>
        public AspectMode aspectMode = 0;

        /// <summary>
        /// Create an image feature.
        /// </summary>
        /// <param name="texture"></param>
        public MLImageFeature (Texture2D texture) : this(texture.GetPixels32(), texture.width, texture.height) { }

        /// <summary>
        /// Create an image feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to create image feature from.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (Color32[] pixelBuffer, int width, int height) : base(new MLImageType(width, height)) => this.colorBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer to create image feature from. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (byte[] pixelBuffer, int width, int height) : base(new MLImageType(width, height)) => this.pixelBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public unsafe MLImageFeature (void* nativeBuffer, int width, int height) : base(new MLImageType(width, height)) => this.nativeBuffer = (IntPtr)nativeBuffer;
        #endregion


        #region --Operations--
        private readonly byte[] pixelBuffer;
        private readonly Color32[] colorBuffer;
        private readonly IntPtr nativeBuffer;

        unsafe IntPtr IMLFeature.Create (in MLFeatureType type) {
            if (pixelBuffer != null)
                fixed (void* data = pixelBuffer)
                    return Create(data, type);
            if (colorBuffer != null)
                fixed (void* data = colorBuffer)
                    return Create(data, type);
            if (nativeBuffer != IntPtr.Zero)
                return Create((void*)nativeBuffer, type);
            return IntPtr.Zero;
        }

        private unsafe IntPtr Create (void* data, MLFeatureType type) {
            var featureType = type as MLArrayType;
            var bufferType = this.type as MLImageType;
            NatML.CreateFeature(
                data,
                bufferType.width,
                bufferType.height,
                featureType.shape,
                featureType.dataType.NativeType(),
                new [] { mean.x, mean.y, mean.z },
                new [] { std.x, std.y, std.z },
                (NMLFeatureFlags)aspectMode,
                out var feature
            );
            return feature;
        }
        #endregion
    }
}