/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Features;
    using Hub;
    using Hub.Requests;

    /// <summary>
    /// Server-side ML model capable of making predictions on features.
    /// Hub models are currently NOT thread safe, so predictions MUST be made from one thread at a time.
    /// </summary>
    public sealed class MLHubModel : MLModel {

        #region --Client API--
        /// <summary>
        /// Make a server-side prediction on one or more input features.
        /// </summary>
        /// <param name="inputs">Input features.</param>
        /// <returns>Output features.</returns>
        public async Task<MLFeature[]> Predict (params MLHubFeature[] inputs) {
            // Request prediction
            var input = new RequestPredictionRequest.Input {
                session = session,
                inputs = inputs,
                waitUntilCompleted = true
            };
            var prediction = await NatMLHub.RequestPrediction(input);
            // Check for errors
            if (!string.IsNullOrEmpty(prediction.error))
                throw new InvalidOperationException(prediction.error);
            // Decode
            var results = prediction.results.Select(Deserialize).ToArray();
            return results;
        }
        #endregion


        #region --Operations--

        unsafe internal MLHubModel (string session) : base(session) {
            this.inputs = null;
            this.outputs = null;
            this.metadata = new Dictionary<string, string>();
        }

        private static MLFeature Deserialize (MLHubFeature input) {
            switch (input.type) {
                case HubDataType.Float:     return DeserializeArray<float>(input);
                case HubDataType.Double:    return DeserializeArray<double>(input);
                case HubDataType.SByte:     return DeserializeArray<sbyte>(input);
                case HubDataType.Short:     return DeserializeArray<short>(input);
                case HubDataType.Int:       return DeserializeArray<int>(input);
                case HubDataType.Long:      return DeserializeArray<long>(input);
                case HubDataType.Byte:      return DeserializeArray<byte>(input);
                case HubDataType.UShort:    return DeserializeArray<ushort>(input);
                case HubDataType.UInt:      return DeserializeArray<uint>(input);
                case HubDataType.ULong:     return DeserializeArray<ulong>(input);
                case HubDataType.Image:     return DeserializeImage(input);
                case HubDataType.Audio:     return DeserializeAudio(input);
                case HubDataType.String:    return DeserializeText(input);
                default:                    throw new ArgumentException($"Cannot deserialize invalid feature type: {input.type}", nameof(input));
            }
        }

        private static unsafe MLArrayFeature<T> DeserializeArray<T> (MLHubFeature input) where T : unmanaged {
            var rawData = Convert.FromBase64String(input.data);
            var data = new T[rawData.Length / sizeof(T)];
            var shape = input.shape.Length == 0 ? new [] { 1 } : input.shape;
            Buffer.BlockCopy(rawData, 0, data, 0, rawData.Length);
            return new MLArrayFeature<T>(data, shape);
        }

        private static MLImageFeature DeserializeImage (MLHubFeature input) {
            var data = Convert.FromBase64String(input.data);
            return new MLImageFeature(data);
        }

        private static MLArrayFeature<float> DeserializeAudio (MLHubFeature input) {
            throw new NotImplementedException(@"Deserializing audio features is not yet supported");
        }

        private static MLTextFeature DeserializeText (MLHubFeature input) {
            return new MLTextFeature(input.data);
        }
        #endregion
    }
}