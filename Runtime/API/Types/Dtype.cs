/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    using System;
    using System.Collections;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Feature data type.
    /// This follows `numpy` dtypes.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Dtype : int { // CHECK // Must match `Function.h`
        /// <summary>
        /// Unknown or unsupported data type.
        /// </summary>
        Undefined   = 0,
        /// <summary>
        /// Type is a `int8_t` in C/C++ and `sbyte` in C#.
        /// </summary>
        [EnumMember(Value = @"int8")]
        Int8 = 10,
        /// <summary>
        /// Type is `int16_t` in C/C++ and `short` in C#.
        /// </summary>
        [EnumMember(Value = @"int16")]
        Int16 = 2,
        /// <summary>
        /// Type is `int32_t` in C/C++ and `int` in C#.
        /// </summary>
        [EnumMember(Value = @"int32")]
        Int32 = 3,
        /// <summary>
        /// Type is `int64_t` in C/C++ and `long` in C#.
        /// </summary>
        [EnumMember(Value = @"int64")]
        Int64 = 4,
        /// <summary>
        /// Type is `uint8_t` in C/C++ and `byte` in C#.
        /// </summary>
        [EnumMember(Value = @"uint8")]
        Uint8 = 1,
        /// <summary>
        /// Type is a `uint16_t` in C/C++ and `ushort` in C#.
        /// </summary>
        [EnumMember(Value = @"uint16")]
        Uint16 = 11,
        /// <summary>
        /// Type is a `uint32_t` in C/C++ and `uint` in C#.
        /// </summary>
        [EnumMember(Value = @"uint32")]
        Uint32 = 12,
        /// <summary>
        /// Type is a `uint64_t` in C/C++ and `ulong` in C#.
        /// </summary>
        [EnumMember(Value = @"uint64")]
        Uint64 = 13,
        /// <summary>
        /// Type is a generic half-precision float.
        /// </summary>
        [EnumMember(Value = @"float16")]
        Float16 = 14,
        /// <summary>
        /// Type is `float` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"float32")]
        Float32 = 5,
        /// <summary>
        /// Type is `double` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"float64")]
        Float64 = 6,
        /// <summary>
        /// Type is a `bool` in C/C++/C#.
        /// </summary>
        [EnumMember(Value = @"bool")]
        Bool = 15,
        /// <summary>
        /// Type is `std::string` in C++ and `string` in C#.
        /// </summary>
        [EnumMember(Value = @"string")]
        String = 7,
        /// <summary>
        /// Type is an encoded image.
        /// </summary>
        [EnumMember(Value = @"image")]
        Image = 16,
        /// <summary>
        /// Type is an encoded audio.
        /// </summary>
        [EnumMember(Value = @"audio")]
        Audio = 18,
        /// <summary>
        /// Type is an encoded video.
        /// </summary>
        [EnumMember(Value = @"video")]
        Video = 19,
        /// <summary>
        /// Type is a binary blob.
        /// </summary>
        [EnumMember(Value = @"binary")]
        Binary = 17,
        /// <summary>
        /// Type is a generic list.
        /// </summary>
        [EnumMember(Value = @"list")]
        List = 8,
        /// <summary>
        /// Type is a generic dictionary.
        /// </summary>
        [EnumMember(Value = @"dict")]
        Dict = 9,
    }
}