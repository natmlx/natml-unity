/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public abstract class GraphRequest {

        public string query;

        protected GraphRequest (string query) => this.query = query;
    }

    [Serializable]
    public abstract class GraphResponse {

        public ResponseError[] errors;

        [Serializable]
        public sealed class ResponseError {
            public string message;
        }
    }
}