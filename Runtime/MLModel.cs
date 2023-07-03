/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ML model.
    /// </summary>
    public abstract class MLModel : IDisposable {

        #region --Client API--
        /// <summary>
        /// Input feature types.
        /// </summary>
        public MLFeatureType[] inputs { get; protected set; }

        /// <summary>
        /// Output feature types.
        /// </summary>
        public MLFeatureType[] outputs { get; protected set; }

        /// <summary>
        /// Metadata dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, string> metadata { get; protected set; }

        /// <summary>
        /// Dispose the model and release resources.
        /// </summary>
        public virtual void Dispose () { }
        #endregion
    }
}