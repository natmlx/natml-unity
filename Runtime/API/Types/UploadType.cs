/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    /// <summary>
    /// Upload URL type.
    /// </summary>
    public enum UploadType : int {
        Demo        = 0,
        Feature     = 1,
        Graph       = 2,
        Media       = 3,
        Notebook    = 4,
    }
}