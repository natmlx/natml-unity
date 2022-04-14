/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Editor {

    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using Internal;

    internal static class NMLSettingsProvider {

        [SettingsProvider]
        public static SettingsProvider CreateProvider () => new SettingsProvider(@"Project/NatML", SettingsScope.Project) {
            label = @"NatML",
            // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
            guiHandler = searchContext => {
                var settings = NatMLSettings.GetSerializedSettings();
                EditorGUILayout.PropertyField(settings.FindProperty("accessKey"), new GUIContent("Access Key"));
                settings.ApplyModifiedProperties();
            },
            keywords = new HashSet<string>(new[] { "NatML" }),
        };
    }
}
