/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

#if UNITY_EDITOR
    //#define HUB_DEV
    //#define HUB_STAGING
#endif

namespace NatSuite.ML.Hub {

    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Requests;

    /// <summary>
    /// NatML Hub API.
    /// See the documentation at https://docs.natml.ai/api
    /// </summary>
    public static class NatMLHub {

        /// <summary>
        /// NatML Hub API URL.
        /// </summary>
        public const string URL =
        #if HUB_DEV
        @"http://localhost:8000/graph";
        #elif HUB_STAGING
        @"https://staging.api.natml.ai/graph";
        #else
        @"https://api.natml.ai/graph";
        #endif


        #region --ML Ops--
        /// <summary>
        /// Create a prediction session.
        /// </summary>
        /// <param name="input">Session input.</param>
        /// <param name="accessKey">Hub access key.</param>
        /// <returns></returns>
        public static async Task<Session> CreateSession (CreateSessionRequest.Input input, string accessKey) {
            var request = new CreateSessionRequest(input);
            var response = await Request<CreateSessionRequest, CreateSessionResponse>(request, accessKey);
            return response.data.createSession;
        }

        /// <summary>
        /// Request a Hub prediction.
        /// </summary>
        /// <param name="input">Prediction request.</param>
        /// <returns>Hub prediction.</returns>
        public static async Task<Prediction> RequestPrediction (RequestPredictionRequest.Input input) {
            var request = new RequestPredictionRequest(input);
            var response = await Request<RequestPredictionRequest, RequestPredictionResponse>(request);
            return response.data.requestPrediction;
        }

        /// <summary>
        /// Report an Edge prediction.
        /// </summary>
        /// <param name="input">Prediction report.</param>
        public static async Task<Prediction> ReportPrediction (ReportPredictionRequest.Input input) {
            var request = new ReportPredictionRequest(input);
            var response = await Request<ReportPredictionRequest, ReportPredictionResponse>(request);
            return response.data.reportPrediction;
        }

        /// <summary>
        /// Request an upload URL.
        /// </summary>
        /// <param name="type">Upload type.</param>
        /// <param name="name">File name.</param>
        /// <returns>Pre-signed upload URL.</returns>
        public static async Task<string> UploadURL (string type = UploadType.Feature, string name = default) {
            var input = new UploadURLRequest.Input {
                type = type,
                name = name ?? Guid.NewGuid().ToString()
            };
            var request = new UploadURLRequest(input);
            var response = await Request<UploadURLRequest, UploadURLResponse>(request);
            return response.data.uploadURL;
        }
        #endregion


        #region --Requests---
        /// <summary>
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="accessKey">Access key for requests that require authentication.</param>
        private static async Task Request<TRequest> (
            TRequest request,
            string accessKey = null
        ) where TRequest : MLHubRequest => await Request<TRequest, MLHubResponse>(request, accessKey);

        /// <summary>
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="accessKey">Access key for requests that require authentication.</param>
        /// <returns></returns>
        private static async Task<TResponse> Request<TRequest, TResponse> (
            TRequest request,
            string accessKey = null
        ) where TRequest : MLHubRequest where TResponse : MLHubResponse {
            var payload = JsonUtility.ToJson(request);
            using (var client = new HttpClient())
                using (var content = new StringContent(payload, Encoding.UTF8, @"application/json")) {
                    // Add auth token
                    var authHeader = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
                    client.DefaultRequestHeaders.Authorization = authHeader;
                    // Post
                    using (var response = await client.PostAsync(URL, content)) {
                        // Parse
                        var responseStr = await response.Content.ReadAsStringAsync();
                        var responsePayload = JsonUtility.FromJson<TResponse>(responseStr);
                        // Return
                        if (responsePayload.errors == null)
                            return responsePayload;
                        else
                            throw new InvalidOperationException(responsePayload.errors[0].message);
                    }
                }
        }
        #endregion


