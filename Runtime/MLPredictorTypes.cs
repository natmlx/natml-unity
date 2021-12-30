/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML {

    using System;
    using UnityEngine;

    public partial class MLModelData {

        /// <summary>
        /// Feature normalization.
        /// </summary>
        [Serializable]
        public struct Normalization {

            #region --Client API--
            /// <summary>
            /// Per-channel normalization means.
            /// </summary>
            [SerializeField]
            public float[] mean;

            /// <summary>
            /// Per-channel normalization standard deviations.
            /// </summary>
            [SerializeField]
            public float[] std;
            #endregion


            #region --Operations--

            public void Deconstruct (out Vector3 outMean, out Vector3 outStd) {
                (outMean, outStd) = (Vector3.zero, Vector3.one);
                if (mean != null)
                    outMean = new Vector3(mean[0], mean[1], mean[2]);
                if (std != null)
                    outStd = new Vector3(std[0], std[1], std[2]);
            }
            #endregion
        }

        /// <summary>
        /// Audio format description for models that work on audio data.
        /// </summary>
        [Serializable]
        public struct AudioFormat {

            #region --Client API--
            /// <summary>
            /// Sample rate.
            /// </summary>
            [SerializeField]
            public int sampleRate;

            /// <summary>
            /// Channel count.
            /// </summary>
            [SerializeField]
            public int channelCount;
            #endregion


            #region --Operations--

            public void Deconstruct (out int outSampleRate, out int outChannelCount) {
                outSampleRate = sampleRate;
                outChannelCount = channelCount;
            }
            #endregion
        }
    }
}