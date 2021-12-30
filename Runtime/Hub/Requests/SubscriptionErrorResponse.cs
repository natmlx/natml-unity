/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal sealed class SubscriptionErrorPayload : MLHubRequest {
        public string message;
        public SubscriptionErrorPayload () : base(null) { }
    }
}