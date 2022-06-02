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
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using Hub;
    using Hub.Requests;

    internal sealed class NatMLBuildHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        public int callbackOrder => 0;
        private readonly BuildTarget[] SupportedTargets = new [] {
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
        };
        private const string CachePath = @"Assets/NatML/Data";

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Check platform
            if (Array.IndexOf(SupportedTargets, report.summary.platform) < 0)
                return;
            // Clear
            ClearEmbeddedData();
            // Check
            var embeds = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().SelectMany(type => Attribute.GetCustomAttributes(type, typeof(MLModelDataEmbedAttribute))))
                .Cast<MLModelDataEmbedAttribute>()
                .ToArray();
            if (embeds.Length == 0)
                return;
            // Create sessions
            var platform = PlatformForBuildTarget(report.summary.platform);
            var format = MLModelData.FormatForPlatform(platform);
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
            // Embed
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            Directory.CreateDirectory(CachePath);
            foreach (var (session, graph) in sessions) {
                var modelData = ScriptableObject.CreateInstance<MLModelData>();
                modelData.session = session;
                modelData.graph = graph;
                var name = modelData.tag.Replace("/", "_");
                AssetDatabase.CreateAsset(modelData, $"{CachePath}/{name}.asset");
                assets.Add(modelData);
            }
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) {
            ClearEmbeddedData();
            AssetDatabase.DeleteAsset(CachePath);
        }

        private static void ClearEmbeddedData () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets == null)
                return;            
            assets.RemoveAll(asset => asset && asset.GetType() == typeof(MLModelData));
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static string PlatformForBuildTarget (BuildTarget target) => target switch {
            BuildTarget.Android             => Platform.Android,
            BuildTarget.iOS                 => Platform.iOS,
            BuildTarget.StandaloneOSX       => Platform.macOS,
            BuildTarget.StandaloneWindows   => Platform.Windows,
            BuildTarget.StandaloneWindows64 => Platform.Windows,
            _                               => null
        };
    }
}
