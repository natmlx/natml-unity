/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal sealed class SubscriptionRequest<TRequest> where TRequest : MLHubRequest {
        public string type;
        public string id;
        public TRequest payload;
    }
}