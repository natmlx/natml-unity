/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections.Generic;
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
    public unsafe class MLImageFeature : MLFeature, IMLEdgeFeature, IMLCloudFeature {

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
        public MLImageFeature (Color32[] pixelBuffer, int width, int height) {
            this.type = new MLImageType(width, height, 4);
            this.colorBuffer = pixelBuffer;
        }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (byte[] pixelBuffer, int width, int height) {
            this.type = new MLImageType(width, height, 4);
            this.pixelBuffer = pixelBuffer;
        }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// Note that the native array MUST remain valid for the lifetime of the feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (NativeArray<byte> pixelBuffer, int width, int height) : this(pixelBuffer.GetUnsafeReadOnlyPtr(), width, height) { }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// Note that the pixel buffer MUST remain valid for the lifetime of the feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public unsafe MLImageFeature (void* pixelBuffer, int width, int height) {
            this.type = new MLImageType(width, height, 4);
            this.nativeBuffer = pixelBuffer;
        }

        /// <summary>
        /// Create an image feature from an encoded image buffer.
        /// </summary>
        /// <param name="encodedBuffer">Encoded JPEG or PNG image data.</param>
        public MLImageFeature (byte[] encodedBuffer) : base() {
            this.type = new MLImageType(null, typeof(byte));
            this.encodedBuffer = encodedBuffer;
        }

        /// <summary>
        /// Create an image feature from a Cloud ML feature.
        /// </summary>
        /// <param name="feature">Cloud ML feature. This MUST be an `IMAGE` feature.</param>
        public MLImageFeature (MLCloudFeature feature) {
            // Check
            if (feature.type != DataType.Image)
                throw new ArgumentException(@"Cloud feature is not an image feature", nameof(feature));
            // Deserialize
            this.type = new MLImageType(feature.shape, typeof(byte));
            using var stream = new MemoryStream();
            feature.data.CopyTo(stream);
            this.encodedBuffer = stream.ToArray();
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Create an image feature which contains pixel data loaded into contiguous memory.
        /// If the image feature is already loaded, then `this` feature will be returned.
        /// This method will decode an image feature created from encoded image data.
        /// Note that this method MUST be called on the Unity main thread.
        /// </summary>
        /// <returns>Image feature which contains pixel data in contiguous memory.</returns>
        public virtual MLImageFeature Contiguous () {
            // Check
            if (encodedBuffer == null)
                return this;
            // Decode
            var texture = new Texture2D(2, 2);
            texture.LoadImage(encodedBuffer, false);
            return new MLImageFeature(texture);
        }

        /// <summary>
        /// Copy the image data in this feature into a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo<T> (T[] pixelBuffer) where T : unmanaged {
            fixed (void* buffer = pixelBuffer)
                CopyTo(buffer);
        }

        /// <summary>
        /// Copy the image data in this feature into a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public unsafe void CopyTo<T> (NativeArray<T> pixelBuffer) where T : unmanaged => CopyTo(pixelBuffer.GetUnsafePtr());

        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public virtual unsafe void CopyTo (void* pixelBuffer) {
            // Check
            if (encodedBuffer != null)
                throw new ArgumentException(@"Cannot copy to buffer because image feature is not contiguous");
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
        public virtual unsafe Texture2D ToTexture (Texture2D result = null) {
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
            CopyTo(result.GetRawTextureData<byte>());
            result.Apply();
            // Return
            return result;
        }
        #endregion


        #region --ROI--
        /// <summary>
        /// Get a region-of-interest in the image feature.
        /// If the rectangle extends beyond the bounds of the image, it will be clamped to fit within the image.
        /// </summary>
        /// <param name="rect">ROI rectangle in normalized coordinates.</param>
        /// <returns>Region-of-interest image feature.</returns>
        public MLImageFeature RegionOfInterest (Rect rect) => RegionOfInterest(new RectInt(
            (int)(rect.xMin * width),
            (int)(rect.yMin * height),
            (int)(rect.xMax * width),
            (int)(rect.yMax * height)
        ));

        /// <summary>
        /// Get a region-of-interest in the image feature.
        /// If the rectangle extends beyond the bounds of the image, it will be clamped to fit within the image.
        /// </summary>
        /// <param name="rect">ROI rectangle in pixel coordinates.</param>
        /// <returns>Region-of-interest image feature.</returns>
        public MLImageFeature RegionOfInterest (RectInt rect) {
            // Check
            if (encodedBuffer != null)
                throw new ArgumentException(@"Cannot take RoI because image feature is not contiguous. Call `::Contiguous` first");
            // Clamp
            var bounds = new RectInt(0, 0, width, height);
            rect.ClampToBounds(bounds);
            // Check
            if (rect.width <= 0 || rect.height <= 0)
                throw new ArgumentOutOfRangeException(nameof(rect), @"ROI rectangle has zero area");
            // Copy
            var srcStride = width * 4;
            var dstBuffer = new byte[rect.width * rect.height * 4];
            fixed (byte* dstData = dstBuffer)
                fixed (void* pixelData = this.pixelBuffer)
                    fixed (void* colorData = this.colorBuffer) {
                        var baseAddress = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                        var srcData = (byte*)baseAddress + rect.yMin * srcStride + rect.xMin * 4;
                        UnsafeUtility.MemCpyStride(dstData, rect.width * 4, srcData, srcStride, rect.width * 4, rect.height);
                    }
            // Return
            var feature = new MLImageFeature(dstBuffer, rect.width, rect.height);
            return feature;
        }

        /// <summary>
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        internal MLImageFeature RegionOfInterest (Vector2 center, Vector2 size, float rotation = 0f) => RegionOfInterest(
            Vector2Int.RoundToInt(Vector2.Scale(center, new Vector2(width, height))),
            Vector2Int.RoundToInt(Vector2.Scale(size, new Vector2(width, height))),
            rotation
        );

        /// <summary>
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="rotation"></param>
        internal MLImageFeature RegionOfInterest (Vector2Int center, Vector2Int size, float rotation = 0f) { // INCOMPLETE
            // Check
            if (size.x <= 0 || size.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), @"ROI rectangle has zero area");
            // Extract

            return default;
        }

        /// <summary>
        /// Transform a normalized region-of-interest rectangle from feature space into image space.
        /// </summary>
        /// <param name="rect">Input rectangle.</param>
        /// <param name="featureType">Feature type that defines the input rectangle space.</param>
        /// <returns>Normalized rectangle in image space.</returns>
        public virtual Rect TransformRect (Rect rect, MLImageType featureType) {
            // Shortcut
            if (aspectMode == MLImageFeature.AspectMode.ScaleToFit)
                return rect;
            // Get normalizing factor
            var scaleFactor = 1f;
            if (aspectMode == MLImageFeature.AspectMode.AspectFit)
                scaleFactor = Mathf.Min((float)featureType.width / width, (float)featureType.height / height);
            if (aspectMode == MLImageFeature.AspectMode.AspectFill)
                scaleFactor = Mathf.Max((float)featureType.width / width, (float)featureType.height / height);
            // Compute crop rect
            var w = scaleFactor * width / featureType.width;
            var h = scaleFactor * height / featureType.height;
            var x = 0.5f * (1f - w);
            var y = 0.5f * (1f - h);
            // Scale
            var result = Rect.MinMaxRect(
                (rect.x - x) / w,
                (rect.y - y) / h,
                (rect.xMax - x) / w,
                (rect.yMax - y) / h
            );
            return result;
        }
        #endregion


        #region --Vision--
        /// <summary>
        /// Perform non-max suppression on a set of candidate boxes.
        /// </summary>
        /// <param name="rects">Candidate boxes.</param>
        /// <param name="scores">Candidate scores.</param>
        /// <param name="maxIoU">Maximum IoU for preserving overlapping boxes.</param>
        /// <returns>Indices of boxes to keep.</returns>
        public static int[] NonMaxSuppression (IReadOnlyList<Rect> rects, IReadOnlyList<float> scores, float maxIoU) {
            var discard = new bool[rects.Count];
            for (var i = 0; i < rects.Count - 1; ++i)
                if (!discard[i])
                    for (var j = i + 1; j < rects.Count; ++j)
                        if (!discard[j]) {
                            var iou = IntersectionOverUnion(rects[i], rects[j]);
                            if (iou < maxIoU)
                                continue;
                            if (scores[i] > scores[j])
                                discard[j] = true;
                            else {
                                discard[i] = true;
                                break;
                            }
                        }
            var result = new List<int>();
            for (var i = 0; i < rects.Count; ++i)
                if (!discard[i])
                    result.Add(i);
            return result.ToArray();
        }

        /// <summary>
        /// Calculate the intersection-over-union (IoU) of two rectangles.
        /// </summary>
        public static float IntersectionOverUnion (Rect a, Rect b) {
            var areaA = a.width * a.height;
            var areaB = b.width * b.height;
            var c = Rect.MinMaxRect(
                Mathf.Max(a.xMin, b.xMin),
                Mathf.Max(a.yMin, b.yMin),
                Mathf.Min(a.xMax, b.xMax),
                Mathf.Min(a.yMax, b.yMax)
            );
            var areaC = Mathf.Max(c.width, 0) * Mathf.Max(c.height, 0);
            return areaC / (areaA + areaB - areaC);
        }
        #endregion


        #region --Operations--
        protected readonly byte[] pixelBuffer;
        protected readonly Color32[] colorBuffer;
        protected readonly void* nativeBuffer;
        protected readonly byte[] encodedBuffer;

        unsafe MLEdgeFeature IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check null
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            // Check if encoded
            if (encodedBuffer != null)
                throw new NotImplementedException(@"Cannot create Edge feature because image feature is not contiguous. Call `::Contiguous` first");
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
                        (int)aspectMode,
                        out var feature
                    );
                    return new MLEdgeFeature(feature);
                }
        }

        unsafe MLCloudFeature IMLCloudFeature.Create (in MLFeatureType _) {
            // Shortcut
            if (encodedBuffer != null)
                return new MLCloudFeature {
                    data = new MemoryStream(encodedBuffer, false),
                    type = DataType.Image
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
                    return new MLCloudFeature {
                        data = new MemoryStream(buffer.ToArray(), false),
                        type = DataType.Image
                    };
                }
        }
        #endregion
    }
}