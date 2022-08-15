/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

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
        /// Create the text feature from plain text.
        /// </summary>
        /// <param name="text">Feature text.</param>
        public MLTextFeature (string text) {
            this.type = new MLTextType(text.Length);
            this.text = text;
        }
        #endregion


        #region --Operations--

        public static implicit operator string (MLTextFeature feature) => feature.text;
        #endregion
    }
}