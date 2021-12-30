/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal sealed class ConnectionRequest {
        public string type;
        public Payload payload;

        [Serializable]
        internal sealed class Payload {
            public string authorization;
        }
    }
}