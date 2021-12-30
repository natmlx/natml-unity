/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Editor.Importers {

    using System.IO;
    using UnityEngine;
    using Hub;

    #if UNITY_2020_2_OR_NEWER
    using UnityEditor.AssetImporters;
    #else
    using UnityEditor.Experimental.AssetImporters;
    #endif

    public abstract class GraphImporter : ScriptedImporter {

        public sealed override void OnImportAsset (AssetImportContext ctx) {
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = CreateSession();
            modelData.graph = File.ReadAllBytes(ctx.assetPath);
            ctx.AddObjectToAsset("MLModelData", modelData);
            ctx.SetMainObject(modelData);
        }

        protected abstract Session CreateSession ();
    }
}