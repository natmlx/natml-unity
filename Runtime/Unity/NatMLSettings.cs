/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// NatML settings.
    /// </summary>
    [DefaultExecutionOrder(Int32.MinValue)]
    internal sealed class NatMLSettings : ScriptableObject {

        #region --Types--
        [Serializable]
        internal class Embed {
            public string fingerprint;
            public byte[] data;
        }
        #endregion


        #region --Client API--
        /// <summary>
        /// Project-wide access key.
        /// </summary>
        [SerializeField, HideInInspector]
        internal string accessKey = string.Empty;

        /// <summary>
        /// Embedded model data.
        /// </summary>
        [SerializeField, HideInInspector]
        internal Embed[] embeds = new Embed[0];

        /// <summary>
        /// Settings instance for this project.
        /// </summary>
        internal static NatMLSettings Instance;
        #endregion


        #region --Operations--

        private void OnEnable () {
            // Check editor
            if (Application.isEditor)
                return;
            // Set singleton
            Instance = this;
        }
        #endregion
    }
}