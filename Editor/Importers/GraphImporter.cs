/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor.Importers {
    
    using System.IO;
    using UnityEngine;
    using UnityEditor.AssetImporters;
    using API.Types;

    public abstract class GraphImporter : ScriptedImporter {

        public sealed override void OnImportAsset (AssetImportContext ctx) {
            var modelData = ScriptableObject.CreateInstance<MLModelData>();
            modelData.session = CreateSession();
            modelData.graph = File.ReadAllBytes(ctx.assetPath);
            ctx.AddObjectToAsset("MLModelData", modelData);
            ctx.SetMainObject(modelData);
        }

        protected abstract PredictorSession CreateSession ();
    }
}