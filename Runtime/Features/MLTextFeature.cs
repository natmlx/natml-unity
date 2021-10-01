/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using Types;

    /// <summary>
    /// ML text feature.
    /// This feature will mainly be used with natural language processing models.
    /// </summary>
    public sealed class MLTextFeature : MLFeature {

        #region --Client API--
        /// <summary>
        /// Feature text.
        /// </summary>
        public readonly string text;

        /// <summary>
        /// Create the text feature.
        /// </summary>
        /// <param name="text">Text.</param>
        public MLTextFeature (string text) : base(new MLArrayType(typeof(string), new [] { 1, text.Length })) => this.text = text;
        #endregion
    }
}