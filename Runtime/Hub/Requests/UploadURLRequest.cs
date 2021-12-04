/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class UploadURLRequest : MLHubRequest {

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
}