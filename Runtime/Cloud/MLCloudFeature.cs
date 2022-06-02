/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;
    using System.IO;

    /// <summary>
    /// Feature used for making Cloud predictions.
    /// </summary>
    public sealed class MLCloudFeature : IDisposable {

        #region --Client API--
        /// <summary>
        /// Feature type.
        /// Must be one of the `DataType` constants.
        /// </summary>
        public string type;

        /// <summary>
        /// Feature data.
        /// </summary>
        public Stream data;

        /// <summary>
        /// Feature shape.
        /// This MUST be populated for array features.
        /// </summary>
        public int[] shape;

        /// <summary>
        /// Dispose the feature and release resources.
        /// </summary>
        public void Dispose () => data.Dispose();
        #endregion
    }
}