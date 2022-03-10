/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class RequestPredictionRequest : GraphRequest {

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
            public Feature[] inputs;
            public bool waitUntilCompleted;
        }
    }

    [Serializable]
    public sealed class RequestPredictionResponse : GraphResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Prediction requestPrediction;
        }
    }
}