/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    internal abstract class MLHubSubscription {
        public string type;
        public string id;
    }
}