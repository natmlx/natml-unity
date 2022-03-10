/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class CreateSessionRequest : GraphRequest {

        public Variables variables;

        public CreateSessionRequest (Input input) : base(@"
            mutation ($input: CreateSessionInput!) {
                createSession (input: $input) {
                    id
                    predictor {
                        tag
                        type
                        status
                        labels
                        normalization { mean std }
                        aspectMode
                        audioFormat { sampleRate channelCount }
                    }
                    platform
                    graph
                    format
                    flags
                }
            }
        ") => this.variables = new Variables { input = input };

        [Serializable]
        public sealed class Variables {
            public Input input;
        }

        [Serializable]
        public sealed class Input {
            public string tag;
            public string platform;
            public string format;
            public string framework;
            public string model;
        }
    }
    
    [Serializable]
    public sealed class CreateSessionResponse : GraphResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Session createSession;
        }
    }
}