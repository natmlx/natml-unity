/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections.Concurrent;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Asynchronous preditor which runs predictions on a worker thread.
    /// This predictor wraps an existing predictor and uses it to make predictions.
    /// </summary>
    public sealed class MLAsyncPredictor<TOutput> : IMLPredictor<Task<TOutput>> {

        #region --Client API--
        /// <summary>
        /// Backing predictor used by the async predictor.
        /// </summary>
        public readonly IMLPredictor<TOutput> predictor;

        /// <summary>
        /// Whether the predictor is ready to process new requests immediately.
        /// </summary>
        public bool readyForPrediction {
            [MethodImpl(MethodImplOptions.Synchronized)] get;
            [MethodImpl(MethodImplOptions.Synchronized)] private set;
        }

        /// <summary>
        /// Make a prediction on one or more input features.
        /// </summary>
        /// <param name="inputs">Input features.</param>
        /// <returns>Prediction output.</returns>
        public Task<TOutput> Predict (params MLFeature[] inputs) {
            var tcs = new TaskCompletionSource<TOutput>();
            if (!fence.SafeWaitHandle.IsClosed) {
                queue.Enqueue((inputs, tcs));
                fence.Set();
            }
            else
                tcs.SetCanceled();
            return tcs.Task;
        }

        /// <summary>
        /// Dispose the predictor and release resources.
        /// When this is called, all outstanding prediction requests are cancelled.
        /// </summary>
        public void Dispose () {
            // Stop worker
            cts.Cancel();
            fence.Set();
            task.Wait();
            // Dispose
            cts.Dispose();
            fence.Dispose();
            predictor.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly ConcurrentQueue<(MLFeature[] inputs, TaskCompletionSource<TOutput> tcs)> queue;
        private readonly AutoResetEvent fence;
        private readonly CancellationTokenSource cts;
        private readonly Task task;

        internal MLAsyncPredictor (IMLPredictor<TOutput> predictor) {            
            // Save
            this.predictor = predictor;
            this.queue = new ConcurrentQueue<(MLFeature[], TaskCompletionSource<TOutput>)>();
            this.fence = new AutoResetEvent(false);
            this.cts = new CancellationTokenSource();
            this.task = new Task(() => {
                while (!cts.Token.IsCancellationRequested) {
                    if (queue.TryDequeue(out var request))
                        try {
                            readyForPrediction = false;
                            var result = predictor.Predict(request.inputs);
                            request.tcs.SetResult(result);
                        } catch (Exception ex) {
                            request.tcs.SetException(ex);
                        } finally {
                            readyForPrediction = true;
                        }
                    else
                        fence.WaitOne();                    
                }
                while (queue.TryDequeue(out var request))
                    request.tcs.SetCanceled();
            }, cts.Token, TaskCreationOptions.LongRunning);
            // Start
            task.Start();
            readyForPrediction = true;
        }
        #endregion
    }
}