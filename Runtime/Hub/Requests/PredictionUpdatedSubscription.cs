/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal sealed class PredictionUpdatedSubscription : MLHubSubscription {

        public Payload payload;

        [Serializable]
        internal sealed class Payload : MLHubResponse {
            public ResponseData data;
        }

        [Serializable]
        public sealed class ResponseData {
            public Prediction predictionUpdated;
        }
    }
}