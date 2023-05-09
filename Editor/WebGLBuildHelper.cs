/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    internal sealed class WebGLBuildHelper : IPreprocessBuildWithReport {

        public int callbackOrder => 0;
        private readonly string[] EM_ARGS = new [] {
            @"--bind",
        };

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            if (report.summary.platform != BuildTarget.WebGL)
                return;
            foreach (var arg in EM_ARGS) {
                var standaloneArg = $" {arg} ";
                if (!PlayerSettings.WebGL.emscriptenArgs.Contains(standaloneArg))
                    PlayerSettings.WebGL.emscriptenArgs += standaloneArg;
            }
        }
    }
}