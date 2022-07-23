/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;

    /// <summary>
    /// Feature used for making Edge predictions.
    /// </summary>
    public unsafe readonly struct MLEdgeFeature : IDisposable {

        #region --Client API--
        /// <summary>
        /// Feature data.
        /// </summary>
        public readonly void* data => (void*)feature.FeatureData();

        /// <summary>
        /// Feature shape.
        /// </summary>
        public readonly int[] shape {
            get {
                feature.FeatureType(out var type);
                var shape = new int[type.FeatureTypeDimensions()];
                type.FeatureTypeShape(shape, shape.Length);
                type.ReleaseFeatureType();
                return shape;
            }
        }

        /// <summary>
        /// Feature data type
        /// </summary>
        public readonly Type dataType {
            get {
                feature.FeatureType(out var type);
                var result = type.FeatureTypeDataType();
                type.ReleaseFeatureType();
                return MLEdgeModel.ManagedType(result);
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

        public static implicit operator IntPtr (MLEdgeFeature feature) => feature.feature;
        #endregion
    }
}