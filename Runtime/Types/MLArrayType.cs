/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Types {

    using System;
    using System.Linq;

    /// <summary>
    /// ML array feature type.
    /// </summary>
    public class MLArrayType : MLFeatureType {

        #region --Client API--
        /// <summary>
        /// Array shape.
        /// Note that this can be `null` when array features are created without a shape.
        /// </summary>
        public readonly int[] shape;

        /// <summary>
        /// Array dimensions.
        /// </summary>
        public int dims => shape?.Length ?? 0;

        /// <summary>
        /// Array element count.
        /// </summary>
        public int elementCount => shape?.Aggregate(1, (a, b) => a * b) ?? 0;

        /// <summary>
        /// Create an array feature type.
        /// </summary>
        /// <param name="shape">Array feature shape.</param>
        /// <param name="type">Array element type.</param>
        /// <param name="name">Feature name.</param>
        public MLArrayType (int[] shape, Type type, string name = null) : base(name, type) => this.shape = shape;
        #endregion


        #region --Operations--

        public override string ToString () {
            var nameStr = name != null ? $"{name}: " : string.Empty;
            var shape = this.shape ?? new int[0];
            var shapeStr = $"({string.Join(", ", shape)})";
            return $"{nameStr}{shapeStr} {dataType}";
        }
        #endregion
    }
}