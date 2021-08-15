/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Types {

    using System;

    /// <summary>
    /// ML dictionary feature type.
    /// </summary>
    internal sealed class MLDictionaryType : MLFeatureType { // INCOMPLETE // Internal for now

        #region --Client API--
        /// <summary>
        /// Dictionary key data type.
        /// </summary>
        public Type key => dataType;

        /// <summary>
        /// Dictionary value type.
        /// </summary>
        public readonly MLFeatureType value;

        /// <summary>
        /// Create a dictionary feature type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public MLDictionaryType (string name, Type type, MLFeatureType value) : base(name, type) => this.value = value;
        #endregion


        #region --Operations--

        #endregion
    }
}