/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

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

        unsafe internal MLEdgeModel (MLModelData modelData) : base(modelData) {
            // Create options
            NatML.CreateModelOptions(out var options);
            options.SetComputeTarget(modelData.computeTarget);
            options.SetComputeDevice(modelData.computeDevice);
            options.SetHubSessionFlags(modelData.session.flags);
            // Create model
            fixed (void* buffer = modelData.graph)
                NatML.CreateModel(buffer, modelData.graph.Length, options, out model);
            options.ReleaseModelOptions();
            // Check model
            if (model == IntPtr.Zero)
                throw new ArgumentException(@"Failed to create MLModel from graph data");
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
                case DataType.Sequence:             return default;
                case DataType.Dictionary:           return default;
                case var _ when shape.Length == 4:  return new MLImageType(shape, ManagedType(dtype), name);
                default:                            return new MLArrayType(shape, ManagedType(dtype), name);
            }
        }

        internal static Type ManagedType (DataType type) => type switch {
            DataType.Float16    => null, // Any support for this in C#?
            DataType.Float32    => typeof(float),
            DataType.Float64    => typeof(double),
            DataType.Int8       => typeof(sbyte),
            DataType.Int16      => typeof(short),
            DataType.Int32      => typeof(int),
            DataType.Int64      => typeof(long),
            DataType.UInt8      => typeof(byte),
            DataType.UInt16     => typeof(ushort),
            DataType.UInt32     => typeof(uint),
            DataType.UInt64     => typeof(ulong),
            DataType.String     => typeof(string),
            DataType.Sequence   => typeof(IList),
            DataType.Dictionary => typeof(IDictionary),
            _                   => null,
        };
        #endregion
    }
}