/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Types;
    using Internal;

    /// <summary>
    /// ML array feature.
    /// </summary>
    public sealed unsafe class MLArrayFeature<T> : MLFeature, IMLFeature where T : unmanaged {

        #region --Client API--
        /// <summary>
        /// Feature shape.
        /// </summary>
        public int[] shape => (type as MLArrayType).shape;

        /// <summary>
        /// Feature element count.
        /// </summary>
        public int elementCount => (type as MLArrayType).elementCount;

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [params int[] idx] {
            get {
                var linearIdx = Flatten(idx);
                return nativeData != null ? nativeData[linearIdx] : data[linearIdx];
            }
        }

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
        public MLArrayFeature (T[] data, MLFeatureType type) : base(type) {
            this.data = data;
            this.strides = ComputeStrides(shape);
        }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature shape.</param>
        public unsafe MLArrayFeature (T* data, MLFeatureType type) : base(type) {
            this.nativeData = data;
            this.strides = ComputeStrides(shape);
        }

        /// <summary>
        /// Create an array feature from a native feature.
        /// Note that this does NOT take ownership of the native feature.
        /// As such the native feature must be explicitly released by the client.
        /// </summary>
        /// <param name="nativeFeature">Backing native feature. This MUST be an array feature.</param>
        public unsafe MLArrayFeature (IntPtr nativeFeature) : this((T*)nativeFeature.FeatureData(), FeatureShape(nativeFeature)) { }

        /// <summary>
        /// Copy feature data to a buffer.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="startIndex">Start index to copy into destination array.</param>
        /// <param name="length">Number of elements to copy. Pass `0` to use destination array length.</param>
        public unsafe void CopyTo (T[] array, int startIndex = 0, int length = 0) {
            fixed (T* buffer = &array[startIndex])
                CopyTo(buffer, length > 0 ? length : array.Length);
        }

        /// <summary>
        /// Copy feature data to a buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="length">Number of elements to copy.</param>
        public unsafe void CopyTo (T* buffer, int length) {
            if (nativeData != null)
                Buffer.MemoryCopy(nativeData, buffer, length * sizeof(T), length * sizeof(T));
            else
                fixed (T* src = data) // `MemoryCopy` does `memmove` whereas `BlockCopy` seems to do more.
                    Buffer.MemoryCopy(src, buffer, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Permute the dimensions of this array feature.
        /// This operation is a generalization of the transpose operation.
        /// </summary>
        /// <param name="dims">Permuted dimensions.</param>
        /// <returns>Array feature with permuted dimensions.</returns>
        public unsafe MLArrayFeature<T> Permute (params int[] dims) {
            var newShape = dims.Select(d => shape[d]).ToArray();
            var newStrides = dims.Select(d => strides[d]).ToArray();
            var result = View(newShape);
            Array.Copy(newStrides, result.strides, newStrides.Length);
            return result;
        }

        /// <summary>
        /// Create a view of this array feature with a different shape.
        /// The element count of the new shape MUST match that of the feature.
        /// </summary>
        /// <param name="shape">New shape.</param>
        /// <returns>Array feature with new shape.</returns>
        public unsafe MLArrayFeature<T> View (params int[] shape) {
            // Check
            var elementCount = shape.Aggregate(1, (a, b) => a * b);
            if (elementCount != this.elementCount)
                throw new ArgumentOutOfRangeException(
                    nameof(shape),
                    "Array feature shape ("+string.Join(",", this.shape)+") cannot be viewed as ("+string.Join(",", shape)+")"
                );
            // View
            if (nativeData != null)
                return new MLArrayFeature<T>(nativeData, shape);
            else
                return new MLArrayFeature<T>(data, shape);
        }
        #endregion


        #region --Operations--
        private readonly T[] data;
        private readonly T* nativeData;
        private readonly int[] strides;

        unsafe IntPtr IMLFeature.Create (in MLFeatureType type) {
            // Check types
            var featureType = type as MLArrayType;
            var bufferType = this.type as MLArrayType;
            if (featureType.dataType != bufferType.dataType)
                throw new ArgumentException($"MLModel expects {featureType.dataType} feature but was given {bufferType.dataType} feature");
            // Create feature
            var shape = bufferType.shape ?? featureType.shape;
            if (nativeData != null)
                return Create(nativeData, shape);
            if (data != null)
                fixed (void* baseAddress = data)
                    return Create(baseAddress, shape);
            return IntPtr.Zero;
        }

        private unsafe IntPtr Create (void* data, int[] shape) {
            // Check that shape is fully specified
            var dynamicAxis = Array.IndexOf(shape, -1);
            if (dynamicAxis != -1)
                throw new ArgumentException($"Array feature shape has unspecified size at dimension {dynamicAxis}");
            // Create
            NatML.CreateFeature(
                data,
                shape,
                shape.Length,
                type.dataType.NativeType(),
                0,
                out var feature
            );
            return feature;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Flatten (int[] idx) {
            // Check linear
            if (idx.Length == 1)
                return idx[0];
            // Check shape
            if (idx.Length != strides?.Length)
                throw new ArgumentException(@"Index dimensions must match shape dimensions", nameof(idx));
            // Flatten
            var result = 0;
            for (var i = 0; i < strides.Length; ++i)
                result += idx[i] * strides[i];
            return result;
        }

        private static int[] ComputeStrides (int[] shape) {
            // Check
            if (shape == null)
                return null;
            // Compute
            var result = new int[shape.Length];
            result[shape.Length - 1] = 1;
            for (var i = shape.Length - 2; i >= 0; --i)
                result[i] = shape[i + 1] * result[i + 1];
            return result;
        }

        private static int[] FeatureShape (IntPtr feature) {
            feature.FeatureType(out var type);
            var shape = new int[type.FeatureTypeDimensions()];
            type.FeatureTypeShape(shape, shape.Length);
            type.ReleaseFeatureType();
            return shape;
        }
        #endregion
    }
}