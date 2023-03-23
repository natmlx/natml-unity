/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Graph {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// NatML Graph API client.
    /// </summary>
    public interface IGraphClient {

        /// <summary>
        /// Query the NatML Graph API.
        /// </summary>
        /// <param name="query">Graph query.</param>
        /// <param name="key">Query result key.</param>
        /// <param name="input">Query inputs.</param>
        Task<T?> Query<T> (string query, string key, Dictionary<string, object?>? variables = default);

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="url">URL</param>
        Task<MemoryStream> Download (string url);

        /// <summary>
        /// Upload a data stream.
        /// </summary>
        /// <param name="stream">Data stream.</param>
        /// <param name="url">Upload URL.</param>
        /// <param name="mime">MIME type.</param>
        Task Upload (MemoryStream stream, string url, string? mime = null);
    }

    /// <summary>
    /// NatML Graph API request.
    /// </summary>
    public sealed class GraphRequest {

        public string query = string.Empty;
        public Dictionary<string, object?>? variables;
    }

    /// <summary>
    /// NatML Graph API response.
    /// </summary>
    public sealed class GraphResponse<T> {

        public Dictionary<string, T>? data;
        public Error[]? errors;

        public sealed class Error {
            public string message;
        }
    }

    /// <summary>
    /// NatML Graph API enumeration.
    /// </summary>
    public sealed class GraphEnum<T> : JsonConverter where T : struct, Enum {

        private readonly bool underscores;

        public GraphEnum () : this(false) { }

        public GraphEnum (bool underscores) => this.underscores = underscores;

        public override void WriteJson (
            JsonWriter writer,
            object? value,
            JsonSerializer serializer
        ) {
            // Check
            if (value == null) {
                writer.WriteValue(null as string);
                return;
            }
            // Insert underscores
            var boundary = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])");
            var valueStr = underscores ? boundary.Replace(value.ToString(), "_") : value.ToString();
            // Serialize
            var result = valueStr.ToUpper();
            writer.WriteValue(result);
        }

        public override object ReadJson (
            JsonReader reader,
            Type @type,
            object? existingValue,
            JsonSerializer serializer
        ) {
            var value = (string)reader.Value;
            if (value == null)
                return (T)(object)0;
            return Enum.Parse<T>(value.Replace("_", ""), true);
        }

        public override bool CanConvert(Type @type) => @type == typeof(string);
    }
}