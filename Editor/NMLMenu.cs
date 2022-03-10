/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Editor {

    using UnityEditor;

    internal static class NMLMenu {
        
        [MenuItem(@"NatML/NatML 1.0.9")]
        private static void Version () { }

        [MenuItem(@"NatML/NatML 1.0.9", true)]
        private static bool DisableVersion () => false;

        [MenuItem(@"NatML/Get Access Key")]
        private static void OpenAccessKey () => Help.BrowseURL(@"https://hub.natml.ai/profile");

        [MenuItem(@"NatML/Explore Predictors")]
        private static void OpenHub () => Help.BrowseURL(@"https://hub.natml.ai");

        [MenuItem(@"NatML/Open Documentation")]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.natml.ai/unity");

        [MenuItem(@"NatML/Open an Issue")]
        private static void OpenIssue () => Help.BrowseURL(@"https://github.com/natmlx/NatML");

        [MenuItem(@"NatML/Join Discord Community")]
        private static void OpenDiscord () => Help.BrowseURL(@"https://discord.com/invite/y5vwgXkz2f");
    }
}