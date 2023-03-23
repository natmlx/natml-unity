/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// ML predictor.
    /// </summary>
    [Serializable]
    public sealed class Predictor {

        /// <summary>
        /// Predictor tag.
        /// </summary>
        public string tag;

        /// <summary>
        /// Predictor owner.
        /// </summary>
        public Profile owner;

        /// <summary>
        /// Predictor name.
        /// </summary>
        public string name;

        /// <summary>
        /// Predictor description.
        /// This supports Markdown.
        /// </summary>
        public string description;

        /// <summary>
        /// Predictor status.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<PredictorStatus>))]
        public PredictorStatus status;

        /// <summary>
        /// Predictor access.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<AccessMode>))]
        public AccessMode access;

        /// <summary>
        /// Predictor graphs.
        /// </summary>
        [JsonProperty]
        internal Graph[] graphs;

        /// <summary>
        /// Predictor endpoints.
        /// </summary>
        [JsonProperty]
        internal Endpoint[] endpoints;

        /// <summary>
        /// Predictor license.
        /// </summary>
        public string license;

        /// <summary>
        /// Predictor topics.
        /// </summary>
        public string[] topics;

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// Predictor media URL.
        /// </summary>
        public string? media;

        /// <summary>
        /// Classification labels.
        /// </summary>
        public string[]? labels;

        /// <summary>
        /// Feature normalization.
        /// </summary>
        public Normalization? normalization;

        /// <summary>
        /// Image aspect mode.
        /// </summary>
        [JsonConverter(typeof(API.Graph.GraphEnum<AspectMode>), true)]
        public AspectMode? aspectMode;

        /// <summary>
        /// Audio format.
        /// </summary>
        public AudioFormat? audioFormat;
    }

    /// <summary>
    /// Predictor status.
    /// </summary>
    public enum PredictorStatus : int {
        /// <summary>
        /// Predictor is a draft.
        /// Predictor can only be viewed and used by owner.
        /// </summary>
        Draft       = 0,
        /// <summary>
        /// Predictor has been published.
        /// Predictor viewing and fetching permissions are dictated by the access mode.
        /// </summary>
        Published   = 1,
        /// <summary>
        /// Predictor is archived.
        /// Predictor can be viewed but cannot be used by anyone including owner.
        /// </summary>
        Archived    = 2,
    }

    /// <summary>
    /// Access mode.
    /// </summary>
    public enum AccessMode : int {
        /// <summary>
        /// Resource can only be accessed by the owner.
        /// </summary>
        Private = 0,
        /// <summary>
        /// Resource can be accessed by anyone with NatML authentication.
        /// </summary>
        Public  = 1
    }

    /// <summary>
    /// Image aspect mode.
    /// </summary>
    public enum AspectMode : int { // CHECK // Must match `NatML.h`
        /// <summary>
        /// Scale to fit.
        /// This scale mode DOES NOT preserve the aspect ratio of the image.
        /// </summary>
        ScaleToFit  = 0,
        /// <summary>
        /// Aspect fill.
        /// </summary>
        AspectFill  = 1,
        /// <summary>
        /// Aspect fit.
        /// </summary>
        AspectFit   = 2,
    }
}