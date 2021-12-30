/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Extensions {

    /// <summary>
    /// Common utilities for working with predictors.
    /// </summary>
    public static partial class MLPredictorExtensions {

        #region --Async Predictor--
        /// <summary>
        /// Create an async predictor from a predictor.
        /// This typically results in significant performance improvements as predictions are run on a worker thread.
        /// </summary>
        /// <param name="predictor">Backing predictor to create an async predictor with.</param>
        /// <returns>Async predictor which runs predictions on a worker thread.</returns>
        public static MLAsyncPredictor<TOutput> ToAsync<TOutput> (this IMLPredictor<TOutput> predictor) {
            return new MLAsyncPredictor<TOutput>(predictor);
        }
        #endregion
    }
}