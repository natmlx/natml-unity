/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections;

    public static class NMLUtility {

        /// <summary>
        /// Get the corresponding NML data type for a .NET type.
        /// </summary>
        /// <returns>NML data type.</returns>
        public static NMLDataType NativeType (this Type type) {
            switch (type) {
                case var t when t == typeof(byte): return NMLDataType.UInt8;
                case var t when t == typeof(short): return NMLDataType.Int16;
                case var t when t == typeof(int): return NMLDataType.Int32;
                case var t when t == typeof(long): return NMLDataType.Int64;
                case var t when t == typeof(float): return NMLDataType.Float;
                case var t when t == typeof(double): return NMLDataType.Double;
                case var t when t == typeof(string): return NMLDataType.String;
                case var t when t.IsAssignableFrom(typeof(IList)): return NMLDataType.Sequence;
                case var t when t.IsAssignableFrom(typeof(IDictionary)): return NMLDataType.Dictionary;
                default: return NMLDataType.Undefined;
            }
        }

        /// <summary>
        /// Get the corresponding .NET type for an NML data type.
        /// </summary>
        /// <returns>Managed type.</returns>
        public static Type ManagedType (this NMLDataType type) {
            switch (type) {
                case NMLDataType.UInt8: return typeof(byte);
                case NMLDataType.Int16: return typeof(short);
                case NMLDataType.Int32: return typeof(int);
                case NMLDataType.Int64: return typeof(long);
                case NMLDataType.Float: return typeof(float);
                case NMLDataType.Double: return typeof(double);
                case NMLDataType.String: return typeof(string);
                case NMLDataType.Sequence: return typeof(IList);
                case NMLDataType.Dictionary: return typeof(IDictionary);
                default: return null;
            }
        }
    }
}