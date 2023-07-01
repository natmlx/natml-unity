/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Internal;

    internal static class NatMLMenu {

        private const int BasePriority = -50;
        
        [MenuItem(@"NatML/NatML " + NatML.Version, false, BasePriority)]
        private static void Version () { }

        [MenuItem(@"NatML/NatML " + NatML.Version, true, BasePriority)]
        private static bool EnableVersion () => false;

        [MenuItem(@"NatML/Get Access Key", false, BasePriority + 1)]
        private static void OpenAccessKey () => Help.BrowseURL(@"https://hub.natml.ai/account/developers");

        [MenuItem(@"NatML/Explore Predictors", false, BasePriority + 2)]
        private static void OpenHub () => Help.BrowseURL(@"https://hub.natml.ai");

        [MenuItem(@"NatML/Join Discord Community", false, BasePriority + 3)]
        private static void OpenDiscord () => Help.BrowseURL(@"https://natml.ai/community");

        [MenuItem(@"NatML/View NatML Docs", false, BasePriority + 4)]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.natml.ai/unity");

        [MenuItem(@"NatML/Open a NatML Issue", false, BasePriority + 5)]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/natmlx/natml-unity");

        [MenuItem(@"NatML/Clear Predictor Cache", false, BasePriority + 6)]
        private static void ClearCache () {
            MLEdgeModel.ClearCache();
            Debug.Log("NatML: Cleared predictor cache");
        }
    }
}
