/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.Features {

    using System;
    using API.Types;
    using Internal;

    /// <summary>
    /// Feature used for making edge predictions.
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
        public readonly Dtype dataType {
            get {
                feature.FeatureType(out var type);
                var result = type.FeatureTypeDataType();
                type.ReleaseFeatureType();
                return result;
            }
        }

        /// <summary>
        /// Dispose the feature and release resources.
        /// </summary>
        public readonly void Dispose () => feature.ReleaseFeature();
        #endregion


        #region --Operations--
        private readonly IntPtr feature;

        public MLEdgeFeature (IntPtr feature) => this.feature = feature;

        public static implicit operator IntPtr (MLEdgeFeature feature) => feature.feature;
        #endregion
    }
}