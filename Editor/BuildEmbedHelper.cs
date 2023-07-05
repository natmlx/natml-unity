/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// Lightweight utility for embedding scriptable objects into builds.
    /// </summary>
    public abstract class BuildEmbedHelper<T> : IPreprocessBuildWithReport, IPostprocessBuildWithReport where T : ScriptableObject {

        #region --Client API--
        /// <summary>
        /// Callback order for prioritization.
        /// </summary>
        public virtual int callbackOrder => 0;

        /// <summary>
        /// Supported targets for this build embed.
        /// If `null` the helper will embed for all build targets.
        /// </summary>
        protected virtual BuildTarget[] SupportedTargets => null;

        /// <summary>
        /// Create embeds for the build.
        /// </summary>
        protected abstract T[] CreateEmbeds (BuildReport report);

        /// <summary>
        /// Clear embeds after the build.
        /// </summary>
        protected virtual void ClearEmbeds (BuildReport report) => ClearEmbeds<T>();

        /// <summary>
        /// Clear any existing data embedded in the build.
        /// </summary>
        protected static void ClearEmbeds<U> () {
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList();
            if (assets == null)
                return;
            assets.RemoveAll(asset => asset && asset.GetType() == typeof(U));
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }
        #endregion


        #region --Operations--

        void IPreprocessBuildWithReport.OnPreprocessBuild (BuildReport report) {
            // Clear
            ClearEmbeds<T>();
            // Check target
            var targets = SupportedTargets;
            if (!targets?.Contains(report.summary.platform) ?? false)
                return;
            // Create
            var data = CreateEmbeds(report);
            if (data == null)
                return;
            // Embed
            var assets = PlayerSettings.GetPreloadedAssets()?.ToList() ?? new List<UnityEngine.Object>();
            assets.AddRange(data);
            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild (BuildReport report) => ClearEmbeds(report);
        #endregion
    }
}