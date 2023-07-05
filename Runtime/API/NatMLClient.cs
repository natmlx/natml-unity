/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API {

    using System;
    using Graph;
    using Services;

    /// <summary>
    /// NatML API client.
    /// </summary>
    public sealed class NatMLClient {

        #region --Client API--
        /// <summary>
        /// NatML graph API URL.
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
        /// Upload and download files.
        /// </summary>
        public readonly StorageService Storage;

        /// <summary>
        /// Create the NatML client.
        /// </summary>
        /// <param name="accessKey">NatML access key.</param>
        /// <param name="url">NatML Graph API URL.</param>
        public NatMLClient (
            string accessKey = null,
            string url = null
        ) : this(new DotNetClient(url ?? URL, accessKey)) { }

        /// <summary>
        /// Create the NatML client.
        /// </summary>
        /// <param name="client">NatML Graph API client.</param>
        public NatMLClient (IGraphClient client) {
            this.client = client;
            this.Users = new UserService(client);
            this.Predictors = new PredictorService(client);
            this.Graphs = new GraphService(client);
            this.PredictorSessions = new PredictorSessionService(client);
            this.Storage = new StorageService(client);
        }
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        #endregion


        #region --DEPRECATED--
        [Obsolete(@"Deprecated in NatML 1.1.6.")]
        public readonly PredictorSessionService PredictorSessions;
        #endregion
    }
}