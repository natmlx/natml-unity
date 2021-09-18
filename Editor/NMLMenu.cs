/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Editor {

    using UnityEditor;

    internal static class NMLMenu { // CHECK // Version
        
        [MenuItem("NatML/NatML 1.0.3")]
        private static void Version () { }

        [MenuItem("NatML/NatML 1.0.3", true)]
        private static bool DisableVersion () => false;

        [MenuItem("NatML/View Model Catalog")]
        private static void OpenCatalog () => Help.BrowseURL(@"https://hub.natsuite.io/catalog");

        [MenuItem("NatML/Get Access Key")]
        private static void OpenAccessKey () => Help.BrowseURL(@"https://hub.natsuite.io/profile");

        [MenuItem("NatML/Get Support on Discord")]
        private static void OpenDiscord () => Help.BrowseURL(@"https://discord.com/invite/y5vwgXkz2f");
    }
}