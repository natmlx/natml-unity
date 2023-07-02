/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using API.Types;
    using Internal;
    using Types;

    /// <summary>
    /// ML array feature.
    /// The array feature represents generic numeric tensor data.
    /// The array feature never owns or allocates its own memory. Instead, it provides a "view" into existing data.
    /// </summary>
    public sealed unsafe class MLArrayFeature<T> : MLFeature, IMLEdgeFeature, IMLCloudFeature where T : unmanaged {
    
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
            get {
                fixed (T* data = this)
                    return data[idx];
            }
            set {
                fixed (T* data = this)
                    data[idx] = value;
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
        /// <param name="shape">Feature shape.</param>
        public MLArrayFeature (T[] data, int[] shape = null) : this(data, new MLArrayType(shape, typeof(T))) { }

        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public MLArrayFeature (NativeArray<T> data, int[] shape = null) : this(data, new MLArrayType(shape, typeof(T))) { }
        
        /// <summary>
        /// Create an array feature.
        /// </summary>
        /// <param name="data">Feature data.</param>
        /// <param name="shape">Feature shape.</param>
        public unsafe MLArrayFeature (T* data, int[] shape = null) : this(data, new MLArrayType(shape, typeof(T))) { }

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
            this.buffer = data;
            this.strides = ComputeStrides(shape);
        }

        /// <summary>
        /// Create an array feature from an edge feature.
        /// Note that this does NOT take ownership of the edge feature.
        /// As such the edge feature MUST be disposed by the client.
        /// </summary>
        /// <param name="feature">Edge feature. This MUST be an array feature.</param>
        public MLArrayFeature (MLEdgeFeature feature) : this((T*)feature.data, feature.shape) { }

        /// <summary>
        /// Create an array feature from a cloud feature.
        /// </summary>
        /// <param name="feature">Cloud feature. This MUST be a numeric dtype.</param>
        public MLArrayFeature (MLCloudFeature feature) : this(GetFeatureData(feature), feature.shape) { }
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
            return buffer != null ? new MLArrayFeature<T>(buffer, shape) : new MLArrayFeature<T>(array, shape);
        }
        #endregion


        #region --Copying--
        /// <summary>
        /// Copy the array feature into another feature.
        /// This copies `elementCount * sizeof(T)` bytes.
        /// </summary>
        /// <param name="destination">Feature to copy data into.</destination>
        public unsafe void CopyTo<U> (MLArrayFeature<U> destination) where U : unmanaged {
            // Check that shape is specified
            if (shape == null)
                throw new InvalidOperationException(@"Cannot copy to destination feature because source feature does not have a shape");
            // Copy
            var size = elementCount * sizeof(T); // bytes
            fixed (void* src = this, dst = destination)
                Buffer.MemoryCopy(src, dst, size, size);
        }

        /// <summary>
        /// Copy the array feature data into a texture.
        /// This method MUST only be used from the Unity main thread.
        /// The texture format MUST be compatible with the array feature data type.
        /// </summary>
        /// <param name="destination">Texture to copy data into.</param>
        /// <param name="upload">Whether to upload the pixel data to the GPU after copying.</param>k
        public unsafe void CopyTo (
            Texture2D destination,
            bool upload = true
        ) {
            // Check dims
            var imageType = MLImageType.FromType(type);
            if (destination.width != imageType.width || destination.height != imageType.height)
                throw new ArgumentException(@"Cannot copy to texture because texture size does not match feature size", nameof(destination));
            // Check format
            if (!TextureFormats.TryGetValue(typeof(T), out var formats) || !formats.Contains(destination.format))
                throw new ArgumentException($"Cannot copy to texture because texture format {destination.format} is not compatible with this array feature", nameof(destination));
            // Copy data
            var view = new MLArrayFeature<T>(destination.GetRawTextureData<T>());
            CopyTo(view);
            // Upload
            if (upload)
                destination.Apply();
        }

        /// <summary>
        /// Convert the array feature to a flattened array.
        /// This method always returns a copy of the array feature data.
        /// </summary>
        /// <returns>Result array.</returns>
        public unsafe T[] ToArray () => ToArray<T>();

        /// <summary>
        /// Convert the array feature to a flattened array.
        /// This method always returns a copy of the array feature data.
        /// </summary>
        /// <returns>Result array.</returns>
        public unsafe U[] ToArray<U> () where U : unmanaged {
            var byteLength = elementCount * sizeof(T);
            var length = byteLength / sizeof(U);
            var result = new U[length];
            var feature = new MLArrayFeature<U>(result, new [] { result.Length });
            CopyTo(feature);
            return result;
        }
        #endregion


        #region --Operations--
        private readonly T[] array;
        private readonly T* buffer;
        private readonly int[] strides;
        private static readonly Dtype[] Dtypes = new [] {
            Dtype.Float16, Dtype.Float32, Dtype.Float64,
            Dtype.Int8, Dtype.Int16, Dtype.Int32, Dtype.Int64,
            Dtype.Uint8, Dtype.Uint16, Dtype.Uint32, Dtype.Uint64,
            Dtype.Bool
        };
        private static readonly Dictionary<Type, List<TextureFormat>> TextureFormats = new () {
            [typeof(byte)] = new () { TextureFormat.RGBA32, TextureFormat.BGRA32, TextureFormat.ARGB32, TextureFormat.RGB24, TextureFormat.Alpha8, TextureFormat.R8 },
            [typeof(float)] = new () { TextureFormat.RGBAFloat, TextureFormat.RFloat },
        };

        public ref T GetPinnableReference () => ref (buffer == null ? ref array[0] : ref *buffer);

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
            fixed (T* data = this) {
                NatML.CreateFeature(
                    data,
                    shape,
                    shape.Length,
                    type.dataType.ToDtype(),
                    data == buffer ? 0 : 1,
                    out var feature
                );
                return new MLEdgeFeature(feature);
            }
        }

        unsafe MLCloudFeature IMLCloudFeature.Create (MLFeatureType _) {
            // Check shape
            if (shape == null)
                throw new InvalidOperationException(@"Array feature cannot be used for Cloud prediction because it has no shape");
            // Create feature
            var data = ToArray<byte>();
            var stream = new MemoryStream(data, 0, data.Length, false, false);
            var dtype = typeof(T).ToDtype();
            var feature = new MLCloudFeature(stream, dtype, shape);
            return feature;
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

        private static T[] GetFeatureData (MLCloudFeature feature) {
            // Check
            if (Array.IndexOf(Dtypes, feature.type) < 0)
                throw new ArgumentException(@"Cloud feature is not an array feature", nameof(feature));
            // Deserialize
            var rawData = feature.data.ToArray();
            var data = new T[rawData.Length / sizeof(T)];
            Buffer.BlockCopy(rawData, 0, data, 0, rawData.Length);
            return data;
        }
        #endregion
    }
}