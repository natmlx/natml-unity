/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Hub;
    using Hub.Editor;
    using Hub.Requests;
    using Hub.Internal;

    internal sealed class NatMLBuildHandler : BuildEmbedHelper<MLModelData> {

        protected override BuildTarget[] SupportedTargets => new [] {
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
        };
        private const string CachePath = @"Assets/NatML/Data";

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
            var platform = ToPlatform(report.summary.platform);
            var format = NatMLHub.FormatForPlatform(platform);
            var defaultAccessKey = HubSettings.Instance?.AccessKey;
            var tasks = embeds.Select(embed => Task.Run(async () => {
                var accessKey = !string.IsNullOrEmpty(embed.accessKey) ? embed.accessKey : defaultAccessKey;
                var input = new CreateSessionRequest.Input {
                    tag = embed.tag,
                    platform = platform,
                    format = format,
                    framework = Framework.Unity
                };
                var session = await NatMLHub.CreateSession(input, accessKey);
                using var client = new WebClient();
                var graph = session.graph != null ? await client.DownloadDataTaskAsync(session.graph) : null;
                return (session, graph);
            }));
            var sessions = Task.WhenAll(tasks).Result;
            // Create model data
            Directory.CreateDirectory(CachePath);
            var data = new List<MLModelData>();
            foreach (var (session, graph) in sessions) {
                var modelData = ScriptableObject.CreateInstance<MLModelData>();
                modelData.session = session;
                modelData.graph = graph;
                var name = modelData.tag.Replace("/", "_");
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
