/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Editor.Importers {

    using UnityEditor.AssetImporters;
    using API.Types;

    [ScriptedImporter(1, @"tflite")]
    internal sealed class TFLiteImporter : GraphImporter {

        protected override PredictorSession CreateSession () => new PredictorSession {
            format = GraphFormat.TFLite
        };
    }
}