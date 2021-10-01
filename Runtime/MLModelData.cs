/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"NatSuite.ML.Editor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"NatSuite.ML.Hub")]
namespace NatSuite.ML {

    using System;
    using System.IO;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Features;
    using Hub;

    /// <summary>
    /// Self-contained archive with ML model and supplemental data needed to make predictions with the model.
    /// </summary>
    public sealed class MLModelData : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// Model classification labels.
        /// This is `null` if the model does not have any classification labels.
        /// </summary>
        public string[] labels => _labels?.Length > 0 ? _labels : default;

        /// <summary>
        /// Expected feature normalization for predictions with this model.
        /// </summary>
        public Normalization normalization => _normalization;

        /// <summary>
        /// Expected image aspect mode for predictions with this model.
        /// </summary>
        public MLImageFeature.AspectMode aspectMode => _aspectMode;

        /// <summary>
        /// Expected audio format for predictions with this model.
        /// </summary>
        public AudioFormat audioFormat => _audioFormat;

        /// <summary>
        /// Deserialize the model data to create an ML model that can be used for prediction.
        /// You MUST dispose the model once you are done with it.
        /// </summary>
        /// <returns>ML model.</returns>
        public unsafe MLModel Deserialize () {
            var flags = this.flags;
            var model = new MLModel(graphData, &flags);
            if (!string.IsNullOrEmpty(session)) {
                (model as IHubPredictor).Prediction += latency => {
                    #pragma warning disable 4014
                    NMLHub.ReportPrediction(session, latency);
                    #pragma warning restore 4014
                };
            }
            return model;
        }

        /// <summary>
        /// Fetch ML model data from NatML hub.
        /// </summary>
        /// <param name="tag">Model tag.</param>
        /// <param name="accessKey">Hub access key.</param>
        /// <param name="analytics">Enable performance analytics reporting.</param>
        /// <param name="cache">Cache model data on the device.</param>
        /// <returns>ML model data.</returns>
        public static async Task<MLModelData> FromHub (
            string tag,
            string accessKey = null,
            bool analytics = true,
            bool cache = true
        ) {
            var modelData = cache ? await NMLHub.LoadFromCache(tag) : default;
            if (modelData == null) {
                modelData = await NMLHub.LoadFromHub(tag, accessKey);
                #pragma warning disable 4014
                if (cache)
                    NMLHub.SaveToCache(modelData);
                #pragma warning restore 4014
            }
            if (!analytics)
                modelData.session = null;
            return modelData;
        }

        /// <summary>
        /// Fetch ML model data from a local file.
        /// </summary>
        /// <param name="path">Path to ONNX model file.</param>
        /// <returns>ML model data.</returns>
        public static Task<MLModelData> FromFile (string path) {
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.graphData = File.ReadAllBytes(path);
            return Task.FromResult(modelData);
        }

        /// <summary>
        /// Fetch ML model data from StreamingAssets.
        /// </summary>
        /// <param name="path">Relative path to ONNX model file in `StreamingAssets` folder.</param>
        /// <returns>ML model data.</returns>
        public static async Task<MLModelData> FromStreamingAssets (string relativePath) {
            // Check for direct extraction
            var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
            if (Application.platform != RuntimePlatform.Android)
                return await FromFile(fullPath);
            // Extract from app archive
            using (var request = UnityWebRequest.Get(fullPath)) {
                request.SendWebRequest();
                while (!request.isDone)
                    await Task.Yield();
                if (request.isNetworkError || request.isHttpError)
                    throw new ArgumentException($"Failed to create MLModelData from StreamingAssets: {relativePath}");
                var modelData = ScriptableObject.CreateInstance<MLModelData>();
                modelData.graphData = request.downloadHandler.data;
                return modelData;
            }
        }
        #endregion


        #region --Operations--
        internal string tag;
        internal string session;
        [SerializeField, HideInInspector] internal byte[] graphData;
        [SerializeField, HideInInspector] internal int flags;
        [SerializeField, HideInInspector] internal string[] _labels;
        [SerializeField, HideInInspector] internal Normalization _normalization;
        [SerializeField, HideInInspector] internal MLImageFeature.AspectMode _aspectMode;
        [SerializeField, HideInInspector] internal AudioFormat _audioFormat;

        [Serializable]
        public struct Normalization {
            [SerializeField] public float[] mean;
            [SerializeField] public float[] std;

            public void Deconstruct (out Vector3 outMean, out Vector3 outStd) {
                (outMean, outStd) = (Vector3.zero, Vector3.one);
                if (mean != null)
                    outMean = new Vector3(mean[0], mean[1], mean[2]);
                if (std != null)
                    outStd = new Vector3(std[0], std[1], std[2]);
            }
        }

        [Serializable]
        public struct AudioFormat {
            [SerializeField] public int sampleRate;
            [SerializeField] public int channelCount;

            public void Deconstruct (out int outSampleRate, out int outChannelCount) {
                outSampleRate = sampleRate;
                outChannelCount = channelCount;
            }
        }
        #endregion
    }
}