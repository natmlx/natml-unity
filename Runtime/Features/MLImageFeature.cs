/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Hub;
    using Internal;
    using Types;

    /// <summary>
    /// ML image feature.
    /// This feature will perform any necessary conversions and pre-processing to a model's desired input feature type.
    /// Pixel buffers used to create image features MUST have an RGBA8888 pixel layout.
    /// </summary>
    #pragma warning disable 0618
    public sealed unsafe class MLImageFeature : MLFeature, IMLEdgeFeature, IMLHubFeature, IMLFeature {
    #pragma warning restore 0618

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
        /// Create an image feature from a texture.
        /// </summary>
        /// <param name="texture">Input texture. MUST be readable.</param>
        public MLImageFeature (Texture2D texture) : this(texture.GetPixels32(), texture.width, texture.height) { }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (Color32[] pixelBuffer, int width, int height) : base(new MLImageType(width, height)) => this.colorBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (byte[] pixelBuffer, int width, int height) : base(new MLImageType(width, height)) => this.pixelBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a native buffer.
        /// </summary>
        /// <param name="nativeBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public unsafe MLImageFeature (void* nativeBuffer, int width, int height) : base(new MLImageType(width, height)) => this.nativeBuffer = nativeBuffer;

        /// <summary>
        /// Create an image feature from encoded image data.
        /// </summary>
        /// <param name="encodedImage">Encoded image data.</param>
        public MLImageFeature (byte[] encodedImage) : base(new MLImageType(null, typeof(byte), new [] { 0, 0, 0, 0 })) => this.encodedBuffer = encodedImage;

        /// <summary>
        /// Convert the image feature to a texture.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <returns>Result texture.</returns>
        public Texture2D ToTexture () {
            Texture2D result = null;
            // Load encoded
            if (encodedBuffer != null) {
                result = new Texture2D(16, 16);
                ImageConversion.LoadImage(result, encodedBuffer, false);
            }
            // Load raw
            else {
                var imageType = this.type as MLImageType;
                result = new Texture2D(imageType.width, imageType.height, TextureFormat.RGBA32, false);
                if (pixelBuffer != null)
                    result.GetRawTextureData<byte>().CopyFrom(pixelBuffer);
                else if (colorBuffer != null)
                    result.GetRawTextureData<Color32>().CopyFrom(colorBuffer);
                else
                    result.LoadRawTextureData((IntPtr)nativeBuffer, imageType.width * imageType.height * 4);
                result.Apply();
            }
            return result;
        }
        #endregion


        #region --Operations--
        private readonly byte[] pixelBuffer;
        private readonly Color32[] colorBuffer;
        private readonly void* nativeBuffer;
        private readonly byte[] encodedBuffer;

        unsafe IntPtr IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check
            if (encodedBuffer != null)
                throw new NotImplementedException(@"Cannot create Edge prediction feature from encoded image feature");
            // Create
            fixed (void* pixelData = pixelBuffer)
                fixed (void* colorData = colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    var featureType = type as MLArrayType;
                    var imageType = this.type as MLImageType;
                    NatML.CreateFeature(
                        data,
                        imageType.width,
                        imageType.height,
                        featureType.shape,
                        EdgeType(featureType.dataType),
                        new [] { mean.x, mean.y, mean.z },
                        new [] { std.x, std.y, std.z },
                        (EdgeFeatureFlags)aspectMode,
                        out var feature
                    );
                    return feature;
                }
        }

        unsafe MLHubFeature IMLHubFeature.Serialize () {
            // Shortcut
            if (encodedBuffer != null)
                return new MLHubFeature {
                    data = Convert.ToBase64String(encodedBuffer),
                    type = HubDataType.Image
                };
            // Encode
            fixed (void* pixelData = pixelBuffer)
                fixed (void* colorData = colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    var imageType = this.type as MLImageType;
                    var pixelArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                        data,
                        imageType.width * imageType.height * 4,
                        Allocator.None
                    );
                    var buffer = ImageConversion.EncodeNativeArrayToJPG(
                        pixelArray,
                        GraphicsFormat.R8G8B8A8_UNorm,
                        (uint)imageType.width,
                        (uint)imageType.height,
                        quality: 80
                    ).ToArray();
                    return new MLHubFeature {
                        data = Convert.ToBase64String(buffer),
                        type = HubDataType.Image
                    };
                }
        }
        #endregion
    }
}