/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Newtonsoft.Json;
    using API;
    using API.Types;
    using Features;
    using Internal;
    using Types;

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

            /// <summary>
            /// Specify the compute target used for model predictions.
            /// </summary>
            public ComputeTarget computeTarget = ComputeTarget.Default;

            /// <summary>
            /// Specify the compute device used for model predictions.
            /// The native type of this pointer is platform-specific.
            /// </summary>
            public IntPtr computeDevice = IntPtr.Zero;
        }
        #endregion


        #region --Attributes--
        /// <summary>
        /// Embed an ML graph from NatML at build time so that the model is immediately available at runtime without downloading.
        /// Note that the build size of the application will increase as a result of the embedded model data.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
        public sealed class EmbedAttribute : Attribute {

            #region --Client API--
            /// <summary>
            /// Embed an ML graph from NatML.
            /// </summary>
            /// <param name="tag">Predictor tag.</param>
            /// <param name="accessKey">NatML access key. If `null` the project access key will be used.</param>
            public EmbedAttribute (string tag, string? accessKey = null) {
                this.tag = tag;
                this.accessKey = accessKey;
            }
            #endregion


            #region --Operations--
            internal readonly string tag;
            internal readonly string? accessKey;
            #endregion
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Predictor classification labels.
        /// This is `null` if the model does not use classification labels.
        /// </summary>
        public string[]? labels => session.predictor?.labels;

        /// <summary>
        /// Expected feature normalization for predictions with this model.
        /// </summary>
        public Normalization? normalization => session.predictor?.normalization;

        /// <summary>
        /// Expected image aspect mode for predictions with this model.
        /// </summary>
        public AspectMode aspectMode => session.predictor?.aspectMode ?? 0;

        /// <summary>
        /// Expected audio format for predictions with this model.
        /// </summary>
        public AudioFormat? audioFormat => session.predictor?.audioFormat;

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
        public static Task<MLEdgeModel> Create (
            string tagOrPath,
            Configuration? configuration = null,
            string? accessKey = null
        ) => Create(tagOrPath, configuration, MLUnityExtensions.CreateClient(accessKey));

        /// <summary>
        /// Create an edge ML model.
        /// </summary>
        /// <param name="tagOrPath">Predictor tag or path to model file.</param>
        /// <param name="configuration">Optional model configuration.</param>
        /// <param name="client">NatML API client.</param>
        public static async Task<MLEdgeModel> Create (
            string tagOrPath,
            Configuration? configuration,
            NatMLClient client
        ) {
            // Defines
            PredictorSession session;
            byte[] graph;
            // Handle tag
            if (Tag.TryParse(tagOrPath, out var tag)) {
                session = SessionFromCache(tag.ToString()) ?? await SessionFromHub(tag, client);
                graph = await LoadSessionGraph(session, client);
                await CacheSession(session, graph);
            }
            // Handle file
            else {
                // Check format
                var format = FormatForFile(tagOrPath);
                if (format == null)
                    throw new ArgumentException(@"Model file is not a recognized ML model format", nameof(tagOrPath));
                // Read graph and session
                using var stream = new FileStream(tagOrPath, FileMode.Open, FileAccess.Read);
                graph = new byte[stream.Length];
                await stream.ReadAsync(graph, 0, graph.Length);
                session = new PredictorSession { format = format.Value };
            }
            // Return
            var model = await Create(session, graph, configuration);
            return model;
        }

        /// <summary>
        /// Create an edge ML model.
        /// </summary>
        /// <param name="modelData">ML model data.</param>
        /// <param name="configuration">Optional model configuration.</param>
        public static Task<MLEdgeModel> Create (
            MLModelData modelData,
            Configuration? configuration = null
        ) => Create(modelData.session, modelData.graph, configuration);
        #endregion


        #region --Operations--
        private readonly IntPtr model;
        private readonly PredictorSession session;
        private readonly IntPtr[] rawInputFeatures;
        private readonly IntPtr[] rawOutputFeatures;
        private readonly MLEdgeFeature[] outputFeatures; // prevent GC
        private static string CachePath = string.Empty;
        private static RuntimePlatform Platform = 0;
        private static string Device = string.Empty;
        private const string Extension = @".nml";

        private unsafe MLEdgeModel (IntPtr model, PredictorSession session) {
            this.model = model;
            this.session = session;
            // Marshal input types
            this.inputs = new MLFeatureType[model.InputFeatureCount()];
            this.rawInputFeatures = new IntPtr[this.inputs.Length];
            for (var i = 0; i < inputs.Length; ++i) {
                model.InputFeatureType(i, out var type);
                inputs[i] = CreateFeatureType(type);
                type.ReleaseFeatureType();
            }
            // Marshal output types
            this.outputs = new MLFeatureType[model.OutputFeatureCount()];
            this.rawOutputFeatures = new IntPtr[this.outputs.Length];
            this.outputFeatures = new MLEdgeFeature[this.outputs.Length];
            for (var i = 0; i < outputs.Length; ++i) {
                model.OutputFeatureType(i, out var type);
                outputs[i] = CreateFeatureType(type);
                type.ReleaseFeatureType();
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

        private static Task<MLEdgeModel> Create (PredictorSession session, byte[] graph, Configuration? config) {
            // Check format
            if (session.format != FormatForPlatform(Platform))
                throw new InvalidOperationException($"Cannot deserialize {session.format} graph on current platform");
            // Create configuration
            NatML.CreateModelConfiguration(out var configuration);
            configuration.SetComputeTarget(config?.computeTarget ?? (ComputeTarget)session.flags);
            configuration.SetComputeDevice(config?.computeDevice ?? IntPtr.Zero);
            configuration.SetFingerprint(session.fingerprint);
            configuration.SetSecret(session.secret);
            // Create model
            var tcs = new TaskCompletionSource<MLEdgeModel>();
            var request = new ModelCreationRequest (session, tcs);
            var context = (IntPtr)GCHandle.Alloc(request, GCHandleType.Normal);
            NatML.CreateModel(graph, graph.Length, configuration, OnCreateModel, context);
            configuration.ReleaseModelConfiguration();
            return tcs.Task;
        }

        /// <summary>
        /// Create a predictor session secret.
        /// </summary>
        /// <returns>Predictor session secret.</returns>
        internal static Task<string> CreateSecret () {
            var tcs = new TaskCompletionSource<string>();
            var context = (IntPtr)GCHandle.Alloc(tcs, GCHandleType.Normal);
            NatML.CreateSecret(OnCreateSecret, context);
            return tcs.Task;
        }

        /// <summary>
        /// Clear the predictor cache.
        /// This function should only be used in the Unity Editor.
        /// </summary>
        internal static void ClearCache () {
            try {
                var path = Path.Combine(Application.persistentDataPath, @"natml");
                Directory.Delete(path, true);
            } catch { }
        }
        #endregion


        #region --Callbacks--

        [AOT.MonoPInvokeCallback(typeof(NatML.ModelCreationHandler))]
        private static void OnCreateModel (IntPtr context, IntPtr model) {
            // Get request
            var handle = (GCHandle)context;
            var request = handle.Target as ModelCreationRequest;
            handle.Free();
            // Check
            if (request == null) {
                model.ReleaseModel();
                return;
            }
            // Create model
            var session = request.session;
            var tcs = request.tcs;
            if (model != IntPtr.Zero)
                tcs.SetResult(new MLEdgeModel(model, session));
            else
                tcs.SetException(new ArgumentException(@"Failed to create MLModel from graph data"));
        }

        [AOT.MonoPInvokeCallback(typeof(NatML.SecretCreationHandler))]
        private static void OnCreateSecret (IntPtr context, IntPtr secret) {
            // Get tcs
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<string>;
            handle.Free();
            // Check tcs
            if (tcs == null)
                return;
            // Create secret
            tcs.SetResult(Marshal.PtrToStringAuto(secret));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitialize () {
            CachePath = Path.Combine(Application.persistentDataPath, @"natml");
            Platform = Application.platform;
            Device = $"{SystemInfo.deviceModel} {SystemInfo.operatingSystem}";
        }
        #endregion


        #region --Utilitiess--
        /// <summary>
        /// Load a graph prediction session from the local cache.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <returns>Graph prediction session or `null` if there is no cached session for the given predictor tag.</returns>
        private static PredictorSession? SessionFromCache (string tag) {
            // Check
            var path = GetSessionCachePath(tag);
            if (!File.Exists(path))
                return null;
            // Load session
            PredictorSession? session = null;
            try {
                var sessionStr = File.ReadAllText(path);
                session = JsonConvert.DeserializeObject<PredictorSession>(sessionStr);
            } catch {
                File.Delete(path);
                return null;
            }
            // Check session
            if (session == null)
                return null;
            // Check graph
            var graphPath = GetGraphCachePath(session.fingerprint);
            if (!File.Exists(graphPath)) {
                File.Delete(path);
                return null;
            }
            // Return session
            return session;
        }

        /// <summary>
        /// Create a graph prediction session from NatML.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="client">NatML API client.</param>
        /// <returns>Graph prediction session.</returns>
        private static async Task<PredictorSession> SessionFromHub (string tag, NatMLClient client) {
            var secret = await CreateSecret();
            var format = FormatForPlatform(Platform);
            var session = await client.PredictorSessions.Create(tag, format, secret, Device);
            return session;
        }

        /// <summary>
        /// </summary>
        /// <param name="session"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task<byte[]> LoadSessionGraph (PredictorSession session, NatMLClient client) {
            // Check embed
            var embeddedData = NatMLSettings.Instance.embeds.FirstOrDefault(embed => embed.fingerprint == session.fingerprint);
            if (embeddedData != null)
                return embeddedData.data;
            // Check cached graph
            var path = GetGraphCachePath(session.fingerprint);
            if (File.Exists(path)) {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var graph = new byte[fileStream.Length];
                await fileStream.ReadAsync(graph, 0, graph.Length);
                return graph;
            }
            // Download graph
            using var stream = await client.Storage.Download(session.graph);
            return stream.ToArray();            
        }

        /// <summary>
        /// Cache a graph prediction session.
        /// </summary>
        /// <param name="session">Graph prediction session.</param>
        /// <param name="tag">Graph tag.</param>
        private static async Task CacheSession (PredictorSession session, byte[] graph) {
            // Check if cached
            var sessionPath = GetSessionCachePath(session.predictor.tag);
            if (File.Exists(sessionPath))
                return;
            // Check if cacheable
            if (session.predictor.status == PredictorStatus.Draft || Platform == RuntimePlatform.WebGLPlayer)
                return;
            // Write graph
            var graphPath = GetGraphCachePath(session.fingerprint);
            Directory.CreateDirectory(CachePath);
            using var graphStream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await graphStream.WriteAsync(graph, 0, graph.Length);
            // Write session
            session.graph = string.Empty;
            var sessionStr = JsonConvert.SerializeObject(session);
            using var sessionStream = new StreamWriter(sessionPath);
            await sessionStream.WriteAsync(sessionStr);
        }

        /// <summary>
        /// Get the cache path for a given graph prediction session.
        /// </summary>
        /// <param name="tag">Graph tag.</param>
        /// <returns>Session cache path.</returns>
        private static string GetSessionCachePath (string tag) {
            var stem = tag.Replace('/', '_');
            var path = Path.Combine(CachePath, $"{stem}{Extension}");
            return path;
        }

        /// <summary>
        /// Get the cache path for a given graph fingerprint.
        /// </summary>
        /// <param name="tag">Graph fingerprint.</param>
        /// <returns>Graph cache path.</returns>
        private static string GetGraphCachePath (string fingerprint) {
            var name = $"{fingerprint}{Extension}";
            var path = Path.Combine(CachePath, name);
            return path;
        }

        /// <summary>
        /// Get the graph format for an ML graph file based on its file extension.
        /// </summary>
        /// <param name="path">Path to ML graph.</param>
        /// <returns>Graph format.</returns>
        private static GraphFormat? FormatForFile (string path) => Path.GetExtension(path) switch {
            ".mlmodel"  => GraphFormat.CoreML,
            ".onnx"     => GraphFormat.ONNX,
            ".tflite"   => GraphFormat.TFLite,
            _           => null,
        };

        /// <summary>
        /// Get the graph format for a given platform.
        /// </summary>
        /// <param name="platform">Runtime platform.</param>
        /// <returns>Graph format.</returns>
        private static GraphFormat FormatForPlatform (RuntimePlatform platform) => platform switch {
            RuntimePlatform.Android         => GraphFormat.TFLite,
            RuntimePlatform.IPhonePlayer    => GraphFormat.CoreML,
            RuntimePlatform.OSXEditor       => GraphFormat.CoreML,
            RuntimePlatform.OSXPlayer       => GraphFormat.CoreML,
            RuntimePlatform.WebGLPlayer     => GraphFormat.ONNX,
            RuntimePlatform.WindowsEditor   => GraphFormat.ONNX,
            RuntimePlatform.WindowsPlayer   => GraphFormat.ONNX,
            _                               => GraphFormat.ONNX, // this doesn't need to be nullable
        };

        /// <summary>
        /// Convert a native `NMLFeatureType` to a managed `MLFeatureType`.
        /// </summary>
        /// <param name="nativeType">Native `NMLFeatureType`.</param>
        /// <returns>Managed feature type.</returns>
        private static MLFeatureType? CreateFeatureType (in IntPtr type) {
            // Get dtype
            var dtype = type.FeatureTypeDataType();
            if (dtype == Dtype.Undefined)
                return null;
            // Get name
            var nameBuffer = new StringBuilder(2048);
            type.FeatureTypeName(nameBuffer, nameBuffer.Capacity);
            var name = nameBuffer.Length > 0 ? nameBuffer.ToString() : null;
            // Get shape
            var shape = new int[type.FeatureTypeDimensions()];
            type.FeatureTypeShape(shape, shape.Length);
            // Return
            switch (dtype) {
                case Dtype.List:                    return null;
                case Dtype.Dict:                    return null;
                case var _ when shape.Length == 4:  return new MLImageType(shape, dtype.ToType(), name);
                default:                            return new MLArrayType(shape, dtype.ToType(), name);
            }
        }

        private sealed class ModelCreationRequest {

            public readonly PredictorSession session;
            public readonly TaskCompletionSource<MLEdgeModel> tcs;

            public ModelCreationRequest (PredictorSession session, TaskCompletionSource<MLEdgeModel> tcs) {
                this.session = session;
                this.tcs = tcs;
            }
        }
        #endregion
    }
}