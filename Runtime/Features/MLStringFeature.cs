/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.IO;
    using System.Text;
    using API.Types;
    using Internal;
    using Types;

    /// <summary>
    /// ML string feature.
    /// This feature will mainly be used with natural language processing models.
    /// </summary>
    public sealed class MLStringFeature : MLFeature, IMLCloudFeature {

        #region --Client API--
        /// <summary>
        /// Feature text.
        /// </summary>
        public readonly string text;

        /// <summary>
        /// Create the string feature from plain text.
        /// </summary>
        /// <param name="text">Feature text.</param>
        public MLStringFeature (string text) : base(new MLStringType(text.Length)) => this.text = text;

        /// <summary>
        /// Create the string feature from a cloud feature.
        /// </summary>
        /// <param name="feature">Cloud feature. This MUST be a `string` feature.</param>
        public MLStringFeature (MLCloudFeature feature) : base(new MLStringType(feature.shape[1])) {
            // Check
            if (feature.type != Dtype.String)
                throw new ArgumentException(@"Cloud feature is not a string feature", nameof(feature));
            // Deserialize
            using var reader = new StreamReader(feature.data);
            this.text = reader.ReadToEnd();
        }
        #endregion


        #region --Operations--

        unsafe MLCloudFeature IMLCloudFeature.Create (MLFeatureType _) {
            var data = Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            var feature = new MLCloudFeature(stream, Dtype.String, new [] { 1, text.Length });
            return feature;
        }

        public static implicit operator string (MLStringFeature feature) => feature.text;
        #endregion
    }
}