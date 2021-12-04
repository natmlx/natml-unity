/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Features {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;
    using Types;

    /// <summary>
    /// ML array feature.
    /// </summary>
    #pragma warning disable 0618
    public sealed unsafe class MLArrayFeature<T> : MLFeature, IMLEdgeFeature, IMLHubFeature, IMLFeature where T : unmanaged {
    #pragma warning restore 0618
    
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
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Linear index.</param>
        public T this [in int idx] => nativeData != null ? nativeData[idx] : array[idx];

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1] => this[idx0 * strides[0] + idx1 * strides[1]];

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2] => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2]];

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2, in int idx3] => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3]];

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [in int idx0, in int idx1, in int idx2, in int idx3, in int idx4] => this[idx0 * strides[0] + idx1 * strides[1] + idx2 * strides[2] + idx3 * strides[3] + idx4 * strides[4]];

        /// <summary>
        /// Get a value at a specified index.
        /// </summary>
        /// <param name="idx">Multi-index.</param>
        public T this [params int[] idx] {
            get {
                var linearIdx = 0;
                for (var i = 0; i < strides.Length; ++i)
                    linearIdx += idx[i] * strides[i];
                return this[linearIdx];
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
            this.array = data;
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
        /// </summary>
        /// <returns></returns>
        internal unsafe MLArrayFeature<float> Float () => To<float>();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal unsafe MLArrayFeature<int> Int () => To<int>();

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

        /// <summary>
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        internal unsafe MLArrayFeature<U> To<U> () where U : unmanaged {
            throw new NotImplementedException();
        }
        #endregion


        #region --Math--
        /// <summary>
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="keepDim"></param>
        /// <returns></returns>
        internal unsafe MLArrayFeature<T> Argmax (int dim = -1, bool keepDim = false) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="keepDim"></param>
        /// <returns></returns>
        internal unsafe MLArrayFeature<T> Argmin (int dim = -1, bool keepDim = false) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal unsafe MLArrayFeature<float> Exp () {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        internal unsafe MLArrayFeature<T> Dot (MLArrayFeature<T> input) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="keepDim"></param>
        /// <returns></returns>
        internal unsafe MLArrayFeature<float> Softmax (int dim = -1, bool keepDim = false) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="dim"></param>
        /// <param name="keepDim"></param>
        /// <returns></returns>
        internal unsafe MLArrayFeature<T> Sum (int dim = -1, bool keepDim = false) {
            throw new NotImplementedException();
        }
        #endregion


        #region --Copying--
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
                fixed (T* src = array) // `MemoryCopy` does `memmove` whereas `BlockCopy` seems to do more.
                    Buffer.MemoryCopy(src, buffer, length * sizeof(T), length * sizeof(T));
        }

        /// <summary>
        /// Convert the array feature to a flattened primitive array.
        /// </summary>
        /// <returns>Primitive array.</returns>
        public unsafe T[] ToArray () {
            var result = new T[elementCount];
            CopyTo(result);
            return result;
        }
        #endregion


        #region --Arithmetic--

        public static MLArrayFeature<T> operator - (MLArrayFeature<T> x) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator + (MLArrayFeature<T> a, float b) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator + (float a, MLArrayFeature<T> b) => b + a;

        public static MLArrayFeature<float> operator - (MLArrayFeature<T> a, float b) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator - (float a, MLArrayFeature<T> b) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator * (MLArrayFeature<T> a, float b) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator * (float a, MLArrayFeature<T> b) => b * a;

        public static MLArrayFeature<float> operator / (MLArrayFeature<T> a, float b) {
            throw new NotImplementedException();
        }

        public static MLArrayFeature<float> operator / (float a, MLArrayFeature<T> b) {
            throw new NotImplementedException();
        }
        #endregion


        #region --Operations--
        private readonly T[] array;
        private readonly T* nativeData;
        private readonly int[] strides;

        unsafe IntPtr IMLEdgeFeature.Create (in MLFeatureType type) {
            // Check types
            var featureType = type as MLArrayType;
            var arrayType = this.type as MLArrayType;
            if (featureType.dataType != arrayType.dataType)
                throw new ArgumentException($"MLModel expects {featureType.dataType} feature but was given {arrayType.dataType} feature");
            // Check that shape is fully specified
            var shape = arrayType.shape ?? featureType.shape;
            var dynamicAxis = Array.IndexOf(shape, -1);
            if (dynamicAxis != -1)
                throw new ArgumentException($"Array feature shape has unspecified size at dimension {dynamicAxis}");
            // Create feature
            fixed (T* managedData = array) {
                var data = nativeData != null ? nativeData : managedData;
                NatML.CreateFeature(
                    data,
                    shape,
                    shape.Length,
                    EdgeType(type.dataType),
                    0,
                    out var feature
                );
                return feature;
            }
        }

        unsafe MLHubFeature IMLHubFeature.Serialize () {
            // Check shape
            if (shape == null)
                throw new InvalidOperationException(@"Array feature cannot be used for Hub prediction because it has no shape");
            // Serialize
            var rawData = new byte[elementCount * sizeof(T)];
            fixed (void* dst = rawData)
                CopyTo((T*)dst, elementCount);
            // Create feature
            return new MLHubFeature {
                data = Convert.ToBase64String(rawData),
                type = HubType(type.dataType),
                shape = shape
            };
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