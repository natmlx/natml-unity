/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML {

    using System;

    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    internal sealed class MLModelDataEmbedAttribute : Attribute {

        #region --Client API--
        /// <summary>
        /// </summary>
        /// <param name="tag"></param>
        public MLModelDataEmbedAttribute (string tag) => this.tag = tag;
        #endregion


        #region --Operations--
        internal readonly string tag;
        #endregion
    }
}