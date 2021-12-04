/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Editor {

    using UnityEditor;

    internal static class NMLMenu { // CHECK // Version
        
        [MenuItem(@"NatML/NatML 1.0.6")]
        private static void Version () { }

        [MenuItem(@"NatML/NatML 1.0.6", true)]
        private static bool DisableVersion () => false;

        [MenuItem(@"NatML/Get Access Key")]
        private static void OpenAccessKey () => Help.BrowseURL(@"https://hub.natml.ai/profile");

        [MenuItem(@"NatML/Explore Models on Hub")]
        private static void OpenHub () => Help.BrowseURL(@"https://hub.natml.ai");

        [MenuItem(@"NatML/Join Discord Community")]
        private static void OpenDiscord () => Help.BrowseURL(@"https://discord.com/invite/y5vwgXkz2f");

        [MenuItem(@"NatML/View Online Documentation")]
        private static void OpenDocs () => Help.BrowseURL(@"https://docs.natml.ai/unity");
    }
}