/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
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
        /// Model input feature types.
        /// </summary>
        public MLFeatureType[] inputs { get; protected set; }

        /// <summary>
        /// Model output feature types.
        /// </summary>
        public MLFeatureType[] outputs { get; protected set; }

        /// <summary>
        /// Model metadata dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, string> metadata { get; protected set; }

        /// <summary>
        /// Dispose the model and release resources.
        /// </summary>
        public virtual void Dispose () { }
        #endregion


        #region --Operations--
        protected readonly MLModelData modelData;

        private protected MLModel (MLModelData modelData) => this.modelData = modelData;
        #endregion
    }
}