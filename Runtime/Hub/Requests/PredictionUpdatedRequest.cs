/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal sealed class PredictionUpdatedRequest : MLHubRequest {

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
        internal sealed class Variables {
            public Input input;
        }

        [Serializable]
        public sealed class Input {
            public string id;
        }
    }
}