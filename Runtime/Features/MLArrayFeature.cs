/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using Types;

    /// <summary>
    /// ML array feature.
    /// The array feature represents generic numeric tensor data.
    /// The array feature never owns or allocates its own memory. Instead, it provides a "view" into existing data.
    /// </summary>
    public sealed unsafe class MLArrayFeature<T> : MLFeature, IMLEdgeFeature where T : unmanaged {
    
        #region --Inspection--
        /// <summary>
        /// Feature shape.
        /// </summary>
        public int[] shape => (type as MLArrayType).shape;

        /// <summary>
        /// Feature element count.
        /// </summary>
        public int elementCount => (type as MLArrayType).elementCount;
        #endregion


        #region --Indexing--
        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Linear index.</param>
        public T this [in int idx] {
            get => nativeData != null ? nativeData[idx] : array[idx];
            set {
                if (nativeData != null)
                    nativeData[idx] = value;
                else
                    array[idx] = value;
            }
        }

        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1] {
            get => this[idx0 * strides[0] + idx1 * strides[1]];
            set => this[idx0 * strides[0] + idx1 * strides[1]] = value;
        }

        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2] {
            get => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2]];
            set => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2]] = value;
        }

        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2, in int idx3] {
            get => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3]];
            set => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3]] = value;
        }

        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2, in int idx3, in int idx4] {
            get => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3] + idx4 * strides[4]];
            set => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3] + idx4 * strides[4]] = value;
        }

        /// <summary>
        /// Get or set a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [params int[] idx] {
            get {
                var linearIdx = 0;
                for (var i = 0; i < strides.Length; ++i)
                    linearIdx += idx[i] * strides[i];
                return this[linearIdx];
            }
            set {
                var linearIdx = 0;
                for (var i = 0; i < strides.Length; ++i)
                    linearIdx += idx[i] * strides[i];
                this[linearIdx] = value;
            }
        }
        #endregion


        #region --Constructors--
        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        public MLArrayFeature (T[] data) : this(data, null as int[]) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        public unsafe MLArrayFeature (NativeArray<T> data) : this(data, null as int[]) { }
        
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
        public MLArrayFeature (T[] data, int[] shape) : this(data, new MLArrayType(shape, typeof(T))) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public MLArrayFeature (NativeArray<T> data, int[] shape) : this(data, new MLArrayType(shape, typeof(T))) { }
        
        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public unsafe MLArrayFeature (T* data, int[] shape) : this(data, new MLArrayType(shape, typeof(T))) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature type.</param>
        public MLArrayFeature (T[] data, MLArrayType type) : base(type) {
            this.array = data;
            this.strides = ComputeStrides(shape);
        }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature type.</param>
        public unsafe MLArrayFeature (NativeArray<T> data, MLArrayType type) : this((T*)data.GetUnsafePtr(), type) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="type">Feature type.</param>
        public unsafe MLArrayFeature (T* data, MLArrayType type) : base(type) {
            this.nativeData = data;
            this.strides = ComputeStrides(shape);
        }

        /// <summary>
        /// Create an array feature from an Edge ML feature.
        /// Note that this does NOT take ownership of the Edge ML feature.
        /// As such the Edge ML feature MUST be disposed by the client.
        /// </summary>
        /// <param name="feature">Edge feature. This MUST be an array feature.</param>
        public MLArrayFeature (MLEdgeFeature feature) : this((T*)feature.data, feature.shape) { }
        #endregion


        #region --Viewing--
        /// <summary>
        /// Flatten this array feature into a one-dimensional feature.
        /// </summary>
        /// <param name="startDim">Optional starting dimension to flatten.</param>
        /// <param name="endDim">Optional ending dimension to flatten.</param>
        /// <returns></returns>
        public unsafe MLArrayFeature<T> Flatten (int startDim = 0, int endDim = -1) {
            endDim = endDim < 0 ? shape.Length - 1 : endDim;
            var flatDim = shape.Skip(startDim).Take(endDim - startDim + 1).Aggregate(1, (a, b) => a * b);
            var newShape = new List<int>();
            newShape.AddRange(shape.Take(startDim));
            newShape.Add(flatDim);
            newShape.AddRange(shape.Skip(endDim + 1));
            return View(newShape.ToArray());
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
        /// Remove all dimensions of size 1.
        /// </summary>
        /// <param name="dim">Optional index to squeeze.</param>
        /// <returns>Array feature with singleton dimensions removed.</returns>
        public unsafe MLArrayFeature<T> Squeeze (int dim = -1) {
            var newShape = new List<int>(shape);
            if (dim < 0)
                newShape.RemoveAll(s => s == 1);
            else if (newShape[dim] == 1)
                newShape.RemoveAt(dim);
            return View(newShape.ToArray());
        }

        /// <summary>
        /// Create a view of this array feature with a different shape.
        /// The element count of the new shape MUST match that of the feature.
        /// Any single axis can be `-1`, in which case its value is dynamically calculated.
        /// </summary>
        /// <param name="shape">New shape.</param>
        /// <returns>Array feature with new shape.</returns>
        public unsafe MLArrayFeature<T> View (params int[] shape) {
            // Fill
            var dynamicAxis = Array.IndexOf(shape, -1);
            if (dynamicAxis >= 0) {
                var staticAxisElementCount = shape
                    .Where((s, i) => i != dynamicAxis)
                    .Aggregate(1, (a, b) => a * b);
                shape[dynamicAxis] = this.elementCount / staticAxisElementCount;
            }
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
                return new MLArrayFeature<T>(array, shape);
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Copy feature data to an array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="startIndex">Start index to copy into destination array.</param>
        /// <param name="length">Number of elements to copy. Pass `0` to use destination array length.</param>
        public unsafe void CopyTo (T[] array, int startIndex = 0, int length = 0) {
            fixed (T* buffer = &array[startIndex])
                CopyTo(buffer, length > 0 ? length : array.Length);
        }

        /// <summary>
        /// Copy feature data to a native array.
        /// </summary>
        /// <param name="array">Destination native array.</param>
        /// <param name="startIndex">Start index to copy into destination array.</param>
        /// <param name="length">Number of elements to copy. Pass `0` to use destination array length.</param>
        public unsafe void CopyTo (NativeArray<T> array, int startIndex = 0, int length = 0) {
            T* buffer = &((T*)array.GetUnsafeReadOnlyPtr())[startIndex];
            CopyTo(buffer, length > 0 ? length : array.Length);
        }

        /// <summary>
        /// Copy feature data to a buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="length">Number of elements to copy.</param>
        public unsafe void CopyTo (T* buffer, int length) { // CHECK // Does not support permuted tensor
            if (nativeData != null)
                Buffer.MemoryCopy(nativeData, buffer, length * sizeof(T), length * sizeof(T));
            else
                fixed (T* src = array) // `MemoryCopy` does `memmove` whereas `BlockCopy` seems to do more.
                    Buffer.MemoryCopy(src, buffer, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Convert the array feature to a flattened array.
        /// This method always returns a copy of the array feature data.
        /// </summary>
        /// <returns>Result array.</returns>
        public unsafe T[] ToArray () {
            var result = new T[elementCount];
            CopyTo(result);
            return result;
        }
        #endregion


        #region --Operations--
        private readonly T[] array;
        private readonly T* nativeData;
        private readonly int[] strides;

        unsafe MLEdgeFeature IMLEdgeFeature.Create (MLFeatureType type) {
            // Check array type
            var arrayType = this.type as MLArrayType;
            var featureType = type as MLArrayType;
            if (featureType.dataType != arrayType.dataType)
                throw new ArgumentException($"Cannot create {featureType.dataType} feature with {arrayType.dataType} feature", nameof(type));
            // Check that shape is fully specified
            var shape = arrayType.shape ?? featureType.shape;
            var dynamicAxis = Array.IndexOf(shape, -1);
            if (dynamicAxis != -1)
                throw new ArgumentException($"Array feature shape has unspecified size at dimension {dynamicAxis}", nameof(type));
            // Create feature
            fixed (T* managedData = array) {
                var data = nativeData != null ? nativeData : managedData;
                NatML.CreateFeature(
                    data,
                    shape,
                    shape.Length,
                    EdgeType(type.dataType),
                    data == managedData ? 1 : 0,
                    out var feature
                );
                return new MLEdgeFeature(feature);
            }
        }

        private static int[] ComputeStrides (int[] shape) {
            // Check
            if (shape == null || shape.Length == 0)
                return null;
            // Compute
            var result = new int[shape.Length];
            result[shape.Length - 1] = 1;
            for (var i = shape.Length - 2; i >= 0; --i)
                result[i] = shape[i + 1] * result[i + 1];
            return result;
        }
        #endregion
    }
}