/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using Types;

    /// <summary>
    /// ML image feature.
    /// The image feature will perform any necessary conversions and pre-processing to a model's desired input feature type.
    /// Pixel buffers used to create image features MUST have an RGBA8888 pixel layout.
    /// </summary>
    public unsafe class MLImageFeature : MLFeature, IMLEdgeFeature {

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
        /// <param name="texture">Input texture.</param>
        public MLImageFeature (Texture2D texture) : this(new MLImageType(texture.width, texture.height, 4)) {
            if (!texture.isReadable)
                throw new ArgumentException(@"Cannot create image feature because texture is not readable", nameof(texture));
            if (texture.format == TextureFormat.RGBA32) // zero copy :D
                this.nativeBuffer = texture.GetRawTextureData<byte>().GetUnsafeReadOnlyPtr();
            else
                this.colorBuffer = texture.GetPixels32();
        }

        /// <summary>
        /// Create an image feature from a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer.</param>
        /// <param name="width">Pixel buffer width.</param>
        /// <param name="height">Pixel buffer height.</param>
        public MLImageFeature (
            Color32[] pixelBuffer,
            int width,
            int height
        ) : this(new MLImageType(width, height, 4)) => this.colorBuffer = pixelBuffer;

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
        ) : this(new MLImageType(width, height, 4)) => this.pixelBuffer = pixelBuffer;

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
        ) : this(new MLImageType(width, height, 4)) => this.nativeBuffer = pixelBuffer;
        #endregion


        #region --Copying--
        /// <summary>
        /// Copy the image feature data into a texture.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <param name="destination">Destination texture to copy data into.</param>
        /// <param name="upload">Whether to upload the pixel data to the GPU after copying.</param>
        public virtual unsafe void CopyTo (Texture2D destination, bool upload = true) {
            // Check size
            if (destination.width != width || destination.height != height)
                throw new ArgumentException(@"Cannot copy to texture because texture size does not match feature size", nameof(destination));
            // Check format
            if (destination.format != TextureFormat.RGBA32)
                throw new ArgumentException(@"Cannot copy to texture because texture format is not RGBA32", nameof(destination));
            // Copy data
            CopyTo(destination.GetRawTextureData<byte>());
            if (upload)
                destination.Apply();
        }

        /// <summary>
        /// Copy the image data in this feature into a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public virtual unsafe void CopyTo<T> (T[] pixelBuffer) where T : unmanaged {
            fixed (void* buffer = pixelBuffer)
                CopyTo(buffer);
        }

        /// <summary>
        /// Copy the image data in this feature into a pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public virtual unsafe void CopyTo<T> (NativeArray<T> pixelBuffer) where T : unmanaged => CopyTo(pixelBuffer.GetUnsafePtr());

        /// <summary>
        /// Copy the image data in this feature into the provided pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">Destination pixel buffer.</param>
        public virtual unsafe void CopyTo (void* pixelBuffer) {
            fixed (void* pixelData = this.pixelBuffer)
                fixed (void* colorData = this.colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    UnsafeUtility.MemCpy(pixelBuffer, data, width * height * 4);
                }
        }
        #endregion


        #region --ROI--
        /// <summary>
        /// Get a region-of-interest in the image feature.
        /// </summary>
        /// <param name="rect">ROI rectangle in normalized coordinates.</param>
        /// <param name="rotation">Rectangle clockwise rotation in degrees.</param>
        /// <param name="background">Background color for unmapped pixels.</param>
        /// <returns>Region-of-interest image feature.</returns>
        public virtual MLImageFeature RegionOfInterest (
            Rect rect,
            float rotation = 0f,
            Color32 background = default
        ) => RegionOfInterest(
            new RectInt((int)(rect.xMin * width),(int)(rect.yMin * height),(int)(rect.xMax * width),(int)(rect.yMax * height)),
            rotation,
            background
        );

        /// <summary>
        /// Get a region-of-interest in the image feature.
        /// </summary>
        /// <param name="rect">ROI rectangle in pixel coordinates.</param>
        /// <param name="rotation">Rectangle clockwise rotation in degrees.</param>
        /// <param name="background">Background color for unmapped pixels.</param>
        /// <returns>Region-of-interest image feature.</returns>
        public virtual unsafe MLImageFeature RegionOfInterest (
            RectInt rect,
            float rotation = 0f,
            Color32 background = default
        ) {
            // Check
            if (rect.size.x * rect.size.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(rect), @"ROI rectangle must have non-zero area");
            // Extract
            var center = Vector2Int.RoundToInt(rect.center);
            var dstBuffer = new byte[rect.size.x * rect.size.y * 4];
            var srcRect = stackalloc [] { center.x, center.y, rect.size.x, rect.size.y };
            fixed (byte* dstData = dstBuffer)
                fixed (void* pixelData = this.pixelBuffer)
                    fixed (void* colorData = this.colorBuffer)
                        NatML.RegionOfInterest(
                            nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer,
                            width,
                            height,
                            srcRect,
                            rotation,
                            (byte*)&background,
                            dstData
                        );
            // Return
            var feature = new MLImageFeature(dstBuffer, rect.size.x, rect.size.y);
            return feature;
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
        public virtual Vector2 TransformPoint (Vector2 point, MLImageType featureType) {
            // Shortcut
            if (aspectMode == MLImageFeature.AspectMode.ScaleToFit)
                return point;
            // Get normalizing factor
            var scaleFactor = 1f;
            if (aspectMode == MLImageFeature.AspectMode.AspectFit)
                scaleFactor = Mathf.Max((float)width / featureType.width, (float)height / featureType.height);
            if (aspectMode == MLImageFeature.AspectMode.AspectFill)
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
        public virtual Rect TransformRect (Rect rect, MLImageType featureType) {
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
        private readonly Color32[] colorBuffer;
        private readonly void* nativeBuffer;

        /// <summary>
        /// Initialize the image feature with the image feature type.
        /// </summary>
        /// <param name="type">Image feature type.</param>
        protected MLImageFeature (MLImageType type) : base(type) { }

        unsafe MLEdgeFeature IMLEdgeFeature.Create (MLFeatureType type) {
            fixed (void* pixelData = pixelBuffer)
                fixed (void* colorData = colorBuffer) {
                    var data = nativeBuffer == null ? pixelData == null ? colorData : pixelData : nativeBuffer;
                    var featureType = type as MLArrayType;
                    var meanArr = stackalloc [] { mean.x, mean.y, mean.z, mean.w };
                    var stdArr = stackalloc [] { std.x, std.y, std.z, std.w };
                    NatML.CreateFeature(
                        data,
                        width,
                        height,
                        featureType.shape,
                        EdgeType(featureType.dataType),
                        meanArr,
                        stdArr,
                        (int)aspectMode,
                        out var feature
                    );
                    return new MLEdgeFeature(feature);
                }
        }
        #endregion
    }
}