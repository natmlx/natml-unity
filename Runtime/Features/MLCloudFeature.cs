/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.Features {

    using System;
    using System.IO;
    using API.Types;

    /// <summary>
    /// Feature used for making cloud predictions.
    /// </summary>
    public readonly struct MLCloudFeature : IDisposable {

        #region --Client API--
        /// <summary>
        /// Feature data.
        /// </summary>
        public readonly MemoryStream data;

        /// <summary>
        /// Feature type.
        /// </summary>
        public readonly Dtype type;

        /// <summary>
        /// Feature shape.
        /// This MUST be populated for array features.
        /// </summary>
        public readonly int[]? shape;

        /// <summary>
        /// Dispose the feature and release resources.
        /// </summary>
        public readonly void Dispose () => data.Dispose();

        /// <summary>
        /// Create a cloud feature.
        /// </summary>
        /// <param name="data">Feature data stream.</param>
        /// <param name="type">Feature data type.</param>
        /// <param name="shape">Feature shape. This is only used for array features.</param>
        public MLCloudFeature (MemoryStream data, Dtype type, int[]? shape = null) {
            this.data = data;
            this.type = type;
            this.shape = shape;
        }
        #endregion
    }
}