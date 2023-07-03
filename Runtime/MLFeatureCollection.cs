/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML {

    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Lightweight collection of ML features.
    /// Disposing the collection disposes all features within the collection.
    /// </summary>
    public readonly struct MLFeatureCollection<TFeature> : IReadOnlyList<TFeature>, IDisposable where TFeature : IDisposable {

        #region --Client API--
        /// <summary>
        /// Number of features in the feature collection.
        /// </summary>
        public readonly int Count => features.Length;

        /// <summary>
        /// Get the feature at the specified index.
        /// </summary>
        public readonly TFeature this [int index] => features[index];

        /// <summary>
        /// Create a feature collection.
        /// </summary>
        /// <param name="features">Features to add to the collection.</param>
        public MLFeatureCollection (TFeature[] features) => this.features = features;

        /// <summary>
        /// Dispose all features in this collection.
        /// </summary>
        public readonly void Dispose () {
            for (var i = 0; i < features.Length; ++i)
                features[i]?.Dispose();
        }
        #endregion


        #region --Operations--
        private readonly TFeature[] features;

        readonly IEnumerator<TFeature> IEnumerable<TFeature>.GetEnumerator () => (features as IEnumerable<TFeature>).GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator () => features.GetEnumerator();

        public static implicit operator TFeature[] (MLFeatureCollection<TFeature> collection) => collection.features;

        public static implicit operator MLFeatureCollection<TFeature> (TFeature[] features) => new MLFeatureCollection<TFeature>(features);
        #endregion
    }
}