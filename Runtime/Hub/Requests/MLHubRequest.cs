/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub.Requests {

    using System;

    [Serializable]
    public abstract class MLHubRequest {

        public string query;

        protected MLHubRequest (string query) => this.query = query;
    }
}