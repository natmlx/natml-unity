/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Hub {

    using System;
    using Internal;

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
    /// Hub feature data type.
    /// </summary>
    public static class HubDataType {
        /// <summary>
        /// Single precision floating point number.
        /// </summary>
        public const string Float = @"FLOAT32";
        /// <summary>
        /// Double precision floating point number.
        /// </summary>
        public const string Double = @"FLOAT64";
        /// <summary>
        /// Signed 8-bit integer.
        /// </summary>
        public const string SByte = @"INT8";
        /// <summary>
        /// Signed 16-bit integer.
        /// </summary>
        public const string Short = @"INT16";
        /// <summary>
        /// Signed 32-bit integer.
        /// </summary>
        public const string Int = @"INT32";
        /// <summary>
        /// Signed 64-bit integer.
        /// </summary>
        public const string Long = @"INT64";
        /// <summary>
        /// Unsigned 8-bit integer.
        /// </summary>
        public const string Byte = @"UINT8";
        /// <summary>
        /// Unsigned 16-bit integer.
        /// </summary>
        public const string UShort = @"UINT16";
        /// <summary>
        /// Unsigned 32-bit integer.
        /// </summary>
        public const string UInt = @"UINT32";
        /// <summary>
        /// Unsigned 64-bit integer.
        /// </summary>
        public const string ULong = @"UINT64";
        /// <summary>
        /// Encoded image.
        /// </summary>
        public const string Image = @"IMAGE";
        /// <summary>
        /// Encoded audio
        /// </summary>
        public const string Audio =  @"AUDIO";
        /// <summary>
        /// Plain text.
        /// </summary>
        public const string String = @"STRING";
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
        /// Hub Predictor.
        /// Predictions are made server-side.
        /// </summary>
        public const string Hub = @"HUB";
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
        /// Feature data for Hub prediction.
        /// </summary>
        public const string Feature = @"FEATURE";
    }

    internal static class SubscriptionMessage {
        public const string Initialize = @"connection_init";
        public const string Acknowledge = @"connection_ack";
        public const string Start = @"start";
        public const string KeepAlive = @"ka";
        public const string Data = @"data";
        public const string Error = @"error";
        public const string Complete = @"complete";
    }
    #endregion


    #region --Structs--
    [Serializable]
    public sealed class Session {
        public string id;
        public Predictor predictor;
        public string platform;
        public string graph;
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
        public MLHubFeature[] results;
        public string error;
    }
    #endregion
}