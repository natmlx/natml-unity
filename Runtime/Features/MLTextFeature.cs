/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.IO;
    using System.Text;
    using Hub;
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
        /// <param name="text">Text.</param>
        public MLTextFeature (string text) : base(new MLTextType(text.Length)) => this.text = text;

        /// <summary>
        /// Create the text feature from a Cloud ML feature.
        /// </summary>
        /// <param name="feature">Cloud ML feature. This MUST be an `STRING` feature.</param>
        public MLTextFeature (MLCloudFeature feature) : base(new MLTextType(feature.shape[1])) {
            // Check
            if (feature.type != DataType.String)
                throw new ArgumentException(@"Cloud ML feature is not a text feature", nameof(feature));
            // Deserialize
            using var reader = new StreamReader(feature.data);
            this.text = reader.ReadToEnd();
        }
        #endregion


        #region --Operations--

        unsafe MLCloudFeature IMLCloudFeature.Create (in MLFeatureType _) => new MLCloudFeature {
            data = new MemoryStream(Encoding.UTF8.GetBytes(text), false),
            type = DataType.String
        };

        public static implicit operator string (MLTextFeature feature) => feature.text;
        #endregion
    }
}