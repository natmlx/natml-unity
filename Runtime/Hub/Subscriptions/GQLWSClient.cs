/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Subscriptions {

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// NatML subscription client.
    /// </summary>
    public sealed class GQLWSClient : SubscriptionClient {

        #region --Client API--
        /// <summary>
        /// Create a NatML subscription client.
        /// </summary>
        /// <param name="url">API URL.</param>
        /// <param name="accessKey">NatML access key.</param>
        public GQLWSClient (string url, string accessKey = null) : base(url) => this.accessKey = accessKey;

        /// <summary>
        /// Connect to the NatML subscription server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public override async Task Connect (CancellationToken cancellationToken = default) {
            client.Options.AddSubProtocol(@"graphql-transport-ws");
            await client.ConnectAsync(uri, cancellationToken);
            // Connect
            var authorization = !string.IsNullOrEmpty(accessKey) ? $"Bearer {accessKey}" : null;
            var connection = new SubscriptionMessage<ConnectionPayload> {
                type = @"connection_init",
                payload = new ConnectionPayload { authorization = authorization }
            };
            await SendPayload(connection);
            // Receive ack
            var response = await ReceivePayload<SubscriptionMessage<object>>();
            if (response.type != @"connection_ack")
                throw new InvalidOperationException(@"Server failed to acknowledge subscription connection");
        }

        /// <summary>
        /// Subscribe to an event.
        /// </summary>
        /// <param name="request">Subscription request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Subscription ID to use for receiving from and closing active subscriptions.</returns>
        public override async Task<string> Subscribe<TRequest> (
            TRequest request,
            CancellationToken cancellationToken = default
        ) {
            var payload = new SubscriptionMessage<TRequest> {
                type = @"subscribe",
                id = Guid.NewGuid().ToString(),
                payload = request
            };
            await SendPayload(payload);
            return payload.id;
        }

        /// <summary>
        /// Receive data from a subscription.
        /// </summary>
        /// <param name="id">Subscription ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Event data or `null` if no data was received.</returns>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown when the server is out of data.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when the server raises an error for the subscription.</exception>
        public override async Task<TResponse> Receive<TResponse> (
            string id,
            CancellationToken cancellationToken = default
        ) {
            var messageStr = await ReceivePayload(cancellationToken);
            var message = JsonUtility.FromJson<SubscriptionMessage<TResponse>>(messageStr);
            switch (message.type) {
                case @"next" when message.id == id:
                    return message.payload;
                case @"complete":
                    throw new TaskCanceledException();
                case @"error" when message.id == id:
                    var errorMessage = JsonUtility.FromJson<SubscriptionMessage<Error[]>>(messageStr);
                    throw new InvalidOperationException(errorMessage.payload[0].message);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Close a subscription.
        /// </summary>
        /// <param name="id">Subscription ID.</param>
        public override async Task Close (
            string id,
            CancellationToken cancellationToken = default
        ) {
            var payload = new SubscriptionMessage<object> { type = @"complete", id = id };
            await SendPayload(payload, cancellationToken);
        }
        #endregion


        #region --Operations--
        private readonly string accessKey;

        [Serializable]
        private sealed class SubscriptionMessage<TPayload> {
            public string type;
            public string id; // nullable
            public TPayload payload; // nullable
        }

        [Serializable]
        internal sealed class ConnectionPayload {
            public string authorization;
        }

        [Serializable]
        internal sealed class Error {
            public string message;
        }
        #endregion
    }
}