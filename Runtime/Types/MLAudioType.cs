/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Types {

    /// <summary>
    /// ML audio feature type.
    /// Audio types always represent floating-point linear PCM data.
    /// </summary>
    public sealed class MLAudioType : MLArrayType {

        #region --Client API--
        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public readonly int sampleRate;

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount => shape[2];

        /// <summary>
        /// Create an audio feature type.
        /// <summary>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleCount">Total sample count.</param>
        public MLAudioType (
            int sampleRate,
            int channelCount,
            int sampleCount
        ) : this(default, sampleRate, channelCount, sampleCount) { }

        /// <summary>
        /// Create an audio feature type.
        /// <summary>
        /// <param name="name">Feature name.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleCount">Total sample count.</param>
        public MLAudioType (
            string name,
            int sampleRate,
            int channelCount,
            int sampleCount
        ) : base(null, typeof(float), new [] { 1, sampleCount / channelCount, channelCount }) => this.sampleRate = sampleRate;
        #endregion
    }
}