/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NatSuite.ML.Editor")]
namespace NatSuite.ML {

    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Features;
    using Hub;
    using Hub.Requests;
    using Internal;

    /// <summary>
    /// Self-contained archive with ML model and supplemental data needed to make predictions.
    /// </summary>
    public sealed class MLModelData : ScriptableObject {

        #region --Types--
        /// <summary>
        /// Feature normalization.
        /// </summary>
        [Serializable]
        public struct Normalization {

            /// <summary>
            /// Per-channel normalization means.
            /// </summary>
            [SerializeField] public float[] mean;

            /// <summary>
            /// Per-channel normalization standard deviations.
            /// </summary>
            [SerializeField] public float[] std;

            public void Deconstruct (out Vector3 outMean, out Vector3 outStd) {
                outMean = mean?.Length > 0 ? new Vector3(mean[0], mean[1], mean[2]) : Vector3.zero;
                outStd = std?.Length > 0 ? new Vector3(std[0], std[1], std[2]) : Vector3.one;
            }
        }

        /// <summary>
        /// Audio format description for models that work on audio data.
        /// </summary>
        [Serializable]
        public struct AudioFormat {

            /// <summary>
            /// Sample rate.
            /// </summary>
            [SerializeField] public int sampleRate;

            /// <summary>
            /// Channel count.
            /// </summary>
            [SerializeField] public int channelCount;

            public void Deconstruct (out int sampleRate, out int channelCount) {
                sampleRate = this.sampleRate;
                channelCount = this.channelCount;
            }
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Predictor tag on NatML.
        /// See https://hub.natml.ai.
        /// </summary>
        public string tag => session.predictor.tag;

        /// <summary>
        /// Predictor classification labels.
        /// This is `null` if the predictor does not have use classification labels.
        /// </summary>
        public string[] labels => session.predictor.labels;

        /// <summary>
        /// Expected feature normalization for predictions with this model.
        /// </summary>
        public Normalization normalization => session.predictor.normalization;

        /// <summary>
        /// Expected image aspect mode for predictions with this model.
        /// </summary>
        public MLImageFeature.AspectMode aspectMode {
            get {
                switch (session.predictor.aspectMode) {
                    case "SCALE_TO_FIT":
                    case "ScaleToFit":      return MLImageFeature.AspectMode.ScaleToFit;
                    case "ASPECT_FILL":
                    case "AspectFill":      return MLImageFeature.AspectMode.AspectFill;
                    case "ASPECT_FIT":
                    case "AspectFit":       return MLImageFeature.AspectMode.AspectFit;
                    default:                return 0;
                }
            }
        }

        /// <summary>
        /// Expected audio format for predictions with this model.
        /// </summary>
        public AudioFormat audioFormat => session.predictor.audioFormat;

        /// <summary>
        /// Deserialize the model data to create an ML model that can be used for prediction.
        /// You MUST dispose the model once you are done with it.
        /// </summary>
        /// <returns>ML model.</returns>
        public unsafe MLModel Deserialize () {
            // Check platform to prevent hard crashes :)
            if (!string.IsNullOrEmpty(session.format) && session.format != PlatformFormat)
                throw new InvalidOperationException($"Cannot deserialize {session.format} graph on {CurrentPlatform}");
            // Deserialize
            switch (session.predictor.type) {
                case PredictorType.Edge:    return new MLEdgeModel(session.id, graph, (NatML.ModelFlags)session.flags);
                case PredictorType.Hub:     return new MLHubModel(session.id);
                default:                    throw new InvalidOperationException(@"Invalid predictor type");
            }
        }

        /// <summary>
        /// Fetch ML model data from NatML.
        /// Explore the NatML catalog: https://hub.natml.ai/
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns>ML model data.</returns>
        public static async Task<MLModelData> FromHub (string tag, string accessKey = null) {
            // Load from cache
            var modelData = await FromCache(tag);
            if (modelData != null)
                return modelData;
            // Load from Hub
            var input = new CreateSessionRequest.Input {
                tag = tag,
                platform = CurrentPlatform,
                format = PlatformFormat,
                framework = Framework.Unity,
                model = SystemInfo.deviceModel
            };
            var session = await NatMLHub.CreateSession(input, accessKey);
            // Fetch graph
            byte[] graph = null;
            if (session.predictor.type == PredictorType.Edge) {
                using var client = new WebClient();
                graph = await client.DownloadDataTaskAsync(session.graph);
            }
            // Create model data
            modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph;
            // Cache
            #pragma warning disable 4014
            SaveToCache(modelData);
            #pragma warning restore 4014
            // Return
            return modelData;
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector] internal Session session;
        [SerializeField, HideInInspector] internal byte[] graph;

