/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public sealed class CreateSessionResponse : MLHubResponse {

        public ResponseData data;

        [Serializable]
        public sealed class ResponseData {
            public Session createSession;
        }
    }
}