/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Unity.Collections.LowLevel.Unsafe;
    using Hub;
    using Internal;
    using Types;

    /// <summary>
    /// ML audio feature.
    /// </summary>
    #pragma warning disable 0618
    public sealed unsafe class MLAudioFeature : MLFeature, IMLEdgeFeature, IMLHubFeature, IMLFeature {
    #pragma warning restore 0618

        #region --Client API--
        /// <summary>
        /// Create an audio feature from an audio clip.
        /// </summary>
        /// <param name="clip">Audio clip.</param>
        /// <param name="duration">Optional duration to extract in seconds. Negative values will use the whole clip.</param>
        public MLAudioFeature (
            AudioClip clip,
            float duration = -1
        ) : this(Extract(clip, duration), clip.frequency, clip.channels) { }

        /// <summary>
        /// Create an audio feature from a sample buffer list.
        /// </summary>
        /// <param name="sampleBuffer">List of linear PCM sample buffers interleaved by channel.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        public MLAudioFeature (
            IEnumerable<float[]> bufferList,
            int sampleRate,
            int channelCount
        ) : this(Flatten(bufferList), sampleRate, channelCount) { }

        /// <summary>
        /// Create an audio feature from a sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        public MLAudioFeature (
            float[] sampleBuffer,
            int sampleRate,
            int channelCount
        ) : base(new MLAudioType(sampleRate, channelCount, sampleBuffer.Length)) => this.sampleBuffer = sampleBuffer;

        /// <summary>
        /// Create an audio feature from a sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleCount">Total sample count.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        public unsafe MLAudioFeature (
            float* sampleBuffer,
            int sampleCount,
            int sampleRate,
            int channelCount
        ) : base(new MLAudioType(sampleRate, channelCount, sampleCount)) => this.nativeBuffer = sampleBuffer;

        /// <summary>
        /// Create an audio feature from a Hub feature.
        /// </summary>
        /// <param name="hubFeature">Hub feature. This MUST be an `AUDIO` feature.</param>
        public MLAudioFeature (MLHubFeature hubFeature) : base(new MLAudioType(0, 0, 0)) {
            // Check
            if (hubFeature.type != HubDataType.Audio)
                throw new ArgumentException(@"Hub feature is not an audio feature", nameof(hubFeature));
            // Not implemented
            throw new NotImplementedException(@"Deserializing audio features is not yet supported");
        }
        #endregion


        #region --Operations--
        private readonly float[] sampleBuffer;
        private readonly float* nativeBuffer;

        unsafe IntPtr IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check types
            var featureType = type as MLArrayType;
            var audioType = this.type as MLArrayType;
            if (featureType.dataType != audioType.dataType)
                throw new ArgumentException($"MLModel expects {featureType.dataType} feature but was given {audioType.dataType} feature");
            if (featureType.dims != audioType.dims)
                throw new ArgumentException($"MLModel expects {featureType.dims}D feature but was given {audioType.dims}D feature");
            // Create feature
            fixed (float* managedBuffer = sampleBuffer) {
                var data = nativeBuffer != null ? nativeBuffer : managedBuffer;
                NatML.CreateFeature(
                    data,
                    audioType.shape,
                    audioType.shape.Length,
                    EdgeType(type.dataType),
                    0,
                    out var feature
                );
                return feature;
            }
        }

        unsafe MLHubFeature IMLHubFeature.Serialize () { // Courtesy of NatCorder
            using (var stream = new MemoryStream())
                fixed (float* managedBuffer = sampleBuffer) {
                    var data = nativeBuffer != null ? nativeBuffer : managedBuffer;
                    var type = this.type as MLAudioType;
                    var sampleCount = type.shape[1];
                    // Write header
                    stream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
                    stream.Write(BitConverter.GetBytes(stream.Length - 8), 0, 4);
                    stream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
                    stream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
                    stream.Write(BitConverter.GetBytes(16), 0, 4);
                    stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                    stream.Write(BitConverter.GetBytes(type.channelCount), 0, 2);
                    stream.Write(BitConverter.GetBytes(type.sampleRate), 0, 4);
                    stream.Write(BitConverter.GetBytes(type.sampleRate * type.channelCount * sizeof(short)), 0, 4);
                    stream.Write(BitConverter.GetBytes((ushort)(type.channelCount * 2)), 0, 2);
                    stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
                    stream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
                    stream.Write(BitConverter.GetBytes(sampleCount * sizeof(ushort)), 0, 4);
                    // Write data
                    fixed (short* shortBuffer = new short[sampleCount]) {
                        for (var i = 0; i < sampleCount; ++i)
                            shortBuffer[i] = (short)(data[i] * short.MaxValue);
                        new UnmanagedMemoryStream((byte*)shortBuffer, sampleCount * sizeof(short)).CopyTo(stream);
                    }
                    // Close
                    var buffer = stream.ToArray();
                    return new MLHubFeature {
                        data = Convert.ToBase64String(buffer),
                        type = HubDataType.Audio
                    };
                }
        }

        private static float[] Extract (AudioClip clip, float duration = -1) {
            var frameCount = duration < 0 ? clip.samples : Mathf.RoundToInt(clip.frequency * duration);
            frameCount = Mathf.Min(frameCount, clip.samples);
            var sampleBuffer = new float[frameCount * clip.channels];
            clip.GetData(sampleBuffer, 0);
            return sampleBuffer;
        }

        private static unsafe float[] Flatten (IEnumerable<float[]> bufferList) {
            var bufferSize = bufferList.Select(s => s.Length).Sum();
            var result = new float[bufferSize];
            var idx = 0;
            fixed (float* dst = result)
                foreach (var sampleBuffer in bufferList)
                    fixed (float* src = sampleBuffer) {
                        UnsafeUtility.MemCpy(&dst[idx], src, sampleBuffer.Length * sizeof(float));
                        idx += sampleBuffer.Length;
                    }
            return result;
        }
        #endregion
    }
}