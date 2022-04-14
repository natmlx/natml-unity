/* 
*   NatML
*   Copyright (c) 2022 NatML Inc. All rights reserved.
*/

namespace NatSuite.ML.Hub {

    using System;

    #region --Enumerations--
    /// <summary>
    /// Development framework.
    /// </summary>
    public static class Framework {
        /// <summary>
        /// Unity Engine.
        /// </summary>
        public const string Unity = "UNITY";
    }

    /// <summary>
    /// Cloud feature data type.
    /// </summary>
    public static class DataType {
        /// <summary>
        /// Single precision floating point number.
        /// </summary>
        public const string Float32 = @"FLOAT32";
        /// <summary>
        /// Double precision floating point number.
        /// </summary>
        public const string Float64 = @"FLOAT64";
        /// <summary>
        /// Signed 8-bit integer.
        /// </summary>
        public const string Int8 = @"INT8";
        /// <summary>
        /// Signed 16-bit integer.
        /// </summary>
        public const string Int16 = @"INT16";
        /// <summary>
        /// Signed 32-bit integer.
        /// </summary>
        public const string Int32 = @"INT32";
        /// <summary>
        /// Signed 64-bit integer.
        /// </summary>
        public const string Int64 = @"INT64";
        /// <summary>
        /// Unsigned 8-bit integer.
        /// </summary>
        public const string UInt8 = @"UINT8";
        /// <summary>
        /// Unsigned 16-bit integer.
        /// </summary>
        public const string UInt16 = @"UINT16";
        /// <summary>
        /// Unsigned 32-bit integer.
        /// </summary>
        public const string UInt32 = @"UINT32";
        /// <summary>
        /// Unsigned 64-bit integer.
        /// </summary>
        public const string UInt64 = @"UINT64";
        /// <summary>
        /// Plain text.
        /// </summary>
        public const string String = @"STRING";
        /// <summary>
        /// Encoded image.
        /// </summary>
        public const string Image = @"IMAGE";
        /// <summary>
        /// Encoded video.
        /// </summary>
        public const string Video = @"VIDEO";
        /// <summary>
        /// Encoded audio.
        /// </summary>
        public const string Audio =  @"AUDIO";
        /// <summary>
        /// Raw binary data.
        /// </summary>
        public const string Binary = @"BINARY";
    }

    /// <summary>
    /// Graph format.
    /// </summary>
    public static class GraphFormat {
        /// <summary>
        /// Apple CoreML.
        /// </summary>
        public const string CoreML = @"COREML";
        /// <summary>
        /// Open Neural Network Exchange.
        /// </summary>
        public const string ONNX = @"ONNX";
        /// <summary>
        /// TensorFlow Lite.
        /// </summary>
        public const string TensorFlowLite = @"TFLITE";
    }

    /// <summary>
    /// Device platform.
    /// </summary>
    public static class Platform {
        /// <summary>
        /// Android.
        /// </summary>
        public const string Android = @"ANDROID";
        /// <summary>
        /// iOS.
        /// </summary>
        public const string iOS = @"IOS";
        /// <summary>
        /// Linux.
        /// </summary>
        public const string Linux = @"LINUX";
        /// <summary>
        /// macOS.
        /// </summary>
        public const string macOS = @"MACOS";
        /// <summary>
        /// Browser or other Web platform.
        /// </summary>
        public const string Web = @"WEB";
        /// <summary>
        /// Windows.
        /// </summary>
        public const string Windows = @"WINDOWS";
    }

    /// <summary>
    /// Predictor type.
    /// </summary>
    public static class PredictorType {
        /// <summary>
        /// Edge predictor.
        /// Predictions are made on-device with model graphs delivered from Hub.
        /// </summary>
        public const string Edge = @"EDGE";
        /// <summary>
        /// Cloud predictor.
        /// Predictions are made server-side.
        /// </summary>
        public const string Cloud = @"CLOUD";
    }

    /// <summary>
    /// Predictor status.
    /// </summary>
    public static class PredictorStatus {
        /// <summary>
        /// Predictor is a draft.
        /// Predictor can only be viewed and used by author.
        /// </summary>
        public const string Draft = @"DRAFT";
        /// <summary>
        /// Predictor is pending review.
        /// Predictor can only be viewed and used by author.
        /// </summary>
        public const string Pending = @"PENDING";
        /// <summary>
        /// Predictor is in review.
        /// Predictor can be viewed and used by owner or NatML predictor review team.
        /// </summary>
        public const string Review = @"REVIEW";
        /// <summary>
        /// Predictor has been published.
        /// Predictor viewing and fetching permissions are dictated by the access mode.
        /// </summary>
        public const string Published = @"PUBLISHED";
        /// <summary>
        /// Predictor is archived.
        /// Predictor can be viewed but cannot be used by anyone including owner.
        /// </summary>
        public const string Archived = @"ARCHIVED";
    }

    /// <summary>
    /// Prediction status.
    /// </summary>
    public static class PredictionStatus {
        /// <summary>
        /// Waiting to execute.
        /// </summary>
        public const string Waiting = @"WAITING";
        /// <summary>
        /// Prediction is being executed.
        /// </summary>
        public const string Processing = @"PROCESSING";
        /// <summary>
        /// Prediction completed or errored.
        /// </summary>
        public const string Completed = @"COMPLETED";
    }

    /// <summary>
    /// Upload URL type.
    /// </summary>
    public static class UploadType {
        /// <summary>
        /// Feature data for Cloud prediction.
        /// </summary>
        public const string Feature = @"FEATURE";
    }
    #endregion


    #region --Structs--
    [Serializable]
    public sealed class Session {
        public string id;
        public Predictor predictor;
        public string platform;
        public string graph;
        public string format;
        public int flags;
    }

    [Serializable]
    public sealed class Predictor {
        public string tag;
        public string type;
        public string status;
        public string aspectMode;
        public string[] labels;
        public MLModelData.Normalization normalization;
        public MLModelData.AudioFormat audioFormat;
    }

    [Serializable]
    public sealed class Prediction {
        public string id;
        public string status;
        public Feature[] results;
        public string error;
    }

    [Serializable]
    public sealed class Feature {
        public string type;
        public string data;
        public int[] shape;
    }
    #endregion
}