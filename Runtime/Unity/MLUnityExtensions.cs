/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using API;
    using API.Graph;
    using API.Types;
    using Internal;

    /// <summary>
    /// Utilities for working with Unity.
    /// </summary>
    public static class MLUnityExtensions {

        /// <summary>
        /// Create a NatML client that won't break on WebGL.
        /// </summary>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns>NatML client.</returns>
        public static NatMLClient CreateClient (string? accessKey = null, string? url = null) {
            url ??= NatMLClient.URL;
            accessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : NatMLSettings.Instance.accessKey;
            IGraphClient graphClient = Application.platform == RuntimePlatform.WebGLPlayer ?
                new UnityGraphClient(url, accessKey) :
                new DotNetClient(url, accessKey);
            var client = new NatMLClient(graphClient);
            return client;
        }

        /// <summary>
        /// Convert a `StreamingAssets` path to an absolute path accessible on the file system.
        /// This function will perform any necessary copying to ensure that the file is accessible.
        /// </summary>
        /// <param name="relativePath">Relative path to target file in `StreamingAssets` folder.</param>
        /// <returns>Absolute path to file or `null` if the file cannot be found.</returns>
        public static async Task<string?> StreamingAssetsToAbsolutePath (string relativePath) {
            // Check persistent
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            // Handle other platform
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
            // Check persistent
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            // Create directories
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            // Download from APK/AAB
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                return null;
            // Copy
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            // Return
            return persistentPath;
        }

        /// <summary>
        /// Deconstruct feature normalization into mean and standard deviation vectors.
        /// </summary>
        public static void Deconstruct (this Normalization? norm, out Vector3 mean, out Vector3 std) {
            mean = norm?.mean.Length > 0 ? new Vector3(norm.mean[0], norm.mean[1], norm.mean[2]) : Vector3.zero;
            std = norm?.std.Length > 0 ? new Vector3(norm.std[0], norm.std[1], norm.std[2]) : Vector3.one;
        }

        /// <summary>
        /// Deconstruct an audio format into the sample rate and channel count.
        /// </summary>
        public static void Deconstruct (this AudioFormat? format, out int sampleRate, out int channelCount) {
            sampleRate = format?.sampleRate ?? 0;
            channelCount = format?.channelCount ?? 0;
        }
    }
}