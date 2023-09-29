/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System;
    using System.Diagnostics.CodeAnalysis;
    using API.Graph;

    /// <summary>
    /// NatML resource tag.
    /// </summary>
    [Preserve, Serializable]
    public sealed class Tag {

        #region --Client API--
        /// <summary>
        /// Resource owner username.
        /// </summary>
        public string username;

        /// <summary>
        /// Resource name.
        /// </summary>
        public string name;

        /// <summary>
        /// Resource variant.
        /// </summary>
        public string? variant;

        /// <summary>
        /// Create a tag.
        /// </summary>
        /// <param name="username">Owner username.</param>
        /// <param name="name">Resource name.</param>
        /// <param name="variant">Resource variabt.</param>
        public Tag (string username, string name, string? variant = null) {
            this.username = username;
            this.name = name;
            this.variant = variant;
        }

        /// <summary>
        /// Serialize the tag.
        /// </summary>
        public override string ToString () {
            var suffix = string.IsNullOrEmpty(variant) ? string.Empty : $"@{variant}";
            var result = $"@{username}/{name}{suffix}";
            return result;
        }

        /// <summary>
        /// Try to parse a predictor tag from a string.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="tag">Output tag.</param>
        /// <returns>Whether the tag was successfully parsed.</returns>
        public static bool TryParse (string input, [NotNullWhen(true)] out Tag? tag) {
            tag = null;
            // Check username prefix
            input = input.ToLowerInvariant();
            if (!input.StartsWith("@"))
                return false;
            // Check stem
            var stem = input.Split('/');
            var extendedName = stem[1].Split('@');
            if (stem.Length != 2)
                return false;
            // Parse
            var username = stem[0].Substring(1);
            var name = extendedName[0];
            var variant = extendedName.Length > 1 ? extendedName[1] : null;
            tag = new Tag(username, name, variant);
            // Return
            return true;
        }

        public static implicit operator string (Tag tag) => tag.ToString();
        #endregion

    }
}