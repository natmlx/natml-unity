/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using Hub;
    using Internal;
    using Types;

    /// <summary>
    /// ML text feature.
    /// This feature will mainly be used with natural language processing models.
    /// </summary>
    public sealed class MLTextFeature : MLFeature, IMLHubFeature {

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
        /// Create the text feature from a Hub feature.
        /// </summary>
        /// <param name="hubFeature">Hub feature. This MUST be an `STRING` feature.</param>
        public MLTextFeature (MLHubFeature hubFeature) : base(new MLTextType(hubFeature.data.Length)) {
            // Check
            if (hubFeature.type != HubDataType.String)
                throw new ArgumentException(@"Hub feature is not a text feature", nameof(hubFeature));
            // Deserialize
            this.text = hubFeature.data;
        }
        #endregion


        #region --Operations--

        unsafe MLHubFeature IMLHubFeature.Serialize () => new MLHubFeature {
            data = text,
            type = HubDataType.String
        };

        public static implicit operator string (MLTextFeature feature) => feature.text;
        #endregion
    }
}