/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Types {

    /// <summary>
    /// ML text feature type.
    /// </summary>
    public class MLTextType : MLFeatureType {

        #region --Client API--
        /// <summary>
        /// Text length.
        /// </summary>
        public virtual int length { get; protected set; }

        /// <summary>
        /// Create a text feature type.
        /// </summary>
        /// <param name="length">Text length.</param>
        /// <param name="name">Feature name.</param>
        public MLTextType (int length, string name = null) : base(name, typeof(string)) => this.length = length;
        #endregion
    }
}