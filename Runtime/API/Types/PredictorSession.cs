/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System;
    using Newtonsoft.Json;
    using API.Graph;

    /// <summary>
    /// Predictor graph session.
    /// </summary>
    [Preserve, Serializable, Obsolete(@"Deprecated in NatML 1.1.6")]
    public sealed class PredictorSession {

        /// <summary>
        /// Session ID.
        /// </summary>
        public string id;

        /// <summary>
        /// Predictor for which this session was created.
        /// </summary>
        public Predictor predictor;

        /// <summary>
        /// Session graph URL.
        /// This URL is only valid for 10 minutes.
        /// </summary>
        public string graph;

        /// <summary>
        /// Session graph format.
        /// </summary>
        public GraphFormat format;

        /// <summary>
        /// Session flags.
        /// </summary>
        public int flags;

        /// <summary>
        /// Session graph fingerprint.
        /// This token uniquely identifies a graph across sessions.
        /// </summary>
        public string fingerprint;

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// Session secret.
        /// </summary>
        public string? secret;
    }
}