/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API {

    using Graph;
    using Services;

    /// <summary>
    /// NatML API client.
    /// </summary>
    public sealed class NatMLClient {

        #region --Client API--
        /// <summary>
        /// NatML Graph API URL.
        /// </summary>
        public const string URL = @"https://api.natml.ai/graph";

        /// <summary>
        /// Manage users.
        /// </summary>
        public readonly UserService Users;

        /// <summary>
        /// Manage predictors.
        /// </summary>
        public readonly PredictorService Predictors;

        /// <summary>
        /// Manage predictor graphs.
        /// </summary>
        public readonly GraphService Graphs;

        /// <summary>
        /// Manage predictor endpoints.
        /// </summary>
        public readonly EndpointService Endpoints;

        /// <summary>
        /// Create graph prediction sessions.
        /// </summary>
        public readonly PredictorSessionService PredictorSessions;

        /// <summary>
        /// Create endpoint prediction sessions.
        /// </summary>
        public readonly PredictionSessionService PredictionSessions;

        /// <summary>
        /// Upload and download files.
        /// </summary>
        public readonly StorageService Storage;

        /// <summary>
        /// Create the NatML client.
        /// </summary>
        /// <param name="accessKey">NatML access key.</param>
        /// <param name="url">NatML Graph API URL.</param>
        public NatMLClient (string accessKey = null, string url = null) : this(new DotNetClient(url ?? URL, accessKey)) { }

        /// <summary>
        /// Create the NatML client.
        /// </summary>
        /// <param name="client">NatML Graph API client.</param>
        public NatMLClient (IGraphClient client) {
            this.client = client;
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.Graphs = new GraphService(client);
            this.Endpoints = new EndpointService(client);
            this.PredictorSessions = new PredictorSessionService(client);
            this.PredictionSessions = new PredictionSessionService(client);
            this.Storage = new StorageService(client);
        }
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        #endregion
    }
}