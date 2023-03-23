/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using API;
    using API.Types;
    using Hub.Editor;
    using Hub.Internal;
    using Internal;

    internal sealed class NatMLSettingsEmbed : BuildEmbedHelper<NatMLSettings> {

        protected override BuildTarget[] SupportedTargets => new [] {
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.WebGL,
        };
        private const string CachePath = @"Assets/NMLBuildCache";

        protected override NatMLSettings[] CreateEmbeds (BuildReport report) {
            // Create cache path
            Directory.CreateDirectory(CachePath);
            // Create settings
            var settings = CreateSettings();
            // Get embeds
            var embeds = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly
                    .GetTypes()
                    .SelectMany(type => Attribute.GetCustomAttributes(type, typeof(MLEdgeModel.EmbedAttribute)))
                )
                .Cast<MLEdgeModel.EmbedAttribute>()
                .ToArray();
            var defaultAccessKey = HubSettings.Instance.AccessKey;
            var format = GetFormat(report.summary.platform);
            var secret = MLEdgeModel.CreateSecret().Result; // this completes immediately everywhere except web
            settings.embeds = Task.WhenAll(embeds.Select(e => Task.Run(async () => {
                var accessKey = !string.IsNullOrEmpty(e.accessKey) ? e.accessKey : defaultAccessKey;
                var client = new NatMLClient(accessKey);
                var session = await client.PredictorSessions.Create(e.tag, format, secret);
                var graphStream = await client.Storage.Download(session.graph);
                var graph = graphStream.ToArray();
                var embed = new NatMLSettings.Embed { fingerprint = session.fingerprint, data = graph };
                return embed;
            }))).Result;
            // Write settings
            AssetDatabase.CreateAsset(settings, $"{CachePath}/NatML.asset");
            return new [] { settings };
        }

        protected override void ClearEmbeds (BuildReport report) {
            base.ClearEmbeds(report);
            AssetDatabase.DeleteAsset(CachePath);
        }

        internal static NatMLSettings CreateSettings () {
            var settings = ScriptableObject.CreateInstance<NatMLSettings>();
            settings.accessKey = HubSettings.Instance.AccessKey;
            return settings;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnLoad () => NatMLSettings.Instance = CreateSettings();

        private static GraphFormat GetFormat (BuildTarget target) => target switch {
            BuildTarget.Android             => GraphFormat.TFLite,
            BuildTarget.iOS                 => GraphFormat.CoreML,
            BuildTarget.StandaloneOSX       => GraphFormat.CoreML,
            BuildTarget.StandaloneWindows   => GraphFormat.ONNX,
            BuildTarget.StandaloneWindows64 => GraphFormat.ONNX,
            BuildTarget.WebGL               => GraphFormat.ONNX,
            _                               => 0,
        };
    }
}