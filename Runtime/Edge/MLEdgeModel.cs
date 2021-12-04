/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using Hub;
    using Hub.Requests;
    using Types;

    /// <summary>
    /// ML model capable of making predictions on features.
    /// Models are NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLEdgeModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Make a prediction on one or more native input features.
        /// Input and output features MUST be released when no longer needed.
        /// </summary>
        /// <param name="inputs">Native input features.</param>
        /// <returns>Native output features.</returns>
        public IntPtr[] Predict (params IntPtr[] inputs) {
            var outputs = new IntPtr[this.outputs.Length];
            var watch = Stopwatch.StartNew();
            model.Predict(inputs, outputs);
            watch.Stop();
            ReportPrediction(watch.Elapsed.TotalMilliseconds);
            return outputs;
        }

        /// <summary>
        /// Dispose the model and release resources.
        /// </summary>
        public override void Dispose () => model.ReleaseModel();
        #endregion


        #region --Operations--
        private readonly IntPtr model;

        unsafe internal MLEdgeModel (string session, byte[] graph, void* reserved) : base(session) {
            // Create
            fixed (void* buffer = graph)
                NatML.CreateModel(buffer, graph.Length, reserved, out model);
            if (model == IntPtr.Zero)
                throw new ArgumentException(@"Failed to create MLModel from graph", nameof(graph));
            // Marshal input types
            this.inputs = new MLFeatureType[model.InputFeatureCount()];
            for (var i = 0; i < inputs.Length; ++i) {
                model.InputFeatureType(i, out var nativeType);
                inputs[i] = MarshalFeatureType(nativeType);
                nativeType.ReleaseFeatureType();
            }
            // Marshal output types
            this.outputs = new MLFeatureType[model.OutputFeatureCount()];
            for (var i = 0; i < outputs.Length; ++i) {
                model.OutputFeatureType(i, out var nativeType);
                outputs[i] = MarshalFeatureType(nativeType);
                nativeType.ReleaseFeatureType();
            }
            // Marshal dictionary
            var metadata = new Dictionary<string, string>();
            var count = model.MetadataCount();
            var metadataBuffer = new StringBuilder(2048);
            for (var i = 0; i < count; ++i) {
                metadataBuffer.Clear();
                model.MetadataKey(i, metadataBuffer, metadataBuffer.Capacity);
                var key = metadataBuffer.ToString();
                metadataBuffer.Clear();
                model.MetadataValue(key, metadataBuffer, metadataBuffer.Capacity);
                var value = metadataBuffer.ToString();
                metadata.Add(key, value);
            }
            this.metadata = metadata;
        }

        private async void ReportPrediction (double latency) {
            var date = DateTime.UtcNow;
            // Check
            if (string.IsNullOrEmpty(session))
                return;
            // Build payload // Force async completion so we don't hold up model prediction
            await Task.Yield();
            var report = new ReportPredictionRequest.Input {
                session = session,
                latency = latency,
                date = $"{date:O}"
            };
            // Send
            await NatMLHub.ReportPrediction(report);
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
            if (dtype == EdgeDataType.Undefined)
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
                case EdgeDataType.Sequence:         return default;
                case EdgeDataType.Dictionary:       return default;
                case var _ when shape.Length == 4:  return new MLImageType(name, ManagedType(dtype), shape);
                default:                            return new MLArrayType(name, ManagedType(dtype), shape);
            }
        }

        private static Type ManagedType (in EdgeDataType type) {
            switch (type) {
                case EdgeDataType.UInt8:        return typeof(byte);
                case EdgeDataType.Int16:        return typeof(short);
                case EdgeDataType.Int32:        return typeof(int);
                case EdgeDataType.Int64:        return typeof(long);
                case EdgeDataType.Float:        return typeof(float);
                case EdgeDataType.Double:       return typeof(double);
                case EdgeDataType.String:       return typeof(string);
                case EdgeDataType.Sequence:     return typeof(IList);
                case EdgeDataType.Dictionary:   return typeof(IDictionary);
                default:                        return null;
            }
        }
        #endregion
    }
}