/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Graph {

    using System;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// NatML Graph API enumeration.
    /// </summary>
    public sealed class GraphEnum<T> : JsonConverter where T : struct, Enum {

        private readonly GraphEnumOptions options;

        public GraphEnum () : this(GraphEnumOptions.None) { }

        public GraphEnum (GraphEnumOptions options) => this.options = options;

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
            var valueStr = options.HasFlag(GraphEnumOptions.Underscores) ? boundary.Replace(value.ToString(), "_") : value.ToString();
            // Serialize
            var result = options.HasFlag(GraphEnumOptions.Lowercase) ? valueStr.ToLower() : valueStr.ToUpper();
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

    /// <summary>
    /// Graph enumeration options.
    /// </summary>
    [Flags]
    public enum GraphEnumOptions {
        /// <summary>
        /// No options.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enumeration members use underscores at word boundaries.
        /// </summary>
        Underscores = 1,
        /// <summary>
        /// Enumeration members are lowercase.
        /// </summary>
        Lowercase = 2,
    }
}