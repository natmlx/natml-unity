/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Types {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Internal;

    /// <summary>
    /// ML video feature type.
    /// </summary>
    public class MLVideoType : MLImageType {

        #region --Client API--
        /// <summary>
        /// Video width.
        /// </summary>
        public override int width => shape?[interleaved ? 3 : 4] ?? 0;

        /// <summary>
        /// Video height.
        /// </summary>
        public override int height => shape?[interleaved ? 2 : 3] ?? 0;

        /// <summary>
        /// Video channels.
        /// </summary>
        public override int channels => shape?[interleaved ? 4 : 2] ?? 0;

        /// <summary>
        /// Video frame count.
        /// Note that this is almost always an approximate count.
        /// </summary>
        public virtual int frames => shape?[1] ?? 0;

        /// <summary>
        /// Create an video feature type.
        /// </summary>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frames">Video frame count.</param>
        public MLVideoType (int width, int height, int frames) : this(width, height, frames, typeof(byte)) { }

        /// <summary>
        /// Create an video feature type.
        /// </summary>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="frames">Video frame count.</param>
        /// <param name="type">Video frame data type.</param>
        public MLVideoType (int width, int height, int frames, Type type) : this(new [] { 1, frames, height, width, 3 }, type) { }

        /// <summary>
        /// Create an video feature type.
        /// </summary>
        /// <param name="shape">Video feature shape.</param>
        /// <param name="type">Video frame data type.</param>
        /// <param name="name">Feature name.</param>
        public MLVideoType (int[] shape, Type type, string name = null) : base(shape, type, name) => this.interleaved = shape == null || shape[2] > shape[4];

        /// <summary>
        /// Get the video type for a video file at a given path.
        /// </summary>
        /// <param name="path">Video path.</param>
        /// <returns>Corresponding video type or `null` if file is not a valid video file.</returns>
        public static MLVideoType FromFile (string path) {
            // Check
            if (!File.Exists(path))
                throw new ArgumentException(@"Cannot create video type because file does not exist", nameof(path));
            // Populate
            var name = Path.GetFileName(path);
            NatML.GetVideoFormat(path, out var width, out var height, out var frames);
            return new MLVideoType(new [] { 1, frames, height, width, 4 }, typeof(byte), name);
        }

        /// <summary>
        /// Get the video type for a video file in the `StreamingAssets` folder.
        /// </summary>
        /// <param name="relativePath">Relative path to video file in `StreamingAssets` folder.</param>
        /// <returns>Corresponding video type or `null` if the file is not a valid video file.</returns>
        public static async Task<MLVideoType> FromStreamingAssets (string relativePath) {
            var absolutePath = await MLUnityExtensions.StreamingAssetsToAbsolutePath(relativePath);
            return FromFile(absolutePath);
        }
        #endregion
    }
}