/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Internal {

    using System.IO;
    using UnityEngine;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    internal sealed class NatMLSettings : ScriptableObject {

        #region --Serialized--
        [SerializeField, HideInInspector]
        private string accessKey = string.Empty;
        #endregion


        #region --Client API--
        public string AccessKey => accessKey;

        public static NatMLSettings Settings {
            get {
                #if UNITY_EDITOR
                // Check
                var settings = AssetDatabase.LoadAssetAtPath<NatMLSettings>(SettingsPath);
                if (settings != null)
                    return settings;
                // Create
                Directory.CreateDirectory(SettingsDirectory);
                settings = ScriptableObject.CreateInstance<NatMLSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                return settings;
                #else
                return Resources.Load<NatMLSettings>("Settings/NatML");
                #endif
            }
        }
        #endregion


        #region --Operations--
        private const string SettingsDirectory = @"Assets/NatML/Resources/Settings";
        private static string SettingsPath => $"{SettingsDirectory}/NatML.asset";

        #if UNITY_EDITOR
        internal static SerializedObject GetSerializedSettings () => new SerializedObject(Settings);
        #endif
        #endregion
    }
}