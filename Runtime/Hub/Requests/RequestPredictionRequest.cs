/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;
    using Internal;

    [Serializable]
    public sealed class RequestPredictionRequest : MLHubRequest {

        public Variables variables;

        public RequestPredictionRequest (Input input) : base(@"
            mutation ($input: RequestPredictionInput!) {
                requestPrediction (input: $input) {
                    id
                    status
                    results { data type shape }
                    error
                }
            }
        ") => this.variables = new Variables { input = input };

        [Serializable]
        public sealed class Variables {
            public Input input;
        }

        [Serializable]
        public sealed class Input {
            public string session;
            public MLHubFeature[] inputs;
            public bool waitUntilCompleted;
        }
    }
}