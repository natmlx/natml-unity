/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML {

    using System;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// ML model.
    /// </summary>
    #pragma warning disable 0618
    public abstract class MLModel : IMLModel, IDisposable {
    #pragma warning restore 0618

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
        protected readonly string session;

        private protected MLModel (string session) => this.session = session;

        IntPtr[] IMLModel.Predict (params IntPtr[] inputs) => (this as MLEdgeModel).Predict(inputs);
        #endregion
    }
}