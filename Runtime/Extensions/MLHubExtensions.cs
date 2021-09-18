/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Extensions {

    using System;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public static partial class MLPredictorExtensions {

        #region --Client API--
        /// <summary>
        /// Serialize an audio sample buffer to a waveform.
        /// This method is thread-safe and can be used from any thread.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        /// <returns>Serialized waveform data.</returns>
        public static unsafe byte[] SerializeAudio (in float[] sampleBuffer, in int sampleRate, in int channelCount) {
            fixed (float* buffer = sampleBuffer)
                return SerializeAudio(buffer, sampleBuffer.Length, sampleRate, channelCount);
        }

        /// <summary>
        /// Serialize an audio sample buffer to a waveform.
        /// This method is thread-safe and can be used from any thread.
        /// </summary>
        /// <param name="sampleBuffer">Linear PCM sample buffer interleaved by channel.</param>
        /// <param name="sampleCount">Total sample count in the buffer.</param>
        /// <param name="sampleRate">Audio sample rate.</param>
        /// <param name="channelCount">Audio channel count.</param>
        /// <returns>Serialized waveform data.</returns>
        public static unsafe byte[] SerializeAudio (in float* sampleBuffer, in int sampleCount, in int sampleRate, in int channelCount) { // Courtesy of NatCorder
            var stream = new MemoryStream();
            // Write header
            stream.Write(Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes(stream.Length - 8), 0, 4);
            stream.Write(Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            stream.Write(Encoding.UTF8.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);                                // Channel count
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);                                  // Sample rate
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * sizeof(short)), 0, 4);   // Output rate in bytes
            stream.Write(BitConverter.GetBytes((ushort)(channelCount * 2)), 0, 2);                  // Block alignment
            stream.Write(BitConverter.GetBytes((ushort)16), 0, 2);                                  // Bits per sample
            stream.Write(Encoding.UTF8.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes(sampleCount * sizeof(ushort)), 0, 4);        // Total sample count
            // Write data
            fixed (short* shortBuffer = new short[sampleCount]) {
                for (var i = 0; i < sampleCount; ++i)
                    shortBuffer[i] = (short)(sampleBuffer[i] * short.MaxValue);
                new UnmanagedMemoryStream((byte*)shortBuffer, sampleCount * sizeof(short)).CopyTo(stream);
            }
            // Close
            var buffer = stream.ToArray();
            stream.Dispose();
            return buffer;
        }

        /// <summary>
        /// Serialize a pixel buffer to a PNG.
        /// This method is thread-safe and can be used from any thread.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer with RGBA8888 layout.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>Serialized PNG data.</returns>
        public static unsafe byte[] SerializeImage<T> (in T[] pixelBuffer, in int width, in int height) where T : unmanaged {
            fixed (T* buffer = pixelBuffer)
                return SerializeImage(buffer, width, height);
        }

        /// <summary>
        /// Serialize a pixel buffer to a PNG.
        /// This method is thread-safe and can be used from any thread.
        /// </summary>
        /// <param name="pixelBuffer">Pixel buffer with RGBA8888 layout.</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>Serialized PNG data.</returns>
        public static unsafe byte[] SerializeImage (in void* pixelBuffer, in int width, in int height) { // Courtesy of NatCorder
            var pixelArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                pixelBuffer,
                width * height * 4,
                Allocator.None
            );
            var buffer = ImageConversion.EncodeNativeArrayToPNG(
                pixelArray,
                GraphicsFormat.R8G8B8A8_UNorm,
                (uint)width,
                (uint)height
            ).ToArray();
            return buffer;
        }
        #endregion
    }
}