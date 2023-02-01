/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.IO;
    using System.Text;
    using Hub.Types;
    using Internal;
    using Types;

    /// <summary>
    /// ML text feature.
    /// This feature will mainly be used with natural language processing models.
    /// </summary>
    public sealed class MLTextFeature : MLFeature, IMLCloudFeature {

        #region --Client API--
        /// <summary>
        /// Feature text.
        /// </summary>
        public readonly string text;

        /// <summary>
        /// Create the text feature from plain text.
        /// </summary>
        /// <param name="text">Feature text.</param>
        public MLTextFeature (string text) : base(new MLTextType(text.Length)) => this.text = text;

        /// <summary>
        /// Create the text feature from a cloud feature.
        /// </summary>
        /// <param name="feature">Cloud feature. This MUST be a `string` feature.</param>
        public MLTextFeature (MLCloudFeature feature) : base(new MLTextType(feature.shape[1])) {
            // Check
            if (feature.type != Dtype.String)
                throw new ArgumentException(@"Cloud feature is not a text feature", nameof(feature));
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

        public static implicit operator string (MLTextFeature feature) => feature.text;
        #endregion
    }
}