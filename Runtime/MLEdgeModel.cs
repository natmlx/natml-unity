/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Features;
    using Hub;
    using Hub.Internal;
    using Hub.Types;
    using Internal;
    using Types;
    using DataType = Internal.NatML.DataType;

    /// <summary>
    /// ML model that makes edge (on-device) predictions with a predictor graph.
    /// Edge models are NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLEdgeModel : MLModel {

        #region --Enumerations--
        /// <summary>
        /// Compute target used for model predictions.
        /// </summary>
        [Flags]
        public enum ComputeTarget : int { // CHECK // Must match `NatML.h`
            /// <summary>
            /// Use the default compute target for the given platform.
            /// </summary>
            Default = 0,
            /// <summary>
            /// Use the CPU.
            /// </summary>
            CPU     = 1 << 0,
            /// <summary>
            /// Use the GPU.
            /// </summary>
            GPU     = 1 << 1,
            /// <summary>
            /// Use the neural processing unit.
            /// </summary>
            NPU     = 1 << 2,
            /// <summary>
            /// Use all available compute targets including the CPU, GPU, and neural processing units.
            /// </summary>
            All     = CPU | GPU | NPU,
        }
        #endregion


        #region --Types--
        /// <summary>
        /// Edge model configuration.
        /// </summary>
        public sealed class Configuration {

            #region --Client API--
            /// <summary>
            /// Specify the compute target used for model predictions.
            /// </summary>
            public ComputeTarget computeTarget = ComputeTarget.Default;

            /// <summary>
            /// Specify the compute device used for model predictions.
            /// The native type of this pointer is platform-specific.
            /// </summary>
            public IntPtr computeDevice = IntPtr.Zero;
            #endregion


            #region --Operations--
            internal int flags = 0;
            internal string fingerprint;
            internal string secret;
            #endregion
        }

        /// <summary>
        /// Feature normalization.
        /// </summary>
        public struct Normalization {

            #region --Client API--
            /// <summary>
            /// Per-channel normalization means.
            /// </summary>
            public float[] mean;

            /// <summary>
            /// Per-channel normalization standard deviations.
            /// </summary>
            public float[] std;
            #endregion

            
            #region --Operations--

            public void Deconstruct (out Vector3 outMean, out Vector3 outStd) => (outMean, outStd) = (
                mean?.Length > 0 ? new Vector3(mean[0], mean[1], mean[2]) : Vector3.zero,
                std?.Length > 0 ? new Vector3(std[0], std[1], std[2]) : Vector3.one
            );
            #endregion
        }

        /// <summary>
        /// Audio format description for models that work on audio data.
        /// </summary>
        public struct AudioFormat {
            
            #region --Client API--
            /// <summary>
            /// Sample rate.
            /// </summary>
            public int sampleRate;

            /// <summary>
            /// Channel count.
            /// </summary>
            public int channelCount;
            #endregion


            #region --Operations--

            public void Deconstruct (out int sampleRate, out int channelCount) => (sampleRate, channelCount) = (this.sampleRate, this.channelCount);
            #endregion
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Predictor classification labels.
        /// This is `null` if the model does not use classification labels.
        /// </summary>
        public string[] labels => session.predictor?.labels;

        /// <summary>
        /// Expected feature normalization for predictions with this model.
        /// </summary>
        public Normalization normalization => new Normalization {
            mean    = session.predictor?.normalization?.mean,
            std     = session.predictor?.normalization?.std
        };

        /// <summary>
        /// Expected image aspect mode for predictions with this model.
        /// </summary>
        public MLImageFeature.AspectMode aspectMode => session.predictor?.aspectMode switch {
            "SCALE_TO_FIT"  => MLImageFeature.AspectMode.ScaleToFit,
            "ScaleToFit"    => MLImageFeature.AspectMode.ScaleToFit,
            "ASPECT_FILL"   => MLImageFeature.AspectMode.AspectFill,
            "AspectFill"    => MLImageFeature.AspectMode.AspectFill,
            "ASPECT_FIT"    => MLImageFeature.AspectMode.AspectFit,
            "AspectFit"     => MLImageFeature.AspectMode.AspectFit,
            _               =>  0,
        };

        /// <summary>
        /// Expected audio format for predictions with this model.
        /// </summary>
        public AudioFormat audioFormat => new AudioFormat {
            sampleRate      = session.predictor?.audioFormat.sampleRate ?? 0,
            channelCount    = session.predictor?.audioFormat.channelCount ?? 0
        };

        /// <summary>
        /// Make a prediction on one or more edge features.
        /// Input and output features MUST be disposed when no longer needed.
        /// </summary>
        /// <param name="inputs">Input edge features.</param>
        /// <returns>Output edge features.</returns>
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

        /// <summary>
        /// Create an edge ML model.
        /// </summary>
        /// <param name="tagOrPath">Predictor tag or path to model file.</param>
        /// <param name="configuration">Optional model configuration.</param>
        /// <param name="accessKey">NatML access key.</param>
        public static async Task<MLEdgeModel> Create (string tagOrPath, Configuration configuration = null, string accessKey = null) {
            // Create from file
            if (!Tag.TryParse(tagOrPath, out var tag))
                return await Create(tagOrPath, configuration);
            // Create from model data
            var modelData = await MLModelData.FromHub(tag, accessKey);
            return await Create(modelData);
        }

        /// <summary>
        /// Create an edge ML model.
        /// </summary>
        /// <param name="modelData">ML model data.</param>
        /// <param name="configuration">Optional model configuration.</param>
        public static Task<MLEdgeModel> Create (MLModelData modelData, Configuration configuration = null) {
            // Check format to prevent angry devs :)
            var session = modelData.session;
            var platform = NatMLHub.CurrentPlatform;
            var format = session?.format ?? "NULL";
            if (format != GraphFormat.FormatForPlatform(platform))
                throw new InvalidOperationException($"Cannot deserialize {format} graph on {platform}");
            // Create model
            var graph = modelData.graph;
            var config = new Configuration {
                computeTarget   = configuration?.computeTarget ?? ComputeTarget.Default,
                computeDevice   = configuration?.computeDevice ?? IntPtr.Zero,
                flags           = session.flags,
                fingerprint     = session.fingerprint,
                secret          = session.secret,
            };
            return Create(graph, session, config);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr model;              // non-zero
        private readonly PredictorSession session;  // non-null
        private readonly IntPtr[] rawInputFeatures;
        private readonly IntPtr[] rawOutputFeatures;
        private readonly MLEdgeFeature[] outputFeatures; // prevent GC

        private unsafe MLEdgeModel (IntPtr model, PredictorSession session) {
            this.model = model;
            this.session = session;
            // Marshal input types
            this.inputs = new MLFeatureType[model.InputFeatureCount()];
            this.rawInputFeatures = new IntPtr[this.inputs.Length];
            for (var i = 0; i < inputs.Length; ++i) {
                model.InputFeatureType(i, out var nativeType);
                inputs[i] = nativeType.ToFeatureType();
                nativeType.ReleaseFeatureType();
            }
            // Marshal output types
            this.outputs = new MLFeatureType[model.OutputFeatureCount()];
            this.rawOutputFeatures = new IntPtr[this.outputs.Length];
            this.outputFeatures = new MLEdgeFeature[this.outputs.Length];
            for (var i = 0; i < outputs.Length; ++i) {
                model.OutputFeatureType(i, out var nativeType);
                outputs[i] = nativeType.ToFeatureType();
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

        private static Task<MLEdgeModel> Create (string path, Configuration configuration) {
            // Check format
            var format = MLGraphUtils.FormatForFile(path);
            if (string.IsNullOrEmpty(format))
                throw new ArgumentException(@"Model file is not a recognized ML model format", nameof(path));
            // Check platform
            var platform = NatMLHub.CurrentPlatform;
            if (format != GraphFormat.FormatForPlatform(platform))
                throw new InvalidOperationException($"Cannot deserialize {format} graph on {platform}");            
            // Create graph
            var graph = File.ReadAllBytes(path);
            var session = new PredictorSession { format = format };
            return Create(graph, session, configuration);
        }

        private static Task<MLEdgeModel> Create (byte[] graph, PredictorSession session, Configuration configuration) {
            // Create options
            configuration ??= new Configuration();
            NatML.CreateModelOptions(out var options);
            options.SetComputeTarget(configuration.computeTarget);
            options.SetComputeDevice(configuration.computeDevice);
            options.SetFlags(configuration.flags);
            options.SetFingerprint(configuration.fingerprint);
            options.SetSecret(configuration.secret);
            // Create model
            var tcs = new TaskCompletionSource<MLEdgeModel>();
            var request = new ModelCreationRequest { session = session, tcs = tcs };
            var context = (IntPtr)GCHandle.Alloc(request, GCHandleType.Normal);
            NatML.CreateModel(graph, graph.Length, options, OnCreateModel, context);
            options.ReleaseModelOptions();
            // Return
            return tcs.Task;
        }

        [AOT.MonoPInvokeCallback(typeof(NatML.ModelCreationHandler))]
        private static void OnCreateModel (IntPtr context, IntPtr model) {
            // Get tcs
            var handle = (GCHandle)context;
            var request = handle.Target as ModelCreationRequest;
            handle.Free();
            // Create model
            var session = request.session;
            var tcs = request.tcs;
            if (model != IntPtr.Zero)
                tcs.SetResult(new MLEdgeModel(model, session));
            else
                tcs.SetException(new ArgumentException(@"Failed to create MLModel from graph data"));
        }

        private sealed class ModelCreationRequest {
            public PredictorSession session;
            public TaskCompletionSource<MLEdgeModel> tcs;
        }
        #endregion
    }
}