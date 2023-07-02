/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Graph {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// </summary>
    public sealed class DotNetClient : IGraphClient {

        #region --Client API--
        /// <summary>
        /// Create the .NET client.
        /// </summary>
        /// <param name="url">NatML Graph API URL.</param>
        /// <param name="accessKey">NatML access key.</param>
        public DotNetClient (string url, string accessKey) {
            this.url = url;
            this.accessKey = accessKey;
        }

        /// <summary>
        /// Query the NatML Graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        public async Task<T?> Query<T> (string query, string key, Dictionary<string, object?>? variables = default) {
            // Serialize payload
            var payload = new GraphRequest {
                query = query,
                variables = variables
            };
            var serializationSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };
            var payloadStr = JsonConvert.SerializeObject(payload, serializationSettings);
            // Create client
            using var client = new HttpClient();
            using var content = new StringContent(payloadStr, Encoding.UTF8, @"application/json");
            // Add auth token
            var authHeader = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            client.DefaultRequestHeaders.Authorization = authHeader;
            // Post
            using var response = await client.PostAsync(url, content);
            // Parse
            var responseStr = await response.Content.ReadAsStringAsync();
            var responsePayload = JsonConvert.DeserializeObject<GraphResponse<T>>(responseStr);
            // Check error
            if (responsePayload.errors != null)
                throw new InvalidOperationException(responsePayload.errors[0].message);
            // Return
            return responsePayload.data.TryGetValue(key, out var value) ? value : default;                
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">Data URL.</param>
        public async Task<MemoryStream> Download (string url) {
            using var client = new HttpClient();
            using var dataStream = await client.GetStreamAsync(url);
            using var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            return memoryStream;
        }

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        public async Task Upload (MemoryStream stream, string url, string? mime = null) {
            using var client = new HttpClient();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(mime ?? @"application/octet-stream");
            using var response = await client.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
        #endregion


        #region --Operations--
        private readonly string url;
        private readonly string accessKey;
        #endregion
    }
}