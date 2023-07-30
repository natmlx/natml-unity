/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Services {

    using System.Threading.Tasks;
    using Graph;
    using Types;

    /// <summary>
    /// Manage users.
    /// </summary>
    public sealed class UserService {

        #region --Client API--
        /// <summary>
        /// Retrieve a user.
        /// </summary>
        /// <param name="username">Username. If `null` then this will retrieve the currently authenticated user.</param>
        public async Task<User?> Retrieve (string? username = null) {
            var profile = !string.IsNullOrEmpty(username);
            var user = await client.Query<User>(
                @$"query {(profile ? "($input: UserInput)" : string.Empty)} {{
                    user {(profile ? "(input: $input)" : "")} {{
                        username
                        {(profile ? string.Empty :  "email")}
                        created
                        name
                        avatar
                        bio
                        website
                        github
                    }}
                }}",
                @"user",
                new () {
                    ["input"] = new UserInput {
                        username = username
                    }
                }
            );
            return user;
        }
        #endregion


        #region --Operations--
        private readonly IGraphClient client;

        internal UserService (IGraphClient client) => this.client = client;
        #endregion
    }

    #region --Types--

    internal sealed class UserInput {
        public string username;
    }
    #endregion
}