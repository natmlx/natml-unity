/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEditor;
    using Internal;

    [FilePath(@"ProjectSettings/NatML.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class NatMLProjectSettings : ScriptableSingleton<NatMLProjectSettings> {

        #region --Data--
        [SerializeField]
        private string accessKey;
        #endregion


        #region --Client API--
        /// <summary>
        /// NatML access key.
        /// </summary>
        internal string AccessKey {
            get => accessKey;
            set {
                accessKey = value;
                Save(false);
            }
        }

        /// <summary>
        /// NatML Function settings from the current project settings.
        /// </summary>
        internal static NatMLSettings CreateSettings () {
            var settings = ScriptableObject.CreateInstance<NatMLSettings>();
            settings.accessKey = instance.AccessKey;
            return settings;
        }
        #endregion


        #region --Operations--

        [InitializeOnLoadMethod]
        private static void OnLoad () => NatMLSettings.Instance = CreateSettings();

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor (EnterPlayModeOptions options) => NatMLSettings.Instance = CreateSettings();
        #endregion
    }
}