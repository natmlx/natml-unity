/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Editor.Importers {

    using Hub;

    #if UNITY_2020_2_OR_NEWER
    using UnityEditor.AssetImporters;
    #else
    using UnityEditor.Experimental.AssetImporters;
    #endif

    [ScriptedImporter(1, @"tflite")]
    internal sealed class TFLiteImporter : GraphImporter {

        protected override Session CreateSession (string graphPath) => new Session {
            predictor = new Predictor {
                type = PredictorType.Edge,
                status = PredictorStatus.Draft
            },
            format = GraphFormat.TensorFlowLite
        };
    }
}