        private static string CurrentPlatform {
            get {
                switch (Application.platform) {
                    case RuntimePlatform.Android:       return Platform.Android;
                    case RuntimePlatform.IPhonePlayer:  return Platform.iOS;
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:   return Platform.Linux;
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:     return Platform.macOS;
                    case RuntimePlatform.WebGLPlayer:   return Platform.Web;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer: return Platform.Windows;
                    default:                            return null;
                }
            }
        }

        private static string PlatformFormat {
            get {
                switch (Application.platform) {
                    case RuntimePlatform.Android:       return GraphFormat.TensorFlowLite;
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:     return GraphFormat.CoreML;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer: return GraphFormat.ONNX;
                    default:                            return null;
                }
            }
        }

        private static async Task<MLModelData> FromCache (string tag) {
            // Check
            var cacheName = tag.Replace('/', '_');
            var basePath = Path.Combine(Application.persistentDataPath, @"natml");
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            if (!File.Exists(cachePath))
                return null;
            // Load session
            var session = JsonUtility.FromJson<Session>(File.ReadAllText(cachePath));
            // Check if backwards compatible
            if (!IsBackwardsCompatible(session)) {
                File.Delete(cachePath);
                return null;
            }
            // Load graph
            byte[] graph = null;
            var graphPath = Path.Combine(basePath, session.graph);
            using var stream = new FileStream(graphPath, FileMode.Open, FileAccess.Read);
            graph = new byte[stream.Length];
            await stream.ReadAsync(graph, 0, graph.Length);
            // Create model data
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph;       
            return modelData;
        }

        private static async Task SaveToCache (MLModelData modelData) {
            // Check
            if (modelData == null)
                return;
            // Check type
            if (modelData.session.predictor.type != PredictorType.Edge)
                return;
            // Check status
            if (modelData.session.predictor.status == PredictorStatus.Draft)
                return;
            // Create cache dir
            var cacheName = modelData.tag.Replace('/', '_');
            var basePath = Path.Combine(Application.persistentDataPath, @"natml");
            Directory.CreateDirectory(basePath);
            // Write graph
            var graphName = Guid.NewGuid().ToString();
            var graphPath = Path.Combine(basePath, graphName);
            using var graphStream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await graphStream.WriteAsync(modelData.graph, 0, modelData.graph.Length);
            // Write session
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            var session = modelData.session;
            session.graph = graphName;
            using var predictorStream = new StreamWriter(cachePath);
            await predictorStream.WriteAsync(JsonUtility.ToJson(session));            
        }

        private static bool IsBackwardsCompatible (Session session) {
            if (string.IsNullOrEmpty(session.predictor.type))
                return false;
            if (string.IsNullOrEmpty(session.graph))
                return false;
            if (string.IsNullOrEmpty(session.format))
                return false;
            return true;
        }
        #endregion


        #region --Utilities--
        /// <summary>
        /// Convert a `StreamingAssets` path to an absolute path accessible on the file system.
        /// This function will perform any necessary copying to ensure that the file is accessible.
        /// </summary>
        /// <param name="relativePath">Relative path to target file in `StreamingAssets` folder.</param>
        /// <returns>Absolute path to file.</returns>
        internal static async Task<string> StreamingAssetsToAbsolutePath (string relativePath) {
            // Check persistent
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            // Handle other platform
            if (Application.platform != RuntimePlatform.Android)
                return fullPath;
            // Check persistent
            var persistentPath = Path.Combine(Application.persistentDataPath, relativePath);
            if (File.Exists(persistentPath))
                return persistentPath;
            // Create directories
            var directory = Path.GetDirectoryName(persistentPath);
            Directory.CreateDirectory(directory);
            // Download from APK/AAB
            using var request = UnityWebRequest.Get(fullPath);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new ArgumentException($"Failed to retrieve file from StreamingAssets: {fullPath}", nameof(relativePath));
            // Copy
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            // Return
            return persistentPath;
        }
        #endregion
    }
}