/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Types {

    /// <summary>
    /// ML string feature type.
    /// </summary>
    public class MLStringType : MLFeatureType {

        #region --Client API--
        /// <summary>
        /// Text length.
        /// </summary>
        public virtual int length { get; protected set; }

        /// <summary>
        /// Create a string feature type.
        /// </summary>
        /// <param name="length">String length.</param>
        /// <param name="name">Feature name.</param>
        public MLStringType (int length, string name = null) : base(name, typeof(string)) => this.length = length;
        #endregion
    }
}