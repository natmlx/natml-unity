/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Hub.Types;

    /// <summary>
    /// Utilities for working with predictor endpoints.
    /// </summary>
    public static class MLEndpointUtils {

        #region --Client API--
        /// <summary>
        /// Create a NatML API feature from a cloud feature.
        /// </summary>
        /// <param name="feature">Cloud feature.</param>
        /// <param name="name">Feature name.</param>
        /// <param name="dataLimit">Maximum size in bytes of feature stream for an upload URL to be created.</param>
        /// <returns>NatML API feature.</returns>
        public static async Task<FeatureInput> ToFeatureInput (this MLCloudFeature feature, string name, int dataLimit = 1024 * 1024) => new FeatureInput {
            name = name,
            data = await CreateURL(feature, dataLimit),
            type = feature.type,
            shape = feature.shape
        };

        /// <summary>
        /// Create a cloud feature from a NatML API feature.
        /// </summary>
        /// <param name="feature">NatML API feature.</param>
        /// <returns>Cloud feature.</returns>
        public static async Task<MLCloudFeature> ToFeature (this Feature feature) => new MLCloudFeature(
            await CreateStream(feature),
            feature.type,
            feature.shape
        );

        /// <summary>
        /// Create a feature data URL.
        /// </summary>
        /// <param name="feature">Cloud feature.</param>
        /// <param name="dataLimit">Maximum size in bytes of feature stream for an upload URL to be created.</param>
        /// <returns>Feature data URL.</returns>
        public static async Task<string> CreateURL (
            this MLCloudFeature feature,
            int dataLimit = 1024
        ) => feature.data.Length <= dataLimit ? CreateDataURL(feature) : await CreateUploadURL(feature);

        /// <summary>
        /// Create a feature data stream.
        /// </summary>
        /// <param name="feature">NatML API feature.</param>
        /// <returns>Feature data stream.</returns>
        public static Task<MemoryStream> CreateStream (this Feature feature) => CreateStream(feature.data);

        /// <summary>
        /// Create a data stream.
        /// </summary>
        /// <param name="url">Data URL.</param>
        /// <returns>Data stream.</returns>
        public static async Task<MemoryStream> CreateStream (this string url) => url.StartsWith(@"data:") ? url.DataStream() : await url.DownloadStream();

        /// <summary>
        /// Get the MIME type for a data stream with the given type.
        /// </summary>
        /// <param name="dtype">Stream data type.</param>
        /// <returns>MIME type.</returns>
        public static string ToMime (this string dtype) => dtype switch {
            Dtype.String    => @"text/plain",
            Dtype.Image     => @"image/jpeg",
            _               => @"application/octet-stream"
        };
        #endregion


        #region --Operations--

        private static string CreateDataURL (MLCloudFeature feature) {
            var data = Convert.ToBase64String(feature.data.ToArray());
            var mime = feature.type.ToMime();
            var result = $"data:{mime};base64,{data}";
            return result;
        }

        private static Task<string> CreateUploadURL (MLCloudFeature feature) { // INCOMPLETE // WebGL support
            throw new NotImplementedException();
        }

        private static MemoryStream DataStream (this string url) {
            var dataIdx = url.LastIndexOf(",") + 1;
            var b64Data = url.Substring(dataIdx);
            var data = Convert.FromBase64String(b64Data);
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            return stream;
        }

        private static Task<MemoryStream> DownloadStream (this string url) => Application.platform == RuntimePlatform.WebGLPlayer ? DownloadStreamUnity(url) : DownloadStreamNet(url);

        private static async Task<MemoryStream> DownloadStreamNet (string url) {
            using var client = new HttpClient();
            using var dataStream = await client.GetStreamAsync(url);
            using var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();
            return new MemoryStream(data, 0, data.Length, false, false);
        }

        private static async Task<MemoryStream> DownloadStreamUnity (string url) {
            using var request = UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException(request.error);
            var data = request.downloadHandler.data;
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            return stream;
        }
        #endregion
    }
}