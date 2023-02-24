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

        #region --Client API--

        internal static async Task<MLModelData> FromHub (string tag, string accessKey = null) {
            var key = !string.IsNullOrEmpty(accessKey) ? accessKey : HubSettings.Instance?.AccessKey;
            var modelData = await FromEmbed(tag, key) ?? await FromCache(tag) ?? await FromNatML(tag, key);
            return modelData;
        }

        internal static void ClearCache () {
            try {
                Directory.Delete(MLModelData.CachePath, true);
            } catch { } finally {
                Debug.Log("NatML: Cleared predictor cache");
            }
        }
        #endregion


        #region --Operations--
        [SerializeField, HideInInspector] internal PredictorSession session;
        [SerializeField, HideInInspector] internal byte[] graph;
        private static string CachePath => Path.Combine(Application.persistentDataPath, @"natml");
        private static readonly Dictionary<string, MLModelData> embeddedData = new Dictionary<string, MLModelData>();
        private const string Extension = @".nml";

        private void OnEnable () {
            var tag = session?.predictor?.tag;
            if (!string.IsNullOrEmpty(tag) && !Application.isEditor)
                embeddedData.Add(tag, this);
        }

        private static async Task<MLModelData> FromEmbed (string tag, string accessKey) {
            // Load model data
            if (!embeddedData.TryGetValue(tag, out var embed))
                return null;
            // Get secret
            var CacheKey = $"{tag}";
            var entry = PlayerPrefs.GetString(CacheKey, string.Empty);
            // Check secret
            if (!string.IsNullOrEmpty(entry)) {
                var secret = entry.Split(':')[1].Trim(); // yaml :)
                embed.session.secret = secret;
                return embed;
            }
            // Create secret
            var session = await CreateSession(tag, accessKey);
            PlayerPrefs.SetString(CacheKey, $"secret: {session.secret}");
            embed.session.secret = session.secret;
            // Return
            return embed;
        }

        private static async Task<MLModelData> FromCache (string tag) {
            // Check
            var sessionPath = GetSessionPath(tag);
            if (!File.Exists(sessionPath))
                return null;
            // Load session
            var sessionStr = File.ReadAllText(sessionPath);
            var session = JsonUtility.FromJson<PredictorSession>(sessionStr);
            // Check backwards compat
            if (!IsBackwardsCompatible(session)) {
                File.Delete(sessionPath);
                return null;
            }
            // Check graph
            var graphPath = Path.Combine(CachePath, session.graph);
            if (!File.Exists(graphPath)) {
                File.Delete(sessionPath);
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
            var session = await CreateSession(tag, accessKey);
            var graph = await session.graph.CreateStream();
            // Create model data
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = session;
            modelData.graph = graph.ToArray();
            // Check if cacheable
            if (session.predictor.status == PredictorStatus.Draft || Application.platform == RuntimePlatform.WebGLPlayer)
                return modelData;
            // Write graph
            var graphStem = Guid.NewGuid().ToString();
            var graphName = $"{graphStem}{Extension}";
            var graphPath = Path.Combine(CachePath, graphName);
            Directory.CreateDirectory(CachePath);
            using var graphStream = new FileStream(graphPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await graphStream.WriteAsync(modelData.graph, 0, modelData.graph.Length);
            // Write session
            var sessionPath = GetSessionPath(tag);
            session.graph = graphName;
            using var sessionStream = new StreamWriter(sessionPath);
            await sessionStream.WriteAsync(JsonUtility.ToJson(session));
            // Return
            return modelData;
        }

        private static async Task<PredictorSession> CreateSession (string tag, string accessKey) {
            // Create session
            var platform = NatMLHub.CurrentPlatform;
            var format = GraphFormat.FormatForPlatform(platform);
            var bundle = NatMLHub.GetAppBundle();
            var secret = await MLGraphUtils.CreateSecret();
            var input = new CreatePredictorSessionRequest.Input {
                tag = tag,
                platform = platform,
                format = format,
                bundle = bundle,
                device = SystemInfo.deviceModel,
                secret = secret
            };
            var session = await NatMLHub.CreatePredictorSession(input, accessKey);
            // Return
            return session;
        }

        private static string GetSessionPath (string tag) {
            var stem = tag.Replace('/', '_');
            var path = Path.Combine(CachePath, $"{stem}{Extension}");
            return path;
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