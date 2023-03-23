/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    /// <summary>
    /// Feature data type.
    /// This follows `numpy` dtypes.
    /// </summary>
    public enum Dtype : int { // CHECK // Must match `NatML.h`
        /// <summary>
        /// Unknown or unsupported data type.
        /// </summary>
        Undefined   = 0,
        /// <summary>
        /// Type is a `int8_t` in C/C++ and `sbyte` in C#.
        /// </summary>
        Int8        = 10,
        /// <summary>
        /// Type is `int16_t` in C/C++ and `short` in C#.
        /// </summary>
        Int16       = 2,
        /// <summary>
        /// Type is `int32_t` in C/C++ and `int` in C#.
        /// </summary>
        Int32       = 3,
        /// <summary>
        /// Type is `int64_t` in C/C++ and `long` in C#.
        /// </summary>
        Int64       = 4,
        /// <summary>
        /// Type is `uint8_t` in C/C++ and `byte` in C#.
        /// </summary>
        Uint8       = 1,
        /// <summary>
        /// Type is a `uint16_t` in C/C++ and `ushort` in C#.
        /// </summary>
        Uint16      = 11,
        /// <summary>
        /// Type is a `uint32_t` in C/C++ and `uint` in C#.
        /// </summary>
        Uint32      = 12,
        /// <summary>
        /// Type is a `uint64_t` in C/C++ and `ulong` in C#.
        /// </summary>
        Uint64      = 13,
        /// <summary>
        /// Type is a generic half-precision float.
        /// </summary>
        Float16     = 14,
        /// <summary>
        /// Type is `float` in C/C++/C#.
        /// </summary>
        Float32     = 5,
        /// <summary>
        /// Type is `double` in C/C++/C#.
        /// </summary>
        Float64     = 6,
        /// <summary>
        /// Type is a `bool` in C/C++/C#.
        /// </summary>
        Bool        = 15,
        /// <summary>
        /// Type is `std::string` in C++ and `string` in C#.
        /// </summary>
        String      = 7,
        /// <summary>
        /// Type is an encoded image.
        /// </summary>
        Image       = 16,
        /// <summary>
        /// Type is a binary blob.
        /// </summary>
        Binary      = 17,
        /// <summary>
        /// Type is a generic sequence.
        /// </summary>
        Sequence    = 8,
        /// <summary>
        /// Type is a generic dictionary.
        /// </summary>
        Dictionary  = 9,
    }
}