/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Types {

    using System;

    /// <summary>
    /// ML sequence feature type.
    /// </summary>
    internal sealed class MLSequenceType : MLFeatureType { // Internal for now

        #region --Client API--
        /// <summary>
        /// Sequence element feature.
        /// </summary>
        public readonly MLFeatureType element;

        /// <summary>
        /// Create a sequence feature type.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <param name="type">Sequence element data type.</param>
        public MLSequenceType (string name, Type type) : base(name, type) {
            
        }
        #endregion

        #region --Operations--

        #endregion
    }
}