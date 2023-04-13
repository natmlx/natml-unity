/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable
#pragma warning disable 8618

namespace NatML.API.Types {

    using System;

    /// <summary>
    /// NatML user profile.
    /// </summary>
    [Serializable]
    public class Profile {

        /// <summary>
        /// Username.
        /// </summary>
        public string username;

        /// <summary>
        /// User email address.
        /// </summary>
        public string? email; // this is private to user hence nullable

        /// <summary>
        /// Date created.
        /// </summary>
        public string created;

        /// <summary>
        /// User display name.
        /// </summary>
        public string? name;

        /// <summary>
        /// User avatar.
        /// </summary>
        public string? avatar;

        /// <summary>
        /// User bio.
        /// </summary>
        public string? bio;

        /// <summary>
        /// User website.
        /// </summary>
        public string? website;

        /// <summary>
        /// User GitHub handle.
        /// </summary>
        public string? github;
    }
}