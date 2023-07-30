/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using API.Types;
    using Internal;
    using Types;

    /// <summary>
    /// ML video feature.
    /// The video feature is always backed by a video file, and provides access to the contained image frames.
    /// When enumerated the video feature yields image frames as `MLImageFeature` instances, along with corresponding frame timestamps.
    /// The enumerated image features can then be used for predictions with ML models.
    /// </summary>
    public sealed class MLVideoFeature : MLFeature, IEnumerable<(MLImageFeature feature, long timestamp)> {

        #region --Inspection--
        /// <summary>
        /// Video path.
        /// </summary>
        public readonly string path;

        /// <summary>
        /// Video width.
        /// </summary>
        public int width => (type as MLVideoType).width;

        /// <summary>
        /// Video height.
        /// </summary>
        public int height => (type as MLVideoType).height;

        /// <summary>
        /// Video frame count.
        /// Note that this is an approximate count.
        /// </summary>
        public int frames => (type as MLVideoType).frames;
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
        /// Create an video feature from a video file.
        /// </summary>
        /// <param name="path">Video file path.</param>
        public MLVideoFeature (string path) : base(MLVideoType.FromFile(path)) => this.path = path;

        /// <summary>
        /// Create a video feature from a video file in the `StreamingAssets` folder.
        /// </summary>
        /// <param name="relativePath">Relative path to video file in `StreamingAssets` folder.</param>
        /// <returns>Video feature or `null` if no valid video can be found at the relative path.</returns>
        public static async Task<MLVideoFeature> FromStreamingAssets (string relativePath) {
            var absolutePath = await MLUnityExtensions.StreamingAssetsToAbsolutePath(relativePath);
            return new MLVideoFeature(absolutePath);
        }
        #endregion


        #region --Operations--

        IEnumerator<(MLImageFeature, long)> IEnumerable<(MLImageFeature feature, long timestamp)>.GetEnumerator () {
            // Create reader
            NatML.CreateImageFeatureReader(path, out var reader);
            if (reader == IntPtr.Zero)
                throw new InvalidOperationException(@"Failed to create image reader");
            // Read
            var feature = IntPtr.Zero;
            try {
                for (;;) {
                    // Read frame
                    feature.ReleaseFeature();
                    feature = IntPtr.Zero;
                    reader.ReadNextFeature(out var timestamp, out feature);
                    // EOS
                    if (timestamp < 0)
                        break;
                    // Skip
                    if (feature == IntPtr.Zero)
                        continue;
                    // Create image
                    var imageFeature = CreateImageFeature(feature);
                    imageFeature.mean = mean;
                    imageFeature.std = std;
                    imageFeature.aspectMode = aspectMode;
                    yield return (imageFeature, timestamp);
                }
            }
            // Release
            finally {
                feature.ReleaseFeature();
                reader.ReleaseFeatureReader();
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<(MLImageFeature, long)>).GetEnumerator();

        private static unsafe MLImageFeature CreateImageFeature (IntPtr feature) {
            NatML.FeatureType(feature, out var type);
            var shape = new int[4];
            type.FeatureTypeShape(shape, shape.Length);
            type.ReleaseFeatureType();
            return new MLImageFeature((void*)feature.FeatureData(), shape[2], shape[1]);
        }
        #endregion
    }
}