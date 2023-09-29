/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.API.Types {

    using System;
    using System.Collections;

    public static class TypeExtensions {

        /// <summary>
        /// Convert a NatML data type to a managed type.
        /// </summary>
        /// <param name="dtype">NatML data type.</param>
        /// <returns>Managed data type.</returns>
        public static Type ToType (this Dtype dtype) => dtype switch {
            Dtype.Float16       => null, // Any support for this in C#?
            Dtype.Float32       => typeof(float),
            Dtype.Float64       => typeof(double),
            Dtype.Int8          => typeof(sbyte),
            Dtype.Int16         => typeof(short),
            Dtype.Int32         => typeof(int),
            Dtype.Int64         => typeof(long),
            Dtype.Uint8         => typeof(byte),
            Dtype.Uint16        => typeof(ushort),
            Dtype.Uint32        => typeof(uint),
            Dtype.Uint64        => typeof(ulong),
            Dtype.String        => typeof(string),
            Dtype.List          => typeof(IList),
            Dtype.Dict          => typeof(IDictionary),
            _                   => null,
        };

        /// <summary>
        /// Convert a managed type to a NatML data type.
        /// </summary>
        /// <param name="type">Managed type.</param>
        /// <returns>NatML data type.</returns>
        public static Dtype ToDtype (this Type dtype) => dtype switch {
            var t when t == typeof(float)   => Dtype.Float32,
            var t when t == typeof(double)  => Dtype.Float64,
            var t when t == typeof(sbyte)   => Dtype.Int8,
            var t when t == typeof(short)   => Dtype.Int16,
            var t when t == typeof(int)     => Dtype.Int32,
            var t when t == typeof(long)    => Dtype.Int64,
            var t when t == typeof(byte)    => Dtype.Uint8,
            var t when t == typeof(ushort)  => Dtype.Uint16,
            var t when t == typeof(uint)    => Dtype.Uint32,
            var t when t == typeof(ulong)   => Dtype.Uint64,
            _                               => Dtype.Undefined,
        };
    }
}