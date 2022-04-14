/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Editor {

    using System;
    using System.Linq;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    internal sealed class NMLBuildHandler : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        public int callbackOrder => 0;

        public void OnPreprocessBuild (BuildReport report) {
            var embeds = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => 
                    assembly
                    .GetTypes()
                    .SelectMany(type => (MLModelDataEmbedAttribute[])Attribute.GetCustomAttributes(type, typeof(MLModelDataEmbedAttribute)))
                )
                .ToArray();
            var tags = embeds.Select(embed => embed.tag).ToArray();
        }

        public void OnPostprocessBuild (BuildReport report) {

        }
    }
}
