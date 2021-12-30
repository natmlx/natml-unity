/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class ReportPredictionResponse : MLHubResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Prediction reportPrediction;
        }
    }
}