/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Hub;
    using Hub.Editor;
    using Hub.Internal;
    using Hub.Requests;
    using Hub.Types;
    using Internal;

    internal sealed class NatMLBuildHandler : BuildEmbedHelper<MLModelData> {

        protected override BuildTarget[] SupportedTargets => new [] {
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.WebGL,
        };
        private const string CachePath = @"Assets/NMLBuildCache";

        protected override MLModelData[] CreateEmbeds (BuildReport report) {
            // Get embeds
            var embeds = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().SelectMany(type => Attribute.GetCustomAttributes(type, typeof(MLModelDataEmbedAttribute))))
                .Cast<MLModelDataEmbedAttribute>()
                .ToArray();
            // Check
            if (embeds.Length == 0)
                return null;
            // Create sessions
            (PredictorSession, byte[])[] sessions = null;
            var platform = ToPlatform(report.summary.platform);
            var format = GraphFormat.FormatForPlatform(platform);
            var bundle = NatMLHub.GetAppBundle(platform);
            var defaultAccessKey = HubSettings.Instance?.AccessKey;
            try {
                var secret = MLGraphUtils.CreateSecret().Result; // this completes immediately everywhere except web
                var tasks = embeds.Select(embed => Task.Run(async () => {
                    var accessKey = !string.IsNullOrEmpty(embed.accessKey) ? embed.accessKey : defaultAccessKey;
                    var input = new CreatePredictorSessionRequest.Input {
                        tag = embed.tag,
                        platform = platform,
                        format = format,
                        bundle = bundle,
                        secret = secret,
                    };
                    var session = await NatMLHub.CreatePredictorSession(input, accessKey);
                    using var client = new WebClient();
                    var graph = session.graph != null ? await client.DownloadDataTaskAsync(session.graph) : null;
                    return (session, graph);
                }));
                sessions = Task.WhenAll(tasks).Result;
            } catch (Exception ex) {
                Debug.LogWarning($"NatML: Failed to embed model data due to exception: {ex}");
                return new MLModelData[0];
            }
            // Create model data
            Directory.CreateDirectory(CachePath);
            var data = new List<MLModelData>();
            foreach (var (session, graph) in sessions) {
                var modelData = ScriptableObject.CreateInstance<MLModelData>();
                modelData.session = session;
                modelData.graph = graph;
                var name = modelData.session.predictor.tag.Replace("/", "_");
                AssetDatabase.CreateAsset(modelData, $"{CachePath}/{name}.asset");
                data.Add(modelData);
            }
            // Embed
            return data.ToArray();
        }

        protected override void ClearEmbeds (BuildReport report) {
            base.ClearEmbeds(report);
            AssetDatabase.DeleteAsset(CachePath);
        }
    }
}
