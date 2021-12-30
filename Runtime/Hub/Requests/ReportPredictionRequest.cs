/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class ReportPredictionRequest : MLHubRequest {

        public Variables variables;

        public ReportPredictionRequest (Input input) : base(@"
            mutation ($input: ReportPredictionInput!) {
                reportPrediction (input: $input) {
                    id
                    status
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
            public double latency;
            public string date;
        }
    }
}