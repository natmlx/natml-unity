/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;

    /// <summary>
    /// Feature used for making Edge predictions.
    /// </summary>
    public sealed unsafe class MLEdgeFeature : IDisposable {

        #region --Client API--
        /// <summary>
        /// Feature data.
        /// </summary>
        public void* data => (void*)feature.FeatureData();

        /// <summary>
        /// Feature shape.
        /// </summary>
        public int[] shape {
            get {
                feature.FeatureType(out var type);
                var shape = new int[type.FeatureTypeDimensions()];
                type.FeatureTypeShape(shape, shape.Length);
                type.ReleaseFeatureType();
                return shape;
            }
        }

        /// <summary>
        /// Dispose the feature and release resources.
        /// </summary>
        public void Dispose () => feature.ReleaseFeature();
        #endregion


        #region --Operations--
        private readonly IntPtr feature;

        public MLEdgeFeature (IntPtr feature) => this.feature = feature;

        public static implicit operator IntPtr (MLEdgeFeature feature) => feature?.feature ?? IntPtr.Zero;
        #endregion


        #region --DEPRECATED--
        [Obsolete(@"Deprecated in NatML 1.0.9. Use `Dispose` instead.", false)]
        public void ReleaseFeature () => Dispose();
        #endregion
    }
}