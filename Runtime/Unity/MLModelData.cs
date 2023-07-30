/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using UnityEngine;
    using API.Types;

    /// <summary>
    /// Self-contained archive with ML model and supplemental data needed to make predictions.
    /// </summary>
    public sealed class MLModelData : ScriptableObject {

        #region --Operations--
        [SerializeField, HideInInspector] internal PredictorSession session;
        [SerializeField, HideInInspector] internal byte[] graph;
        #endregion
    }
}