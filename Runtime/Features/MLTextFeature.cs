/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

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
        /// Create the text feature.
        /// </summary>
        /// <param name="text">Text.</param>
        public MLTextFeature (string text) : base(new MLTextType(text.Length)) => this.text = text;
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