/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"NatML.ML.Editor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"NatML.ML.Tests")]
namespace NatML {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Features;
    using Hub;
    using Hub.Internal;
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
        public struct Normalization {

            /// <summary>
            /// Per-channel normalization means.
            /// </summary>
            public float[] mean;

            /// <summary>
            /// Per-channel normalization standard deviations.
            /// </summary>
            public float[] std;

            public void Deconstruct (out Vector3 outMean, out Vector3 outStd) {
                outMean = mean?.Length > 0 ? new Vector3(mean[0], mean[1], mean[2]) : Vector3.zero;
                outStd = std?.Length > 0 ? new Vector3(std[0], std[1], std[2]) : Vector3.one;
            }
        }

        /// <summary>
        /// Audio format description for models that work on audio data.
        /// </summary>
        public struct AudioFormat {

            /// <summary>
            /// Sample rate.
            /// </summary>
            public int sampleRate;

            /// <summary>
            /// Channel count.
            /// </summary>
            public int channelCount;

            public void Deconstruct (out int sampleRate, out int channelCount) {
                sampleRate = this.sampleRate;
                channelCount = this.channelCount;
            }
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// NatML predictor tag.
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
        public Normalization normalization => new Normalization {
            mean = session.predictor.normalization.mean,
            std = session.predictor.normalization.std
        };

        /// <summary>
        /// Expected image aspect mode for predictions with this model.
        /// </summary>
        public MLImageFeature.AspectMode aspectMode => session.predictor.aspectMode switch {
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
            sampleRate = session.predictor.audioFormat.sampleRate,
            channelCount = session.predictor.audioFormat.channelCount
        };

        /// <summary>
        /// Deserialize the model data to create an ML model that can be used for prediction.
        /// You MUST dispose the model once you are done with it.
        /// </summary>
        /// <returns>ML model.</returns>
        public unsafe MLModel Deserialize () {
            // Check platform to prevent hard crashes :)
            var platform = NatMLHub.CurrentPlatform;
            if (!string.IsNullOrEmpty(session.format) && session.format != NatMLHub.FormatForPlatform(platform))
                throw new InvalidOperationException($"Cannot deserialize {session.format} graph on {platform}");
            // Deserialize
            return new MLEdgeModel(session.id, graph, session.flags);
        }

        /// <summary>
        /// Fetch ML model data from NatML.
        /// Explore the NatML catalog: https://hub.natml.ai/
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns>ML model data.</returns>
        public static async Task<MLModelData> FromHub (string tag, string accessKey = null) {
            MLModelData result = null;
            // Embed
            if (embeddedData.TryGetValue(tag, out result))
                return result;
            // Cache
            result = await FromCache(tag);
            if (result)
                return result;
            // NatML
            result = await FromNatML(tag, accessKey);
            #pragma warning disable 4014
            SaveToCache(result, CachePath);
            #pragma warning restore 4014
            return result;
        }

        /// <summary>
        /// Fetch ML model data from a local file.
        /// Note that the model data will not contain any supplementary data.
        /// </summary>
        /// <param name="path">Path to ML model file.</param>
        /// <returns>ML model data.</returns>
        public static Task<MLModelData> FromFile (string path) {
            // Check format
            var format = GraphFormatForFile(path);
            if (string.IsNullOrEmpty(format))
                throw new ArgumentException(@"Model file is not a recognized ML model format", nameof(path));
            // Create
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = new Session {
                predictor = new Predictor {
                    status = PredictorStatus.Draft
                },
                format = format
            };
            modelData.graph = File.ReadAllBytes(path);
            // Return
            return Task.FromResult(modelData);
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector] internal Session session;
        [SerializeField, HideInInspector] internal byte[] graph;
        internal static string CachePath => Path.Combine(Application.persistentDataPath, @"natml");
        private static readonly Dictionary<string, MLModelData> embeddedData = new Dictionary<string, MLModelData>();
        internal const string PredictorExtension = @".nml";
        internal const string GraphExtension = @".nmlb";

