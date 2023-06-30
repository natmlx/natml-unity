/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    using System.Runtime.Serialization;

    /// <summary>
    /// Upload URL type.
    /// </summary>
    public enum UploadType : int {
        [EnumMember(Value = @"DEMO")]
        Demo        = 0,
        [EnumMember(Value = @"FEATURE")]
        Feature     = 1,
        [EnumMember(Value = @"GRAPH")]
        Graph       = 2,
        [EnumMember(Value = @"MEDIA")]
        Media       = 3,
        [EnumMember(Value = @"NOTEBOOK")]
        Notebook    = 4,
    }
}