/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class UploadURLRequest : GraphRequest {

        public Variables variables;

        public UploadURLRequest (Input input) : base(@"
            query ($input: UploadURLInput!) {
                uploadURL (input: $input)
            }
        ") => this.variables = new Variables { input = input };

        [Serializable]
        public sealed class Variables {
            public Input input;
        }

        [Serializable]
        public sealed class Input {
            public string name;
            public string type;
        }
    }

    [Serializable]
    public sealed class UploadURLResponse : GraphResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public string uploadURL;
        }
    }
}