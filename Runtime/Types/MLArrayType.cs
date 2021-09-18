/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Types {

    using System;
    using System.Linq;

    /// <summary>
    /// ML array feature type.
    /// </summary>
    public class MLArrayType : MLFeatureType {

        #region --Client API--
        /// <summary>
        /// Array shape.
        /// </summary>
        public readonly int[] shape; // Can be `null`

        /// <summary>
        /// Array dimensions.
        /// </summary>
        public int dims => shape?.Length ?? 0; // Mark `readonly` in C# 8

        /// <summary>
        /// Array element count.
        /// </summary>
        public int elementCount => shape?.Aggregate(1, (a, b) => a * b) ?? 0; // Mark `readonly` in C# 8

        /// <summary>
        /// Create an array feature type.
        /// </summary>
        /// <param name="type">Array element type.</param>
        /// <param name="shape">Array feature shape.</param>
        public MLArrayType (Type type, int[] shape) : this(default, type, shape) { }

        /// <summary>
        /// Create an array feature type.
        /// </summary>
        /// <param name="name">Feature name.</param>
        /// <param name="type">Array element type.</param>
        /// <param name="shape">Array feature shape.</param>
        public MLArrayType (string name, Type type, int[] shape) : base(name, type) => this.shape = shape;
        #endregion


        #region --Operations--

        public override string ToString () {
            var nameStr = name != null ? $"{name}: " : string.Empty;
            var shapeStr = shape != null ? $"({string.Join(", ", shape)})" : "<shapeless>";
            return $"{nameStr}{shapeStr} {dataType}";
        }
        #endregion
    }
}