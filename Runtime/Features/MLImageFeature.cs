/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using API.Types;
    using Internal;
    using Types;

    /// <summary>
    /// ML image feature.
    /// The image feature will perform any necessary conversions and pre-processing to a model's desired input feature type.
    /// Pixel buffers used to create image features MUST have an RGBA8888 pixel layout.
    /// </summary>
    public sealed unsafe class MLImageFeature : MLFeature, IMLEdgeFeature, IMLCloudFeature {

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
        /// Create an empty image feature.
        /// </summary>
        /// <param name="width">Image feature width.</param>
        /// <param name="height">Image feature height.</param>
        public MLImageFeature (int width, int height) : this(new byte[width * height * 4], width, height) { }

        /// <summary>
        /// Create an image feature from a texture.
        /// </summary>
        /// <param name="texture">Input texture.</param>
        public MLImageFeature (Texture2D texture) : base(new MLImageType(texture.width, texture.height, 4)) {
            if (!texture.isReadable)
                throw new ArgumentException(@"Cannot create image feature because texture is not readable", nameof(texture));
            if (texture.format == TextureFormat.RGBA32) // zero copy :D
                this.nativeBuffer = texture.GetRawTextureData<byte>().GetUnsafeReadOnlyPtr();
            else // double copy :(
                this.pixelBuffer = ToPixelBuffer(texture.GetPixels32(), texture.width, texture.height);
        }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="colorBuffer">Color buffer.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (
            Color32[] colorBuffer,
            int width,
            int height
        ) : this(ToPixelBuffer(colorBuffer, width, height), width, height) { }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (
            byte[] pixelBuffer,
            int width,
            int height
        ) : base(new MLImageType(width, height, 4)) => this.pixelBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// Note that the native array MUST remain valid for the lifetime of the feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (
            NativeArray<byte> pixelBuffer,
            int width,
            int height
        ) : this(pixelBuffer.GetUnsafeReadOnlyPtr(), width, height) { }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// Note that the pixel buffer MUST remain valid for the lifetime of the feature.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer. MUST have an RGBA8888 layout.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public unsafe MLImageFeature (
            void* pixelBuffer,
            int width,
            int height
        ) : base(new MLImageType(width, height, 4)) => this.nativeBuffer = pixelBuffer;

        /// <summary>
        /// Create an image feature from a cloud feature.
        /// This constructor MUST only be used on the Unity main thread.
        /// </summary>
        /// <param name="feature">Cloud feature. This MUST be an `image` feature.</param>
        public MLImageFeature (MLCloudFeature feature) : base(new MLImageType(feature.shape, typeof(byte))) {
            // Check
            if (feature.type != Dtype.Image)
                throw new ArgumentException(@"Cloud feature is not an image feature", nameof(feature));
            // Load image
            var texture = new Texture2D(16, 16);
            ImageConversion.LoadImage(texture, feature.data.ToArray(), false);
            this.pixelBuffer = ToPixelBuffer(texture.GetPixels32(), texture.width, texture.height);
            // Release image
            Texture2D.Destroy(texture);
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Copy the image feature into another feature.
        /// </summary>
        /// <param name="destination">Feature to copy data into.</param>
        public void CopyTo (MLImageFeature destination) => CopyTo(
            destination,
            new RectInt(0, 0, width, height)
        );

        /// <summary>
        /// Copy an image feature region of interest into another feature.
        /// </summary>
        /// <param name="destination">Feature to copy data into.</param>
        /// <param name="rect">ROI rectangle in normalized coordinates.</param>
        /// <param name="rotation">Rectangle clockwise rotation in degrees.</param>
        /// <param name="background">Background color for unmapped pixels.</param>
        public void CopyTo (
            MLImageFeature destination,
            Rect rect,
            float rotation = 0f,
            Color32 background = default
        ) => CopyTo(
            destination,
            new RectInt((int)(rect.xMin * width), (int)(rect.yMin * height), (int)(rect.xMax * width), (int)(rect.yMax * height)),
            rotation,
            background
        );

        /// <summary>
        /// Copy an image feature region of interest into another feature.
        /// </summary>
        /// <param name="destination">Feature to copy data into.</param>
        /// <param name="rect">ROI rectangle in pixel coordinates.</param>
        /// <param name="rotation">Rectangle clockwise rotation in degrees.</param>
        /// <param name="background">Background color for unmapped pixels.</param>
        public unsafe void CopyTo (
            MLImageFeature destination,
            RectInt rect,
            float rotation = 0f,
            Color32 background = default
        ) {
            // Check
            if (rect.size.x <= 0 || rect.size.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rect), @"ROI rectangle must have positive width and height");
            // Check
            if (rect.size.x != destination.width || rect.size.y != destination.height)
                throw new ArgumentOutOfRangeException(nameof(rect), @"ROI rectangle size does not match destination feature size");
            // Copy
            var center = Vector2Int.RoundToInt(rect.center);
            var srcRect = stackalloc [] { center.x, center.y, rect.size.x, rect.size.y };
            var srcBuffer = this.nativeBuffer;
            var dstBuffer = destination.nativeBuffer;
            fixed (void* src = this, dst = destination)
                NatML.CopyTo(src, width, height, srcRect, rotation, (byte*)&background, dst);
        }

        /// <summary>
        /// Copy the image feature data into a texture.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <param name="destination">Texture to copy data into.</param>
        /// <param name="upload">Whether to upload the pixel data to the GPU after copying.</param>
        public unsafe void CopyTo (
            Texture2D destination,
            bool upload = true
        ) {
            // Check size
            if (destination.width != width || destination.height != height)
                throw new ArgumentException(@"Cannot copy to texture because texture size does not match feature size", nameof(destination));
            // Check format
            if (destination.format != TextureFormat.RGBA32)
                throw new ArgumentException(@"Cannot copy to texture because texture format is not RGBA32", nameof(destination));
            // Copy data
            var view = new MLImageFeature(destination.GetRawTextureData<byte>(), width, height);
            CopyTo(view);
            // Upload
            if (upload)
                destination.Apply();
        }

        /// <summary>
        /// Create a texture from the image feature.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <returns>Texture containing image feature data.</returns>
        public Texture2D ToTexture () {
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            CopyTo(result, upload: true);
            return result;
        }
        #endregion


        #region --Coordinate Transformations--
        /// <summary>
        /// Transform a normalized point from feature space into image space.
        /// This method is used by detection models to correct for aspect ratio padding when making predictions.
        /// </summary>
        /// <param name="rect">Input point.</param>
        /// <param name="featureType">Feature type that defines the input space.</param>
        /// <returns>Normalized point in image space.</returns>
        public Vector2 TransformPoint (Vector2 point, MLImageType featureType) {
            // Shortcut
            if (aspectMode == AspectMode.ScaleToFit)
                return point;
            // Get normalizing factor
            var scaleFactor = 1f;
            if (aspectMode == AspectMode.AspectFit)
                scaleFactor = Mathf.Max((float)width / featureType.width, (float)height / featureType.height);
            if (aspectMode == AspectMode.AspectFill)
                scaleFactor = Mathf.Min((float)width / featureType.width, (float)height / featureType.height);
            // Transform
            var transform = Matrix4x4.Scale(new Vector3(1f / width, 1f / height, 1f)) *
                Matrix4x4.Translate(0.5f * new Vector2(width, height)) *
                Matrix4x4.Scale(new Vector3(scaleFactor * featureType.width, scaleFactor * featureType.height, 1f)) *
                Matrix4x4.Translate(-0.5f * Vector2.one);
            var result = transform.MultiplyPoint3x4(new Vector3(point.x, point.y, 1f));
            return result;
        }

        /// <summary>
        /// Transform a normalized region-of-interest rectangle from feature space into image space.
        /// This method is used by detection models to correct for aspect ratio padding when making predictions.
        /// </summary>
        /// <param name="rect">Input rectangle.</param>
        /// <param name="featureType">Feature type that defines the input space.</param>
        /// <returns>Normalized rectangle in image space.</returns>
        public Rect TransformRect (Rect rect, MLImageType featureType) {
            var min = TransformPoint(rect.min, featureType);
            var max = TransformPoint(rect.max, featureType);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
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
        public static int[] NonMaxSuppression (
            IReadOnlyList<Rect> rects,
            IReadOnlyList<float> scores,
            float maxIoU
        ) {
            var discard = new bool[rects.Count];
            for (int i = 0, ilen = discard.Length - 1; i < ilen; ++i)
                if (!discard[i])
                    for (var j = i + 1; j < discard.Length; ++j)
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
            for (var i = 0; i < discard.Length; ++i)
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
        private readonly byte[] pixelBuffer;
        private readonly void* nativeBuffer;

        public ref byte GetPinnableReference () => ref (nativeBuffer == null ? ref pixelBuffer[0] : ref *(byte*)nativeBuffer);

        unsafe MLEdgeFeature IMLEdgeFeature.Create (MLFeatureType type) {
            fixed (void* data = this) {
                var featureType = type as MLArrayType;
                var meanArr = stackalloc [] { mean.x, mean.y, mean.z, mean.w };
                var stdArr = stackalloc [] { std.x, std.y, std.z, std.w };
                NatML.CreateFeature(
                    data,
                    width,
                    height,
                    featureType.shape,
                    featureType.dataType.ToDtype(),
                    meanArr,
                    stdArr,
                    (int)aspectMode,
                    out var feature
                );
                return new MLEdgeFeature(feature);
            }
        }

        unsafe MLCloudFeature IMLCloudFeature.Create (MLFeatureType _) {
            fixed (void* data = this) {
                var size = width * height * 4;
                var pixelArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, size, Allocator.None);
                using var buffer = ImageConversion.EncodeNativeArrayToJPG(pixelArray, GraphicsFormat.R8G8B8A8_UNorm, (uint)width, (uint)height, quality: 80);
                var encodedData = buffer.ToArray();
                var stream = new MemoryStream(encodedData, 0, encodedData.Length, false, false);
                var shape = (type as MLImageType).shape;
                var feature = new MLCloudFeature(stream, Dtype.Image, shape);
                return feature;
            }
        }

        private static unsafe byte[] ToPixelBuffer (Color32[] colorBuffer, int width, int height) {
            var pixelBuffer = new byte[width * height * 4];
            fixed (void* src = colorBuffer, dst = pixelBuffer)
                Buffer.MemoryCopy(src, dst, pixelBuffer.Length, pixelBuffer.Length);
            return pixelBuffer;
        }
        #endregion
    }
}