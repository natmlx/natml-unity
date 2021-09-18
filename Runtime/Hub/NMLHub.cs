/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

#if UNITY_EDITOR
    //#define HUB_DEV
    //#define HUB_STAGING
#endif

namespace NatSuite.ML.Hub {

    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using Features;

    internal static class NMLHub {

        #region --Client API--

        public static async Task<MLModelData> LoadFromHub (string tag, string accessKey) {
            // Build payload
            var fields = "id modelData { labels normalization { mean std } aspectMode audioFormat { sampleRate channelCount } } graphData flags";
            var mutation = $"createSession (tag: $tag, device: $device) {{ {fields} }}";
            var device = new Device {
                model = SystemInfo.deviceModel,
                os = Application.platform.ToString(),
                gfx = SystemInfo.graphicsDeviceType.ToString()
            };
            var payload = JsonUtility.ToJson(new CreateSessionPayload {
                query = $"mutation ($tag: String!, $device: Device!) {{ {mutation} }}",
                variables = new CreateSessionArgs { tag = tag, device = device }
            });
            // Request
            using (var client = new HttpClient())
                using (var content = new StringContent(payload, Encoding.UTF8, "application/json")) {
                    var authHeader = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue("Bearer", accessKey) : null;
                    client.DefaultRequestHeaders.Authorization = authHeader;
                    using (var response = await client.PostAsync(API, content)) {
                        var responseStr = await response.Content.ReadAsStringAsync();
                        var responseDict = JsonUtility.FromJson<CreateSessionResponse>(responseStr);
                        // Check
                        if (responseDict.errors != null)
                            throw new InvalidOperationException(responseDict.errors[0].message);
                        // Create model data
                        var responseData = responseDict.data.createSession;
                        var cachedData = responseData.modelData;
                        cachedData.session = responseData.id;
                        // Download graph data
                        using (var graphClient = new WebClient()) {
                            var graphData = await graphClient.DownloadDataTaskAsync(responseData.graphData);
                            var modelData = Load(tag, cachedData, graphData);
                            return modelData;
                        }
                    }
            }
        }

        public static async Task<MLModelData> LoadFromCache (string tag) {
            // Check
            var cacheName = tag.Replace('/', '_');
            var basePath = Path.Combine(Application.persistentDataPath, "ML");
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            if (!File.Exists(cachePath))
                return default;
            // Load
            var cachedData = JsonUtility.FromJson<MLCachedData>(File.ReadAllText(cachePath));
            var graphPath = Path.Combine(basePath, cachedData.graphData);
            using (var stream = new FileStream(graphPath, FileMode.Open, FileAccess.Read)) {
                var graphData = new byte[stream.Length];
                await stream.ReadAsync(graphData, 0, graphData.Length);
                var modelData = Load(tag, cachedData, graphData);            
                return modelData;
            }
        }

        public static async Task SaveToCache (MLModelData modelData) {
            // Check
            if (modelData == null)
                return;
            // Build data
            var cacheName = modelData.tag.Replace('/', '_');
            var basePath = Path.Combine(Application.persistentDataPath, "ML");
            var cachePath = Path.Combine(basePath, $"{cacheName}.nml");
            var graphName = Guid.NewGuid().ToString();
            var graphPath = Path.Combine(basePath, graphName);
            var cachedData = new MLCachedData {
                session = modelData.session,
                graphData = graphName,
                flags = modelData.flags,
                labels = modelData._labels,
                normalization = modelData._normalization,
                aspectMode = modelData._aspectMode.ToString(),
                audioFormat = modelData._audioFormat,
            };
            // Write
            Directory.CreateDirectory(basePath);
            using (var stream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None))
                await stream.WriteAsync(modelData.graphData, 0, modelData.graphData.Length);
            using (var stream = new StreamWriter(cachePath))
                await stream.WriteAsync(JsonUtility.ToJson(cachedData));
        }

        public static async Task ReportPrediction (string session, double latency) {
            // Check
            if (string.IsNullOrEmpty(session))
                return;
            var date = $"{DateTime.UtcNow:O}";
            // Build payload
            await Task.Yield(); // Force async completion so we don't hold up model prediction
            var report = "reportPrediction (session: $session, latency: $latency, date: $date)";
            var payload = JsonUtility.ToJson(new ReportPredictionPayload {
                query = $"mutation ($session: ID!, $latency: Float!, $date: DateTime) {{ {report} }}",
                variables = new ReportPredictionArgs { session = session, latency = latency, date = date }
            });
            // Request
            using (var client = new HttpClient())
                using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
                    using (await client.PostAsync(API, content)) { }
        }
        #endregion


        #region --Operations--

        private const string API =
        #if HUB_DEV
        @"http://localhost:8000/graph"; 
        #elif HUB_STAGING
        @"https://staging.api.natsuite.io/graph";
        #else
        @"https://api.natsuite.io/graph";
        #endif

        private static MLModelData Load (string tag, MLCachedData cachedData, byte[] graphData) {
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.tag = tag;
            modelData.session = cachedData.session;
            modelData.graphData = graphData;
            modelData.flags = cachedData.flags;
            modelData._labels = cachedData.labels?.Length != 0 ? cachedData.labels : null;
            modelData._normalization = cachedData.normalization;
            modelData._aspectMode = GetAspectMode(cachedData.aspectMode);
            modelData._audioFormat = cachedData.audioFormat;
            return modelData;
        }

        private static MLImageFeature.AspectMode GetAspectMode (string mode) {
            switch (mode) {
                case "SCALE_TO_FIT": case "ScaleToFit": return MLImageFeature.AspectMode.ScaleToFit;
                case "ASPECT_FILL": case "AspectFill": return MLImageFeature.AspectMode.AspectFill;
                case "ASPECT_FIT": case "AspectFit": return MLImageFeature.AspectMode.AspectFit;
                default: return 0;
            }
        }

        [Serializable]
        struct MLCachedData {
            public string session, graphData, aspectMode;
            public int flags;
            public string[] labels;
            public MLModelData.Normalization normalization;
            public MLModelData.AudioFormat audioFormat;
        }

        [Serializable]
        struct Device { public string model, os, gfx; }

        [Serializable]
        struct CreateSessionArgs { public string tag; public Device device; }

        [Serializable]
        struct ReportPredictionArgs { public string session; public double latency; public string date; }

        [Serializable]
        struct CreateSessionPayload { public string query; public CreateSessionArgs variables; }

        [Serializable]
        struct ReportPredictionPayload { public string query; public ReportPredictionArgs variables; }

        [Serializable]
        struct ResponseError { public string message; }

        [Serializable]
        struct CreateSessionResponse { public CreateSessionResponseData data; public ResponseError[] errors; }
        
        [Serializable]
        struct CreateSessionResponseData { public SessionData createSession; }

        [Serializable]
        struct SessionData { public string id, graphData; public MLCachedData modelData; public int flags; }
        #endregion
    }
}