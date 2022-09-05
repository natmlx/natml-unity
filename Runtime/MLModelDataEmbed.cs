/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;

    /// <summary>
    /// Embed ML model data from NatML at build time so that the model data is available without downloading at runtime.
    /// This is useful when building applications that might not have internet access.
    /// Note that the build size of the application will increase as a result of the embedded model data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class MLModelDataEmbedAttribute : Attribute {

        #region --Client API--
        /// <summary>
        /// Embed ML model data from NatML.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key. If `null` the project access key will be used.</param>
        public MLModelDataEmbedAttribute (string tag, string accessKey = null) {
            this.tag = tag;
            this.accessKey = accessKey;
        }
        #endregion


        #region --Operations--
        internal readonly string tag;
        internal readonly string accessKey;
        #endregion
    }
}