        private void OnEnable () {
            if (session != null && !Application.isEditor)
                embeddedData.Add(tag, this);
        }

        private static async Task<MLModelData> FromCache (string tag) {
            // Check
            var cacheName = tag.Replace('/', '_');
            var predictorPath = Path.Combine(CachePath, $"{cacheName}{PredictorExtension}");
            if (!File.Exists(predictorPath))
                return null;
            // Load session
            var session = JsonUtility.FromJson<Session>(File.ReadAllText(predictorPath));
            if (!IsBackwardsCompatible(session)) {
                File.Delete(predictorPath);
                return null;
            }
            // Check graph
            var graphPath = Path.Combine(CachePath, session.graph);
            if (!File.Exists(graphPath)) {
                File.Delete(predictorPath);
                return null;
            }
            // Load graph
            using var stream = new FileStream(graphPath, FileMode.Open, FileAccess.Read);
            var graph = new byte[stream.Length];
            await stream.ReadAsync(graph, 0, graph.Length);
            // Create model data
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph;       
            return modelData;
        }

        private static async Task<MLModelData> FromNatML (string tag, string accessKey) {
            // Load from NatML
            accessKey = !string.IsNullOrEmpty(accessKey) ? accessKey : HubSettings.Instance?.AccessKey;
            var platform = NatMLHub.CurrentPlatform;
            var input = new CreateSessionRequest.Input {
                tag = tag,
                platform = platform,
                format = NatMLHub.FormatForPlatform(platform),
                bundle = Application.identifier,
                framework = Framework.Unity,
                device = SystemInfo.deviceModel
            };
            var session = await NatMLHub.CreateSession(input, accessKey);
            var graph = await DownloadGraph(session.graph);
            // Create model data
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph;
            // Return
            return modelData;
        }

        private static async Task SaveToCache (MLModelData modelData, string cachePath) {
            // Check model data
            if (modelData == null)
                return;
            // Check path
            if (string.IsNullOrWhiteSpace(cachePath))
                return;
            // Check status
            if (modelData.session.predictor.status == PredictorStatus.Draft)
                return;
            // Check if exists
            var cacheName = modelData.tag.Replace('/', '_');
            var predictorPath = Path.Combine(cachePath, $"{cacheName}{PredictorExtension}");
            if (File.Exists(predictorPath))
                return;
            // Write graph
            var graphName = $"{Guid.NewGuid().ToString()}{GraphExtension}";
            var graphPath = Path.Combine(cachePath, graphName);
            Directory.CreateDirectory(cachePath);
            using var graphStream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await graphStream.WriteAsync(modelData.graph, 0, modelData.graph.Length);
            // Write session
            var session = modelData.session;
            session.graph = graphName;
            using var predictorStream = new StreamWriter(predictorPath);
            await predictorStream.WriteAsync(JsonUtility.ToJson(session));            
        }

        private static async Task<byte[]> DownloadGraph (string url) {
            using var request = UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
                await Task.Yield();
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException(request.error);
            return request.downloadHandler.data;
        }

        private static string GraphFormatForFile (string path) => Path.GetExtension(path) switch {
            ".mlmodel"  => GraphFormat.CoreML,
            ".onnx"     => GraphFormat.ONNX,
            ".tflite"   => GraphFormat.TensorFlowLite,
            _           => null,
        };

        private static bool IsBackwardsCompatible (Session session) {
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
        /// <returns>Absolute path to file or `null` if the file cannot be found.</returns>
        internal static async Task<string> StreamingAssetsToAbsolutePath (string relativePath) {
            // Check persistent
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            // Handle other platform
            if (Application.platform != RuntimePlatform.Android)
                return File.Exists(fullPath) ? fullPath : null;
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
                return null;
            // Copy
            File.WriteAllBytes(persistentPath, request.downloadHandler.data);
            // Return
            return persistentPath;
        }
        #endregion
    }
}