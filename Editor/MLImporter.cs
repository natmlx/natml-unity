/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Editor {

    using System.IO;
    using UnityEngine;
    #if UNITY_2020_2_OR_NEWER
    using UnityEditor.AssetImporters;
    #else
    using UnityEditor.Experimental.AssetImporters;
    #endif

    /// <summary>
    /// NatML model importer.
    /// </summary>
    [ScriptedImporter(1, "onnx")]
    internal sealed class MLImporter : ScriptedImporter {

        public override void OnImportAsset (AssetImportContext ctx) {
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.graphData = File.ReadAllBytes(ctx.assetPath);
            ctx.AddObjectToAsset("MLModelData", modelData);
            ctx.SetMainObject(modelData);
        }
    }
}