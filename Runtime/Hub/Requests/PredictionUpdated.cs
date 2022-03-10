/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class PredictionUpdatedRequest : GraphRequest {

        public Variables variables;

        public PredictionUpdatedRequest (Input input) : base(@"
            subscription ($input: PredictionUpdatedInput!) {
                predictionUpdated (input: $input) {
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
            public string id;
        }
    }

    [Serializable]
    internal sealed class PredictionUpdatedSubscription : GraphSubscription {

        public Payload payload;

        [Serializable]
        internal sealed class Payload : GraphResponse {
            public ResponseData data;
        }

        [Serializable]
        public sealed class ResponseData {
            public Prediction predictionUpdated;
        }
    }
}