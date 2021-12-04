/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public abstract class MLHubResponse {

        public ResponseError[] errors;

        [Serializable]
        public sealed class ResponseError {
            public string message;
        }
    }
}