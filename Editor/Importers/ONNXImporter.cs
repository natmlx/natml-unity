/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor.Importers {

    using UnityEditor.AssetImporters;
    using API.Types;

    [ScriptedImporter(1, @"onnx")]
    internal sealed class ONNXImporter : GraphImporter {

        protected override PredictorSession CreateSession () => new PredictorSession {
            format = GraphFormat.ONNX
        };
    }
}