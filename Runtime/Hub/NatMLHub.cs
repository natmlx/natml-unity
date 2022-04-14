/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

//#define NATML_STAGING

namespace NatSuite.ML.Hub {

    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Requests;
    using Subscriptions;

    /// <summary>
    /// NatML API.
    /// </summary>
    public static class NatMLHub {

        /// <summary>
        /// NatML Hub API URL.
        /// </summary>
        public const string URL =
        #if NATML_STAGING
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
        /// Request a Cloud prediction.
        /// </summary>
        /// <param name="input">Prediction request.</param>
        /// <returns>Cloud prediction.</returns>
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

        /// <summary>
        /// Wait for an async Cloud prediction to complete.
        /// </summary>
        /// <param name="predictionId">Prediction ID.</param>
        /// <returns>Completed prediction.</returns>
        public static async Task<Prediction> WaitForPrediction (string predictionId) {
            var input = new PredictionUpdatedRequest.Input { id = predictionId };
            var request = new PredictionUpdatedRequest(input);
            var prediction = await Subscribe<PredictionUpdatedRequest, PredictionUpdatedResponse>(
                request,
                p => p.data.predictionUpdated.status == PredictionStatus.Completed
            );
            return prediction.data.predictionUpdated;
        }
        #endregion


        #region --Requests---
        /// <summary>
        /// Make a request to the NatML API.
        /// </summary>
        /// <typeparam name="TRequest">NatML API request.</typeparam>
        /// <param name="request">Request.</param>
        /// <param name="accessKey">Access key for requests that require authentication.</param>
        public static async Task Request<TRequest> (
            TRequest request,
            string accessKey = null
        ) where TRequest : GraphRequest => await Request<TRequest, GraphResponse>(request, accessKey);

        /// <summary>
        /// Make a request to the NatML API.
        /// </summary>
        /// <typeparam name="TRequest">NatML API request.</typeparam>
        /// <typeparam name="TResponse">NatML API response.</typeparam>
        /// <param name="request">Request.</param>
        /// <param name="accessKey">Access key for requests that require authentication.</param>
        /// <returns>Response.</returns>
        public static async Task<TResponse> Request<TRequest, TResponse> (
            TRequest request,
            string accessKey = null
        ) where TRequest : GraphRequest where TResponse : GraphResponse {
            var payload = JsonUtility.ToJson(request);
            using var client = new HttpClient();
            using var content = new StringContent(payload, Encoding.UTF8, @"application/json");
            // Add auth token
            var authHeader = !string.IsNullOrEmpty(accessKey) ? new AuthenticationHeaderValue(@"Bearer", accessKey) : null;
            client.DefaultRequestHeaders.Authorization = authHeader;
            // Post
            using var response = await client.PostAsync(URL, content);
            // Parse
            var responseStr = await response.Content.ReadAsStringAsync();
            var responsePayload = JsonUtility.FromJson<TResponse>(responseStr);
            // Return
            if (responsePayload.errors == null)
                return responsePayload;
            else
                throw new InvalidOperationException(responsePayload.errors[0].message);
        }
        #endregion


        #region --Subscriptions--
        /// <summary>
        /// Subscribe to the NatML API.
        /// </summary>
        /// <param name="request">Subscription request.</param>
        /// <param name="predicate">Predicate used to close the subscription once a condition is satisfied.</param>
        /// <param name="onConnect">Delegate invoked when connected to NatML API.</param>
        /// <param name="onDisconnect">Delegate invoked when disconnected from NatML API.</param>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns></returns>
        public static async Task<TResponse> Subscribe<TRequest, TResponse> (
            TRequest request,
            Predicate<TResponse> predicate,
            Action onConnect = null,
            Action onDisconnect = null,
            string accessKey = null
        ) where TRequest : GraphRequest where TResponse : GraphResponse {
            TResponse result = default;
            using var cts = new CancellationTokenSource();
            await Subscribe<TRequest, TResponse>(request, data => {
                if (predicate(data)) {
                    result = data;
                    cts.Cancel();
                }
            }, onConnect, onDisconnect, accessKey, cts.Token);
            return result;
        }

        /// <summary>
        /// Subscribe to the NatML API.
        /// </summary>
        /// <param name="request">Subscription request.</param>
        /// <param name="onData">Delegate invoked when new event data is generated..</param>
        /// <param name="onConnect">Delegate invoked when connected to NatML API.</param>
        /// <param name="onDisconnect">Delegate invoked when disconnected from NatML API.</param>
        /// <param name="accessKey">NatML access key.</param>
        /// <returns></returns>
        public static Task Subscribe <TRequest, TResponse> (
            TRequest request,
            Action<TResponse> onData,
            Action onConnect = null,
            Action onDisconnect = null,
            string accessKey = null,
            CancellationToken cancellationToken = default
        ) where TRequest : GraphRequest where TResponse : GraphResponse => Task.Factory.StartNew(async () => {
            // Connect
            using var client = new GQLWSClient(URL.Replace(@"http", @"ws"), accessKey);
            await client.Connect(cancellationToken);
            onConnect?.Invoke();
            // Subscribe
            var id = await client.Subscribe(request, cancellationToken);
            try {
                while (true) {
                    var response = await client.Receive<TResponse>(id, cancellationToken);
                    if (response != null)
                        onData(response);
                }
            } catch (InvalidOperationException ex) {
                throw ex;
            } catch { }
            // Close
            await client.Close(id, cancellationToken);
            onDisconnect?.Invoke();
        }, TaskCreationOptions.LongRunning).Unwrap();
        #endregion
    }
}