/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Types {

    /// <summary>
    /// ML text feature type.
    /// </summary>
    public sealed class MLTextType : MLFeatureType {

        #region --Client API--
        /// <summary>
        /// Text length.
        /// </summary>
        public readonly int length;

        /// <summary>
        /// Create a text feature type.
        /// </summary>
        /// <param name="length">Text length.</param>
        public MLTextType (int length) : this(null, length) { }

        /// <summary>
        /// Create a text feature type.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <param name="length">Text length.</param>
        public MLTextType (string name, int length) : base(name, typeof(string)) => this.length = length;
        #endregion
    }
}