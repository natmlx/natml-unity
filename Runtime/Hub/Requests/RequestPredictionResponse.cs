/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class RequestPredictionResponse : MLHubResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Prediction requestPrediction;
        }
    }
}