/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Hub;
    using Internal;
    using Types;

    /// <summary>
    /// ML image feature.
    /// The image feature will perform any necessary conversions and pre-processing to a model's desired input feature type.
    /// Pixel buffers used to create image features MUST have an RGBA8888 pixel layout.
    /// </summary>
    public sealed unsafe class MLImageFeature : MLFeature, IMLEdgeFeature, IMLHubFeature {

        #region --Types--
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
        #endregion


        #region --Inspection--
        /// <summary>
        /// Image width.
        /// </summary>
        public int width => (type as MLImageType).width;

        /// <summary>
        /// Image height.
        /// </summary>
        public int height => (type as MLImageType).height;
        #endregion


        #region --Preprocessing--
        /// <summary>
        /// Normalization mean.
        /// </summary>
        public Vector4 mean = Vector4.zero;

        /// <summary>
        /// Normalization standard deviation.
        /// </summary>
        public Vector4 std = Vector4.one;

        /// <summary>
        /// Aspect mode.
        /// </summary>
        public AspectMode aspectMode = 0;
        #endregion


        #region --Constructors--
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
        public MLImageFeature (Color32[] pixelBuffer, int width, int height) : base(new MLImageType(width, height, 4)) => this.colorBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (byte[] pixelBuffer, int width, int height) : base(new MLImageType(width, height, 4)) => this.pixelBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (NativeArray<byte> pixelBuffer, int width, int height) : this(pixelBuffer.GetUnsafeReadOnlyPtr(), width, height) { }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public unsafe MLImageFeature (void* pixelBuffer, int width, int height) : base(new MLImageType(width, height, 4)) =>  this.nativeBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a Hub feature.
        /// </summary>
        /// <param name="hubFeature">Hub feature. This MUST be an `IMAGE` feature.</param>
        public MLImageFeature (MLHubFeature hubFeature) : base(new MLImageType(hubFeature.shape, typeof(byte))) {
            // Check
            if (hubFeature.type != HubDataType.Image)
                throw new ArgumentException(@"Hub feature is not an image feature", nameof(hubFeature));
            // Deserialize
            using var stream = new MemoryStream();
            hubFeature.data.CopyTo(stream);
            this.encodedBuffer = stream.ToArray();
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo (byte[] pixelBuffer) {
            fixed (void* buffer = pixelBuffer)
                CopyTo(buffer);
        }

        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo (Color32[] pixelBuffer) {
            fixed (void* buffer = pixelBuffer)
                CopyTo(buffer);
        }

        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo<T> (NativeArray<T> pixelBuffer) where T : unmanaged => CopyTo(pixelBuffer.GetUnsafePtr());

        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo (void* pixelBuffer) {
            // Check
            if (encodedBuffer != null)
                throw new ArgumentException(@"Cannot copy to buffer because image feature contains encoded data");
            // Copy
            fixed (void* pixelData = this.pixelBuffer)
                fixed (void* colorData = this.colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    UnsafeUtility.MemCpy(pixelBuffer, data, width * height * 4);
                }
        }

        /// <summary>
        /// Convert the image feature to a texture.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <param name="result">Optional. Result texture to copy data into.</param>
        /// <returns>Result texture.</returns>
        public unsafe Texture2D ToTexture (Texture2D result = null) {
            // Load encoded
            if (encodedBuffer != null) {
                result = result ?? new Texture2D(16, 16);
                ImageConversion.LoadImage(result, encodedBuffer, false);
                return result;
            }
            // Create texture
            var imageType = this.type as MLImageType;
            result = result ?? new Texture2D(imageType.width, imageType.height, TextureFormat.RGBA32, false);
            // Resize texture
            if (result.width != imageType.width || result.height != imageType.height || result.format != TextureFormat.RGBA32)
                result.Resize(imageType.width, imageType.height, TextureFormat.RGBA32, false);
            // Copy data
            CopyTo(result.GetRawTextureData<byte>().GetUnsafePtr());
            result.Apply();
            // Return
            return result;
        }
        #endregion


        #region --Operations--
        private readonly byte[] pixelBuffer;
        private readonly Color32[] colorBuffer;
        private readonly void* nativeBuffer;
        private readonly byte[] encodedBuffer;

        unsafe MLEdgeFeature IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check
            if (encodedBuffer != null)
                throw new NotImplementedException(@"Cannot create Edge prediction feature from encoded image feature");
            // Create
            fixed (void* pixelData = pixelBuffer)
                fixed (void* colorData = colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    var featureType = type as MLArrayType;
                    NatML.CreateFeature(
                        data,
                        width,
                        height,
                        featureType.shape,
                        EdgeType(featureType.dataType),
                        new [] { mean.x, mean.y, mean.z, mean.w },
                        new [] { std.x, std.y, std.z, std.w },
                        (NatML.FeatureFlags)aspectMode,
                        out var feature
                    );
                    return new MLEdgeFeature(feature);
                }
        }

        unsafe MLHubFeature IMLHubFeature.Serialize () {
            // Shortcut
            if (encodedBuffer != null)
                return new MLHubFeature {
                    data = new MemoryStream(encodedBuffer, false),
                    type = HubDataType.Image
                };
            // Encode
            fixed (void* pixelData = pixelBuffer)
                fixed (void* colorData = colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    var pixelArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                        data,
                        width * height * 4,
                        Allocator.None
                    );
                    using var buffer = ImageConversion.EncodeNativeArrayToJPG(
                        pixelArray,
                        GraphicsFormat.R8G8B8A8_UNorm,
                        (uint)width,
                        (uint)height,
                        quality: 80
                    );
                    return new MLHubFeature {
                        data = new MemoryStream(buffer.ToArray(), false),
                        type = HubDataType.Image
                    };
                }
        }
        #endregion
    }
}