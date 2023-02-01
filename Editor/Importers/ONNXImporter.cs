/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor.Importers {

    using UnityEditor.AssetImporters;
    using Hub;
    using Hub.Types;

    [ScriptedImporter(1, @"onnx")]
    internal sealed class ONNXImporter : GraphImporter {

        protected override PredictorSession CreateSession (string graphPath) => new PredictorSession {
            format = GraphFormat.ONNX
        };
    }
}