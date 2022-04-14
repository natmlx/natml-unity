/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Types;
    using DataType = NatML.DataType;

    /// <summary>
    /// ML model capable of making predictions on features.
    /// Edge models are NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLEdgeModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Make a prediction on one or more Edge features.
        /// Input and output features MUST be disposed when no longer needed.
        /// </summary>
        /// <param name="inputs">Input Edge features.</param>
        /// <returns>Output Edge features.</returns>
        public MLFeatureCollection<MLEdgeFeature> Predict (params MLEdgeFeature[] inputs) {
            Array.Clear(rawInputFeatures, 0, rawInputFeatures.Length);
            Array.Clear(rawOutputFeatures, 0, rawOutputFeatures.Length);
            for (var i = 0; i < rawInputFeatures.Length; ++i)
                rawInputFeatures[i] = inputs[i];
            model.Predict(rawInputFeatures, rawOutputFeatures);
            for (var i = 0; i < rawOutputFeatures.Length; ++i)
                outputFeatures[i] = new MLEdgeFeature(rawOutputFeatures[i]);
            return outputFeatures;
        }

        /// <summary>
        /// Dispose the model and release resources.
        /// </summary>
        public override void Dispose () => model.ReleaseModel();
        #endregion


        #region --Operations--
        private readonly IntPtr model;
        private readonly IntPtr[] rawInputFeatures;
        private readonly IntPtr[] rawOutputFeatures;
        private readonly MLEdgeFeature[] outputFeatures; // Prevent GC

        unsafe internal MLEdgeModel (string session, byte[] graph, int reserved) : base(session) {
            // Create
            fixed (void* buffer = graph)
                NatML.CreateModel(buffer, graph.Length, reserved, out model);
            if (model == IntPtr.Zero)
                throw new ArgumentException(@"Failed to create MLModel from graph", nameof(graph));
            // Marshal input types
            this.inputs = new MLFeatureType[model.InputFeatureCount()];
            this.rawInputFeatures = new IntPtr[this.inputs.Length];
            for (var i = 0; i < inputs.Length; ++i) {
                model.InputFeatureType(i, out var nativeType);
                inputs[i] = MarshalFeatureType(nativeType);
                nativeType.ReleaseFeatureType();
            }
            // Marshal output types
            this.outputs = new MLFeatureType[model.OutputFeatureCount()];
            this.rawOutputFeatures = new IntPtr[this.outputs.Length];
            this.outputFeatures = new MLEdgeFeature[this.outputs.Length];
            for (var i = 0; i < outputs.Length; ++i) {
                model.OutputFeatureType(i, out var nativeType);
                outputs[i] = MarshalFeatureType(nativeType);
                nativeType.ReleaseFeatureType();
            }
            // Marshal dictionary
            var metadata = new Dictionary<string, string>();
            var count = model.MetadataCount();
            var metadataBuffer = new StringBuilder(8192);
            for (var i = 0; i < count; ++i) {
                metadataBuffer.Clear();
                model.MetadataKey(i, metadataBuffer, metadataBuffer.Capacity);
                var key = metadataBuffer.ToString();
                metadataBuffer.Clear();
                model.MetadataValue(key, metadataBuffer, metadataBuffer.Capacity);
                var value = metadataBuffer.ToString();
                if (!string.IsNullOrEmpty(value))
                    metadata.Add(key, value);
            }
            this.metadata = metadata;
        }

        public override string ToString () {
            var attribs = new List<string> { GetType().Name };
            for (var i = 0; i < inputs.Length; ++i)
                attribs.Add($"Input: {inputs[i]}");
            for (var i = 0; i < outputs.Length; ++i)
                attribs.Add($"Output: {outputs[i]}");
            return string.Join(Environment.NewLine, attribs);
        }

        private static MLFeatureType MarshalFeatureType (in IntPtr nativeType) { // CHECK // Nested types
            // Get dtype
            var dtype = nativeType.FeatureTypeDataType();
            if (dtype == DataType.Undefined)
                return null;
            // Get name
            var nameBuffer = new StringBuilder(2048);
            nativeType.FeatureTypeName(nameBuffer, nameBuffer.Capacity);
            var name = nameBuffer.Length > 0 ? nameBuffer.ToString() : null;
            // Get shape
            var shape = new int[nativeType.FeatureTypeDimensions()];
            nativeType.FeatureTypeShape(shape, shape.Length);
            // Return
            switch (dtype) {
                case DataType.Sequence:         return default;
                case DataType.Dictionary:       return default;
                case var _ when shape.Length == 4:  return new MLImageType(shape, ManagedType(dtype), name);
                default:                            return new MLArrayType(shape, ManagedType(dtype), name);
            }
        }

        private static Type ManagedType (in DataType type) {
            switch (type) {
                case DataType.Float16:      return null; // Any support for this in C#?
                case DataType.Float32:      return typeof(float);
                case DataType.Float64:      return typeof(double);
                case DataType.Int8:         return typeof(sbyte);
                case DataType.Int16:        return typeof(short);
                case DataType.Int32:        return typeof(int);
                case DataType.Int64:        return typeof(long);
                case DataType.UInt8:        return typeof(byte);
                case DataType.UInt16:       return typeof(ushort);
                case DataType.UInt32:       return typeof(uint);
                case DataType.UInt64:       return typeof(ulong);
                case DataType.String:       return typeof(string);
                case DataType.Sequence:     return typeof(IList);
                case DataType.Dictionary:   return typeof(IDictionary);
                default:                    return null;
            }
        }
        #endregion
    }
}