/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System.IO;
    using UnityEditor;
    using UnityEngine;

    internal static class NatMLMenu {

        private const int BasePriority = -50;
        
        [MenuItem(@"NatML/NatML 1.0.17", false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"NatML/NatML 1.0.17", true, BasePriority)]
        private static bool DisableVersion () => false;

        [MenuItem(@"NatML/Explore Predictors", false, BasePriority + 1)]
        private static void OpenHub () => Help.BrowseURL(@"https://hub.natml.ai");

        [MenuItem(@"NatML/View NatML Docs", false, BasePriority + 2)]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.natml.ai/unity");

        [MenuItem(@"NatML/Open a NatML Issue", false, BasePriority + 3)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/natmlx/NatML");

        [MenuItem(@"NatML/Clear Predictor Cache", false, BasePriority + 4)]
        private static void ClearCache () {
            Directory.Delete(MLModelData.CachePath, true);
            Debug.Log("NatML: Cleared predictor cache");
        }
    }
}
