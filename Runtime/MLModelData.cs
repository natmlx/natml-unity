/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;
    using Features;
    using Hub;
    using Hub.Internal;
    using Hub.Requests;
    using Hub.Types;
    using Internal;

    /// <summary>
    /// Self-contained archive with ML model and supplemental data needed to make predictions.
    /// </summary>
    public sealed class MLModelData : ScriptableObject {

        #region --Operations--
        [SerializeField, HideInInspector] internal PredictorSession session;
        [SerializeField, HideInInspector] internal byte[] graph;
        internal static string CachePath => Path.Combine(Application.persistentDataPath, @"natml");
        private static readonly Dictionary<string, MLModelData> embeddedData = new Dictionary<string, MLModelData>();
        internal const string PredictorExtension = @".nml";
        internal const string GraphExtension = @".nmlb";

        private void OnEnable () {
            var tag = session?.predictor?.tag;
            if (!string.IsNullOrEmpty(tag) && !Application.isEditor)
                embeddedData.Add(tag, this);
        }

        internal static async Task<MLModelData> FromHub (string tag, string accessKey = null) {
            // Load from embed -> cache -> Hub
            var modelData = FromEmbed(tag) ?? await FromCache(tag) ?? await FromNatML(tag, accessKey);
            // And cache :)
            await SaveToCache(modelData);
            // Return
            return modelData;
        }

        private static MLModelData FromEmbed (string tag) => embeddedData.TryGetValue(tag, out var embed) ? embed : null;

        private static async Task<MLModelData> FromCache (string tag) {
            // Check
            var cacheName = tag.Replace('/', '_');
            var predictorPath = Path.Combine(CachePath, $"{cacheName}{PredictorExtension}");
            if (!File.Exists(predictorPath))
                return null;
            // Load session
            var session = JsonUtility.FromJson<PredictorSession>(File.ReadAllText(predictorPath));
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
            var secret = await MLGraphUtils.CreateSecret();
            var input = new CreatePredictorSessionRequest.Input {
                tag = tag,
                platform = platform,
                format = GraphFormat.FormatForPlatform(platform),
                bundle = NatMLHub.GetAppBundle(),
                device = SystemInfo.deviceModel,
                secret = secret
            };
            var session = await NatMLHub.CreatePredictorSession(input, accessKey);
            var graph = await session.graph.CreateStream();
            // Create model data
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph.ToArray();
            // Return
            return modelData;
        }

        private static async Task SaveToCache (MLModelData modelData) {
            // Check tag
            var tag = modelData?.session?.predictor?.tag;
            if (string.IsNullOrEmpty(tag))
                return;
            // Check status
            if (modelData.session.predictor.status == PredictorStatus.Draft)
                return;
            // Check platform
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return;
            // Check if embedded
            if (embeddedData.ContainsKey(tag))
                return;
            // Check if cached
            var cachePath = CachePath;
            var cacheName = tag.Replace('/', '_');
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

        private static bool IsBackwardsCompatible (PredictorSession session) {
            if (string.IsNullOrEmpty(session.graph))
                return false;
            if (string.IsNullOrEmpty(session.format))
                return false;
            return true;
        }
        #endregion
    }
}