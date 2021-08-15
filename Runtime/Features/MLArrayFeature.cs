/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using Types;
    using Internal;

    /// <summary>
    /// ML array feature.
    /// </summary>
    public sealed class MLArrayFeature<T> : MLFeature, IMLFeature where T : unmanaged {

        #region --Client API--
        /// <summary>
        /// Feature data.
        /// </summary>
        public readonly T[] data;

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        public MLArrayFeature (T[] data) : this(data, null as int[]) { }
        
        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        public unsafe MLArrayFeature (T* data) : this(data, null as int[]) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public MLArrayFeature (T[] data, int[] shape) : this(data, new MLArrayType(null, typeof(T), shape)) { }
        
        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public unsafe MLArrayFeature (T* data, int[] shape) : this(data, new MLArrayType(null, typeof(T), shape)) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature type.</param>
        public MLArrayFeature (T[] data, MLFeatureType type) : base(type) => this.data = data;

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature shape.</param>
        public unsafe MLArrayFeature (T* data, MLFeatureType type) : base(type) => this.nativeBuffer = (IntPtr)data;
        #endregion


        #region --Operations--
        private readonly IntPtr nativeBuffer;

        unsafe IntPtr IMLFeature.Create (in MLFeatureType type) {
            // Check types
            var featureType = type as MLArrayType;
            var bufferType = this.type as MLArrayType;
            if (featureType.dataType != bufferType.dataType)
                throw new ArgumentException($"MLModel expects {featureType.dataType} feature but was given {bufferType.dataType} feature");
            // Create feature
            var shape = bufferType.shape ?? featureType.shape;
            if (data != null)
                fixed (void* baseAddress = data)
                    return Create(baseAddress, shape);
            if (nativeBuffer != IntPtr.Zero)
                return Create((void*)nativeBuffer, shape);
            return IntPtr.Zero;
        }

        private unsafe IntPtr Create (void* data, int[] shape) {
            // Check that shape is fully specified
            var dynamicAxis = Array.IndexOf(shape, -1);
            if (dynamicAxis != -1)
                throw new ArgumentException($"Array feature shape has unspecified size at dimension {dynamicAxis}");
            // Create
            Bridge.CreateFeature(
                data,
                shape,
                shape.Length,
                type.dataType.NativeType(),
                0,
                out var feature
            );
            return feature;
        }
        #endregion
    }
}