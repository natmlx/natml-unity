/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Internal;
    using Hub;
    using Types;
    using Stopwatch = System.Diagnostics.Stopwatch;

    /// <summary>
    /// ML model.
    /// </summary>
    public sealed class MLModel : IMLModel, IHubPredictor, IReadOnlyDictionary<string, string> {

        #region --Client API--
        /// <summary>
        /// Model input feature types.
        /// </summary>
        public MLFeatureType[] inputs { get; private set; }

        /// <summary>
        /// Model output feature types.
        /// </summary>
        public MLFeatureType[] outputs { get; private set; }

        /// <summary>
        /// Get a value in the model metadata.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <returns>Metadata value. Throws exception if key is not present.</returns>
        public string this [string key] {
            get {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                var buffer = new StringBuilder(2048);
                model.MetadataValue(key, buffer, buffer.Capacity);
                if (buffer.Length > 0)
                    return buffer.ToString();
                else
                    throw new KeyNotFoundException($"MLModel metadata does not contain the requested key: {key}");
            }
        }

        /// <summary>
        /// Dispose the model and release resources.
        /// </summary>
        public void Dispose () => model.ReleaseModel();
        #endregion


        #region --Operations--
        private readonly IntPtr model;
        private MLPredictionHandler predictionHandler;

        unsafe internal MLModel (byte[] graphData, void* reserved) {
            // Create
            fixed (void* buffer = graphData)
                NatML.CreateModel(buffer, graphData.Length, reserved, out model);
            if (model == IntPtr.Zero)
                throw new ArgumentException(@"Failed to create MLModel from graph data", nameof(graphData));
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
        }

        IntPtr[] IMLModel.Predict (params IntPtr[] inputs) { // CHECK // Invoke handler on separate thread
            var outputs = new IntPtr[this.outputs.Length];
            var watch = Stopwatch.StartNew();
            model.Predict(inputs, outputs);
            watch.Stop();
            predictionHandler?.Invoke(watch.Elapsed.TotalMilliseconds);
            return outputs;
        }

        event MLPredictionHandler IHubPredictor.Prediction {
            add => predictionHandler += value;
            remove => predictionHandler -= value;
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator () {
            foreach (var key in (this as IReadOnlyDictionary<string, string>).Keys)
                yield return new KeyValuePair<string, string>(key, this[key]);
        }

        IEnumerable<string> IReadOnlyDictionary<string, string>.Keys {
            get {
                var count = model.MetadataCount();
                var buffer = new StringBuilder(2048);
                for (var i = 0; i < count; ++i) {
                    buffer.Clear();
                    model.MetadataKey(i, buffer, buffer.Capacity);
                    yield return buffer.ToString();
                }
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, string>.Values {
            get {
                foreach (var key in (this as IReadOnlyDictionary<string, string>).Keys)
                    yield return this[key];
            }
        }

        int IReadOnlyCollection<KeyValuePair<string, string>>.Count => model.MetadataCount();

        bool IReadOnlyDictionary<string, string>.ContainsKey (string key) => (this as IReadOnlyDictionary<string, string>).Keys.Contains(key);

        bool IReadOnlyDictionary<string, string>.TryGetValue (string key, out string value) {
            var validKey = (this as IReadOnlyDictionary<string, string>).Keys.Contains(key);
            value = validKey ? this[key] : default;
            return validKey;
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<KeyValuePair<string, string>>).GetEnumerator();

        private static MLFeatureType MarshalFeatureType (in IntPtr nativeType) { // CHECK // Nested types
            // Get dtype
            var dtype = nativeType.FeatureTypeDataType();
            if (dtype == NMLDataType.Undefined)
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
                case NMLDataType.Sequence: return default;
                case NMLDataType.Dictionary: return default;
                case var _ when shape.Length == 4: return new MLImageType(name, dtype.ManagedType(), shape);
                default: return new MLArrayType(name, dtype.ManagedType(), shape);
            }
        }
        #endregion
    }
}