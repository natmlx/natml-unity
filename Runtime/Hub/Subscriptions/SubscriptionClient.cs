/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub.Subscriptions {

    using System;
    using System.IO;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Requests;

    /// <summary>
    /// NatML subscription client.
    /// </summary>
    public abstract class SubscriptionClient : IDisposable {

        #region --Client API--
        /// <summary>
        /// Connect to the NatML subscription server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        public abstract Task Connect (CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribe to an event.
        /// </summary>
        /// <param name="request">Subscription request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Subscription ID to use for receiving from and closing active subscriptions.</returns>
        public abstract Task<string> Subscribe<TRequest> (
            TRequest request,
            CancellationToken cancellationToken = default
        ) where TRequest : GraphRequest;

        /// <summary>
        /// Receive data from a subscription.
        /// </summary>
        /// <param name="id">Subscription ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Event data or `null` if no data was received.</returns>
        /// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown when the server is out of data.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when the server raises an error for the subscription.</exception>
        public abstract Task<TResponse> Receive<TResponse> (
            string id,
            CancellationToken cancellationToken = default
        ) where TResponse : GraphResponse;

        /// <summary>
        /// Close a subscription.
        /// </summary>
        /// <param name="id">Subscription ID.</param>
        public abstract Task Close (
            string id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Dispose the client and release resources.
        /// </summary>
        public virtual void Dispose () {
            client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            client.Dispose();
            stream.Dispose();
        }
        #endregion


        #region --Operations--
        protected readonly Uri uri;
        protected readonly ClientWebSocket client;
        private readonly MemoryStream stream;
        private readonly ArraySegment<byte> segment;

        protected SubscriptionClient (string url) {
            this.uri = new Uri(url);
            this.client = new ClientWebSocket();
            this.stream = new MemoryStream();
            this.segment = new ArraySegment<byte>(new byte[8192]);
        }

        protected Task SendPayload<T> (T payload, CancellationToken cancellationToken = default) => client.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))),
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );

        protected async Task<T> ReceivePayload<T> (CancellationToken cancellationToken = default) {
            var response = await ReceivePayload(cancellationToken);
            return JsonUtility.FromJson<T>(response);
        }

        protected async Task<string> ReceivePayload (CancellationToken cancellationToken = default) {
            // Check
            if (client.State != WebSocketState.Open)
                throw new InvalidOperationException(@"WebSocket has been closed");
            // Read
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0);
            WebSocketReceiveResult result;
            do {
                result = await client.ReceiveAsync(segment, cancellationToken);
                stream.Write(segment.Array, segment.Offset, result.Count);
            } while (!result.EndOfMessage && !cancellationToken.IsCancellationRequested);
            // Check
            cancellationToken.ThrowIfCancellationRequested();
            if (result.MessageType == WebSocketMessageType.Close)
                throw new TaskCanceledException(@"WebSocket has been closed by server");
            // Parse
            stream.Seek(0, SeekOrigin.Begin);
            var response = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
            return response;
        }
        #endregion
    }
}