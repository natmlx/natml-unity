/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class ReportPredictionRequest : GraphRequest {

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

    [Serializable]
    public sealed class ReportPredictionResponse : GraphResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Prediction reportPrediction;
        }
    }
}