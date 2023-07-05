/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System.Collections.Generic;
    using UnityEditor;

    internal static class NatMLSettingsProvider {

        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/NatML", SettingsScope.Project) {
            label = @"NatML",
            guiHandler = searchContext => {
                EditorGUILayout.LabelField(@"NatML Account", EditorStyles.boldLabel);
                NatMLProjectSettings.instance.AccessKey = EditorGUILayout.TextField(@"Access Key", NatMLProjectSettings.instance.AccessKey);
            },
            keywords = new HashSet<string>(new[] { @"NatML", @"NatCorder", @"NatDevice", @"NatShare", @"NatML Hub" }),
        };
    }
}