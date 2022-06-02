/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatML.Features {

    using System;
    using System.IO;
    using Hub;
    using Internal;
    
    /// <summary>
    /// </summary>
    internal sealed class MLBinaryFeature : IMLCloudFeature { // INCOMPLETE // DOC

        #region --Client API--
        /// <summary>
        /// Create a binary feature.
        /// </summary>
        /// <param name="data"></param>
        public MLBinaryFeature (byte[] data) {
            
        }

        /// <summary>
        /// Create a binary feature.
        /// </summary>
        /// <param name="path"></param>
        public MLBinaryFeature (string path) {

        }

        /// <summary>
        /// Create a binary feature.
        /// </summary>
        /// <param name="stream"></param>
        public MLBinaryFeature (Stream stream) {

        }

        /// <summary>
        /// Create a binary feature.
        /// </summary>
        /// <param name="feature">Cloud ML feature. This MUST be a `BINARY` feature.</param>
        public MLBinaryFeature (MLCloudFeature feature) {
            // Check
            if (feature.type != DataType.Binary)
                throw new ArgumentException(@"Cloud feature is not an audio feature", nameof(feature));
            // Copy
            using var stream = new MemoryStream();
            feature.data.CopyTo(stream);
            this.data = stream.ToArray();
        }
        #endregion


        #region --Operations--
        private readonly byte[] data;
        private readonly string path;
        private readonly Stream stream;

        MLCloudFeature IMLCloudFeature.Create (in MLFeatureType _) {
            return new MLCloudFeature {
                data = null,
                type = DataType.Binary
            };
        }
        #endregion
    }
}