/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NatSuite.ML.Editor")]
namespace NatSuite.ML {

    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using UnityEngine;
    using Features;
    using Hub;
    using Hub.Requests;
    using Internal;

    /// <summary>
    /// Self-contained archive with ML model and supplemental data needed to make predictions.
    /// </summary>
    public sealed partial class MLModelData : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// NatML Hub predictor tag.
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
            // Check platform
            if (Consolidate(CurrentPlatform) != Consolidate(session.platform))
                throw new InvalidOperationException($"Cannot deserialize {session.platform} graph on {CurrentPlatform}");
            // Deserialize
            var flags = session.flags;
            switch (session.predictor.type) {
                case PredictorType.Edge:    return new MLEdgeModel(session.id, graph, &flags);
                case PredictorType.Hub:     return new MLHubModel(session.id);
                default:                    throw new InvalidOperationException(@"Invalid predictor type");
            }
        }

        /// <summary>
        /// Fetch ML model data from NatML Hub.
        /// Explore the Hub catalog: https://hub.natml.ai/
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">Hub access key.</param>
        /// <returns>ML model data.</returns>
        public static async Task<MLModelData> FromHub (string tag, string accessKey = null) {
            // Load from cache
            var modelData = await FromCache(tag);
            if (modelData != null)
                return modelData;
            // Load from Hub
            var input = new CreateSessionRequest.Input {
                tag = tag,
                device = new CreateSessionRequest.Device {
                    platform = CurrentPlatform,
                    framework = Framework.Unity,
                    model = SystemInfo.deviceModel
                }
            };
            var session = await NatMLHub.CreateSession(input, accessKey);
            // Fetch graph
            byte[] graph = null;
            if (session.predictor.type == PredictorType.Edge)
                using (var client = new WebClient())
                    graph = await client.DownloadDataTaskAsync(session.graph);
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
                    default: return null;
                }
            }
        }

        private static async Task<MLModelData> FromCache (string tag) {
            // Check
            var cacheName = tag.Replace('/', '_');
            var basePath = Path.Combine(Application.persistentDataPath, "ML");
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            if (!File.Exists(cachePath))
                return null;
            // Load session
            var session = JsonUtility.FromJson<Session>(File.ReadAllText(cachePath));
            // Check if created with older version
            if (string.IsNullOrEmpty(session.predictor.type) || string.IsNullOrEmpty(session.graph)) {
                File.Delete(cachePath);
                return null;
            }
            // Load graph
            byte[] graph = null;
            var graphPath = Path.Combine(basePath, session.graph);
            using (var stream = new FileStream(graphPath, FileMode.Open, FileAccess.Read)) {
                graph = new byte[stream.Length];
                await stream.ReadAsync(graph, 0, graph.Length);
            }
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
            var basePath = Path.Combine(Application.persistentDataPath, "ML");
            Directory.CreateDirectory(basePath);
            // Write graph
            var graphName = Guid.NewGuid().ToString();
            var graphPath = Path.Combine(basePath, graphName);
            using (var stream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None))
                await stream.WriteAsync(modelData.graph, 0, modelData.graph.Length);
            // Write session
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            var session = modelData.session;
            session.graph = graphName;
            using (var stream = new StreamWriter(cachePath))
                await stream.WriteAsync(JsonUtility.ToJson(session));            
        }

        private static string Consolidate (string platform) {
            switch (platform) {
                case Platform.macOS:    return Platform.iOS;
                case "":                return Consolidate(CurrentPlatform); // Backwards compatibility
                default:                return platform;
            }
        }
        #endregion
    }
}