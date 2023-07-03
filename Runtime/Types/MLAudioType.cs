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
    /// ML audio feature type.
    /// Audio types always represent floating-point linear PCM data.
    /// </summary>
    public class MLAudioType : MLArrayType {

        #region --Client API--
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public virtual int sampleRate { get; protected set; }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public virtual int channelCount => shape[2];

        /// <summary>
        /// Audio frame count.
        /// </summary>
        public virtual int frames => shape[1];

        /// <summary>
        /// Create an audio feature type.
        /// <summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleCount">Total sample count.</param>
        /// <param name="name">Feature name.</param>
        public MLAudioType (
            int sampleRate,
            int channelCount,
            int sampleCount,
            string name = null
        ) : this(new [] { 1, sampleCount / channelCount, channelCount }, name) => this.sampleRate = sampleRate;

        /// <summary>
        /// Get the audio type for an audio file at a given path.
        /// </summary>
        /// <param name="path">Audio path.</param>
        /// <returns>Corresponding audio type or `null` if the file is not a valid audio file.</returns>
        public static MLAudioType FromFile (string path) {
            // Check
            if (!File.Exists(path))
                throw new ArgumentException(@"Cannot create audio type because file does not exist", nameof(path));
            // Populate
            var name = Path.GetFileName(path);
            NatML.GetAudioFormat(path, out var sampleRate, out var channelCount, out var sampleCount);
            return new MLAudioType(sampleRate, channelCount, sampleCount, name);
        }

        /// <summary>
        /// Get the audio type for an audio file in the `StreamingAssets` folder.
        /// </summary>
        /// <param name="relativePath">Relative path to audio file in `StreamingAssets` folder.</param>
        /// <returns>Corresponding audio type or `null` if the file is not a valid audio file.</returns>
        public static async Task<MLAudioType> FromStreamingAssets (string relativePath) {
            var absolutePath = await MLUnityExtensions.StreamingAssetsToAbsolutePath(relativePath);
            return FromFile(absolutePath);
        }
        #endregion


        #region --Operations--
        /// <summary>
        /// Create an audio feature type.
        /// <summary>
        /// <param name="shape">Audio tensor shape.</param>
        /// <param name="name">Feature name.</param>
        protected MLAudioType (int[] shape, string name = null) : base(shape, typeof(float), name) { }

        public override string ToString () {
            var nameStr = name != null ? $"{name}: " : string.Empty;
            var shapeStr = $"({string.Join(", ", shape)})";
            var formatStr = $"{sampleRate}Hz {channelCount}ch";
            return $"{nameStr}{shapeStr} {formatStr} {dataType}";
        }
        #endregion
    }
}