        #region --Subscriptions--
        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static Task<PredictionUpdatedSubscription> Subscribe (
            PredictionUpdatedRequest request,
            Predicate<PredictionUpdatedSubscription> predicate
        ) => Subscribe<PredictionUpdatedRequest, PredictionUpdatedSubscription>(request, predicate);

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="predicate"></param>
        /// <param name="onConnect"></param>
        /// <param name="onDisconnect"></param>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        private static async Task<TResponse> Subscribe<TRequest, TResponse> (
            TRequest request,
            Predicate<TResponse> predicate,
            Action onConnect = null,
            Action onDisconnect = null,
            string accessKey = null
        ) where TRequest : MLHubRequest where TResponse : MLHubSubscription {
            TResponse result = default;
            using (var cts = new CancellationTokenSource())
                await Subscribe<TRequest, TResponse>(request, cts.Token, data => {
                    if (predicate(data)) {
                        result = data;
                        cts.Cancel();
                    }
                }, onConnect, onDisconnect);
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onData"></param>
        /// <param name="onConnect"></param>
        /// <param name="onDisconnect"></param>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        private static Task Subscribe <TRequest, TResponse> ( // CHECK // id tracking
            TRequest request,
            CancellationToken cancellationToken,
            Action<TResponse> onData,
            Action onConnect = null,
            Action onDisconnect = null,
            string accessKey = null
        ) where TRequest : MLHubRequest where TResponse : MLHubSubscription => Task.Factory.StartNew(async () => {
            using (var client = new ClientWebSocket()) {
                // Open connection
                var uri = new Uri(URL.Replace(@"http", @"ws"));
                client.Options.AddSubProtocol(@"graphql-ws");
                await client.ConnectAsync(uri, cancellationToken);
                // Connect
                var authorization = !string.IsNullOrEmpty(accessKey) ? $"Bearer {accessKey}" : null;
                var connection = new ConnectionRequest {
                    type = SubscriptionMessage.Initialize,
                    payload = new ConnectionRequest.Payload { 
                        authorization = authorization
                    }
                };
                var connectionPayload = JsonUtility.ToJson(connection);
                await client.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(connectionPayload)),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken
                );
                onConnect?.Invoke();
                // Send request
                var payload = new SubscriptionRequest<TRequest> {
                    type = SubscriptionMessage.Start,
                    id = "1",
                    payload = request
                };
                var payloadStr = JsonUtility.ToJson(payload);
                await client.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(payloadStr)),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken
                );
                // Receive data
                using (var stream = new MemoryStream())
                    while (client.State == WebSocketState.Open) {
                        // Read
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                        var segment = new ArraySegment<byte>(new byte[8192]);
                        WebSocketReceiveResult result;
                        do {
                            result = await client.ReceiveAsync(segment, cancellationToken);
                            stream.Write(segment.Array, segment.Offset, result.Count);
                        } while (!result.EndOfMessage && !cancellationToken.IsCancellationRequested);
                        // Decode
                        stream.Seek(0, SeekOrigin.Begin);
                        if (result.MessageType == WebSocketMessageType.Text) {
                            var responseStr = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
                            var responsePayload = JsonUtility.FromJson<TResponse>(responseStr);
                            switch (responsePayload.type) {
                                case SubscriptionMessage.Acknowledge: break;    // Nop
                                case SubscriptionMessage.KeepAlive: break;      // Nop
                                case SubscriptionMessage.Data:
                                    onData(responsePayload);
                                    break;
                                case SubscriptionMessage.Error:
                                    var errorPayload = JsonUtility.FromJson<SubscriptionRequest<SubscriptionErrorPayload>>(responseStr);
                                    throw new InvalidOperationException(errorPayload.payload.message);
                                case SubscriptionMessage.Complete: break;       // CHECK
                            }
                        }
                        // Check for server close
                        else if (result.MessageType == WebSocketMessageType.Close)
                            break;
                        // Check for cancellation
                        if (cancellationToken.IsCancellationRequested) {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            break;
                        }
                }
                onDisconnect?.Invoke();
            }
        }, TaskCreationOptions.LongRunning).Unwrap();
        #endregion
    }
}