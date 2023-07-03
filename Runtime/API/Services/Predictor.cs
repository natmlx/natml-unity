/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

#nullable enable

namespace NatML.API.Services {

    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Graph;
    using Types;

    /// <summary>
    /// Manage predictors.
    /// </summary>
    public sealed class PredictorService {
        
        #region --Client API--
        /// <summary>
        /// Retrieve a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag. This MUST NOT be a variant tag.</param>
        public Task<Predictor?> Retrieve (string tag) => client.Query<Predictor?>(
            @$"query ($input: PredictorInput!) {{
                predictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"predictor",
            new () {
                ["input"] = new PredictorInput {
                    tag = tag
                }
            }
        );

        /// <summary>
        /// View available predictors.
        /// </summary>
        /// <param name="mine">Fetch only predictors owned by me.</param>
        /// <param name="status">Predictor status. This only applies when `mine` is `true`.</param>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="count">Pagination count.</param>
        public async Task<Predictor[]> List (
            bool? mine = null,
            PredictorStatus? status = null,
            int? offset = null,
            int? count = null
        ) => await client.Query<Predictor[]>(
            @$"query ($input: PredictorsInput!) {{
                predictors (input: $input) {{
                    {Fields}
                }}
            }}",
            @"predictors",
            new () {
                ["input"] = new PredictorsInput {
                    mine = mine,
                    status = status,
                    offset = offset,
                    count = count
                }
            }
        );

        /// <summary>
        /// Search predictors.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="count">Pagination count.</param>
        public Task<Predictor[]> Search (
            string query,
            int? offset = null,
            int? count = null
        ) => client.Query<Predictor[]>(
            @$"query ($input: PredictorsInput!) {{
                predictors (input: $input) {{
                    {Fields}
                }}
            }}",
            @"predictors",
            new () {
                ["input"] = new PredictorsInput {
                    query = query,
                    offset = offset,
                    count = count
                }
            }
        );

        /// <summary>
        /// Create a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="description">Predictor description. This supports Markdown.</param>
        /// <param name="access">Predictor access mode.</param>
        public Task<Predictor> Create (
            string tag,
            string? description = null, 
            AccessMode? access = null
        ) => client.Query<Predictor>(
            @$"mutation ($input: CreatePredictorInput!) {{
                createPredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"createPredictor",
            new () {
                ["input"] = new CreatePredictorInput {
                    tag = tag,
                    description = description,
                    access = access
                }
            }
        );

        /// <summary>
        /// Update a predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        /// <param name="description">Predictor description. This supports Markdown.</param>
        /// <param name="access">Predictor access mode.</param>
        /// <param name="license">Predictor license.</param>
        /// <param name="topics">Predictor topics.</param>
        /// <param name="media">Predictor media URL or path.</param>
        /// <param name="labels">Classification labels.</param>
        /// <param name="normalization">Feature normalization.</param>
        /// <param name="aspectMode">Image aspect mode.</param>
        /// <param name="audioFormat">Audio format.</param>
        public Task<Predictor> Update (
            string tag,
            string? description = null,
            AccessMode? access = null,
            string? license = null,
            string[]? topics = null,
            string? media = null,
            string[]? labels = null,
            Normalization? normalization = null,
            AspectMode? aspectMode = null,
            AudioFormat? audioFormat = null
        ) => client.Query<Predictor>(
            @$"mutation ($input: UpdatePredictorInput!) {{
                updatePredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"updatePredictor",
            new () {
                ["input"] = new UpdatePredictorInput {
                    tag = tag,
                    description = description,
                    access = access,
                    license = license,
                    topics = topics,
                    media = media,
                    labels = labels,
                    normalization = normalization,
                    aspectMode = aspectMode,
                    audioFormat = audioFormat
                }
            }
        );

        /// <summary>
        /// Delete a draft predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public Task<bool> Delete (string tag) => client.Query<bool>(
            @$"mutation ($input: DeletePredictorInput!) {{
                deletePredictor (input: $input)
            }}",
            @"deletePredictor",
            new () {
                ["input"] = new DeletePredictorInput {
                    tag = tag
                }
            }
        );

        /// <summary>
        /// Publish a draft predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public Task<Predictor> Publish (string tag) => client.Query<Predictor>(
            @$"mutation ($input: PublishPredictorInput!) {{
                publishPredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"publishPredictor",
            new () {
                ["input"] = new PublishPredictorInput {
                    tag = tag
                }
            }
        );

        /// <summary>
        /// Archive a published predictor.
        /// </summary>
        /// <param name="tag">Predictor tag.</param>
        public Task<Predictor> Archive (string tag) => client.Query<Predictor>(
            @$"mutation ($input: ArchivePredictorInput!) {{
                archivePredictor (input: $input) {{
                    {Fields}
                }}
            }}",
            @"archivePredictor",
            new () {
                ["input"] = new ArchivePredictorInput {
                    tag = tag
                }
            }
        );
        #endregion


        #region --Operations--
        private readonly IGraphClient client;
        private const string Fields = @"
        tag
        owner {
            username
            created
            name
            avatar
            bio
            website
            github
        }
        name
        description
        status
        access
        license
        topics
        created
        media
        ";

        internal PredictorService (IGraphClient client) => this.client = client;
        #endregion
    }

    #region --Types--

    internal class PredictorInput {
        public string tag;
    }

    internal sealed class PredictorsInput {
        public bool? mine;
        public PredictorStatus? status;
        public string? query;
        public int? offset;
        public int? count;
    }

    internal sealed class CreatePredictorInput {
        public string tag;
        public string? description;
        public AccessMode? access;
    }

    internal sealed class UpdatePredictorInput {
        public string tag;
        public string? description;
        public AccessMode? access;
        public string? license;
        public string[]? topics;
        public string? media;
        public string[]? labels;
        public Normalization? normalization;
        public AspectMode? aspectMode;
        public AudioFormat? audioFormat;
    }

    internal sealed class DeletePredictorInput : PredictorInput { }

    internal sealed class PublishPredictorInput : PredictorInput { }

    internal sealed class ArchivePredictorInput : PredictorInput { }
    #endregion
}