/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Hub;
    using Internal;
    using Types;

    /// <summary>
    /// ML audio feature.
    /// The audio feature will perform any necessary conversions and pre-processing to a model's desired input feature type.
    /// Sample buffers used to create audio features MUST be floating-point linear PCM data interleaved by channel and in range [-1.0, 1.0].
    /// </summary>
    public sealed class MLAudioFeature : MLFeature, IMLEdgeFeature, IMLHubFeature, IEnumerable<(MLAudioFeature feature, long timestamp)> {

        #region --Preprocessing--
        /// <summary>
        /// Desired sample rate for Edge predictions.
        /// </summary>
        public int sampleRate;

        /// <summary>
        /// Desired channel count for Edge predictions.
        /// </summary>
        public int channelCount;

        /// <summary>
        /// Normalization mean.
        /// </summary>
        public float mean = 0f;

        /// <summary>
        /// Normalization standard deviation.
        /// </summary>
        public float std = 1f;
        #endregion


        #region --Constructors--
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
        ) : base(new MLAudioType(sampleRate, channelCount, sampleBuffer.Length)) {
            this.sampleBuffer = sampleBuffer;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
        }

        /// <summary>
        /// Create an audio feature from a sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        public unsafe MLAudioFeature (
            NativeArray<float> sampleBuffer,
            int sampleRate,
            int channelCount
        ) : this((float*)sampleBuffer.GetUnsafeReadOnlyPtr(), sampleRate, channelCount, sampleBuffer.Length) { }

        /// <summary>
        /// Create an audio feature from a sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleCount">Total sample count.</param>
        public unsafe MLAudioFeature (
            float* sampleBuffer,
            int sampleRate,
            int channelCount,
            int sampleCount
        ) : base(new MLAudioType(sampleRate, channelCount, sampleCount)) {
            this.nativeBuffer = (IntPtr)sampleBuffer;
            this.sampleRate = sampleRate;
            this.channelCount = channelCount;
        }

        /// <summary>
        /// Create an audio feature from an audio or video file.
        /// The file is not loaded into memory and as such the feature will not contain any audio samples.
        /// Instead the feature must be enumerated to retrieve contained audio features that contain samples.
        /// </summary>
        /// <param name="path">Audio file path.</param>
        public MLAudioFeature (string path) : base(MLAudioType.FromFile(path)) {
            this.path = path;
            this.sampleRate = (type as MLAudioType).sampleRate;
            this.channelCount = (type as MLAudioType).channelCount;
        }

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

        /// <summary>
        /// Create an audio feature from an audio or video file in the `StreamingAssets` folder.
        /// </summary>
        /// <param name="relativePath">Relative path to audio file in `StreamingAssets` folder.</param>
        /// <returns>Audio feature or `null` if no valid audio can be found at the relative path.</returns>
        public static async Task<MLAudioFeature> FromStreamingAssets (string relativePath) {
            var absolutePath = await MLModelData.StreamingAssetsToAbsolutePath(relativePath);
            return new MLAudioFeature(absolutePath);
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Create an audio feature which contains all audio samples loaded into memory.
        /// Note that this can consume large amounts of memory for large audio files.
        /// For better memory consumption, manually enumerate the feature instead.
        /// </summary>
        /// <returns>Audio feature which contains all audio samples in memory.</returns>
        public Task<MLAudioFeature> ReadToEnd () => Task.Run(() => {
            // Check
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException(@"Cannot load audio feature into memory because it is not backed by an audio file");
            // Read
            var bufferList = new List<float[]>();
            foreach (var (feature, timestamp) in this) {
                var type = feature.type as MLAudioType;
                var buffer = new float[type.elementCount];
                feature.CopyTo(buffer);
                bufferList.Add(buffer);
            }
            return new MLAudioFeature(bufferList, sampleRate, channelCount);
        });

        /// <summary>
        /// Copy the audio data in this feature into the provided sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Destination sample buffer.</param>
        public unsafe void CopyTo (float[] sampleBuffer) {
            fixed (float* buffer = sampleBuffer)
                CopyTo(buffer);
        }

        /// <summary>
        /// Copy the audio data in this feature into the provided sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Destination sample buffer.</param>
        public unsafe void CopyTo (NativeArray<float> sampleBuffer) => CopyTo((float*)sampleBuffer.GetUnsafePtr());

        /// <summary>
        /// Copy the audio data in this feature into the provided sample buffer.
        /// </summary>
        /// <param name="sampleBuffer">Destination sample buffer.</param>
        public unsafe void CopyTo (float* sampleBuffer) {
            // Check
            if (!string.IsNullOrEmpty(path))
                throw new InvalidOperationException(@"Audio feature refers to audio file and must be read into memory to access sample data");
            // Copy
            fixed (float* sampleData = this.sampleBuffer) {
                var data = nativeBuffer == IntPtr.Zero ? sampleData : (float*)nativeBuffer;
                UnsafeUtility.MemCpy(sampleBuffer, data, (type as MLAudioType).elementCount * sizeof(float));
            }
        }

        /// <summary>
        /// Convert the audio feature to an audio clip.
        /// This method MUST only be used from the Unity main thread.
        /// </summary>
        /// <returns>Result audio clip.</returns>
        public AudioClip ToAudioClip () {
            // Check
            if (!string.IsNullOrEmpty(path))
                throw new InvalidOperationException(@"Audio feature refers to audio file and must be read into memory to access sample data");
            // Convert
            var type = this.type as MLAudioType;
            var clip = AudioClip.Create("MLAudioFeature", type.elementCount, type.channelCount, type.sampleRate, false);
            var sampleBuffer = this.sampleBuffer;
            if (sampleBuffer == null) {
                sampleBuffer = new float[type.elementCount];
                CopyTo(sampleBuffer);
            }
            clip.SetData(sampleBuffer, 0);
            return clip;
        }
        #endregion


        #region --Operations--
        private readonly float[] sampleBuffer;
        private readonly IntPtr nativeBuffer;
        private readonly string path;

        unsafe MLEdgeFeature IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check types
            var featureType = type as MLArrayType;
            var audioType = this.type as MLAudioType;
            // Create feature
            var mean = Enumerable.Repeat(this.mean, channelCount).ToArray();
            var std = Enumerable.Repeat(this.std, channelCount).ToArray();
            fixed (float* managedBuffer = sampleBuffer) {
                var data = nativeBuffer != IntPtr.Zero ? (float*)nativeBuffer : managedBuffer;
                NatML.CreateFeature(
                    data,
                    audioType.sampleRate,
                    audioType.shape,    // [1, frames, channels]
                    sampleRate,
                    channelCount,
                    EdgeType(featureType.dataType),
                    mean,
                    std,
                    0,
                    out var feature
                );
                return new MLEdgeFeature(feature);
            }
        }

        unsafe MLHubFeature IMLHubFeature.Serialize () { // Courtesy of NatCorder
            fixed (float* managedBuffer = sampleBuffer) {
                var stream = new MemoryStream();
                var data = nativeBuffer != IntPtr.Zero ? (float*)nativeBuffer : managedBuffer;
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
                // Return
                return new MLHubFeature {
                    data = stream,
                    type = HubDataType.Audio
                };
            }
        }

        IEnumerator<(MLAudioFeature, long)> IEnumerable<(MLAudioFeature feature, long timestamp)>.GetEnumerator () {
            // Check path
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException(@"Cannot enumerate audio feature because it is not backed by a media file");
            // Create reader
            NatML.CreateAudioFeatureReader(path, out var reader);
            if (reader == IntPtr.Zero)
                throw new InvalidOperationException(@"Failed to create audio reader");
            // Read
            var feature = IntPtr.Zero;
            var type = this.type  as MLAudioType;
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
                    // Create feature
                    var audioFeature = CreateAudioFeature(feature, type);
                    audioFeature.sampleRate = sampleRate;
                    audioFeature.channelCount = channelCount;
                    audioFeature.mean = mean;
                    audioFeature.std = std;
                    yield return (audioFeature, timestamp);
                }
            }
            // Release
            finally {
                feature.ReleaseFeature();
                reader.ReleaseFeatureReader();
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<(MLAudioFeature, long)>).GetEnumerator();

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

        private static unsafe MLAudioFeature CreateAudioFeature (IntPtr feature, MLAudioType audioType) {
            NatML.FeatureType(feature, out var type);
            var shape = new int[3];
            type.FeatureTypeShape(shape, shape.Length);
            type.ReleaseFeatureType();
            return new MLAudioFeature((float*)feature.FeatureData(), audioType.sampleRate, shape[2], shape[1] * shape[2]);
        }
        #endregion
    }
}