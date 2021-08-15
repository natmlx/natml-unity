/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Types;

    public static class NMLUtility {

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

        public static MLFeatureType MarshalFeatureType (this in IntPtr nativeType) { // CHECK // Nested types
            // Get dtype
            var dtype = nativeType.FeatureTypeDataType();
            if (dtype == NMLDataType.Undefined)
                return null;
            // Get name
            var nameBuffer = new StringBuilder(2048);
            nativeType.FeatureTypeName(nameBuffer, nameBuffer.Capacity);
            var name = nameBuffer.Length > 0 ? nameBuffer.ToString() : null;
            // Get shape
            var shape = new int[nativeType.FeatureTypeDimensions()];
            nativeType.FeatureTypeShape(shape, shape.Length);
            // Return
            switch (dtype) {
                case NMLDataType.Sequence: return default;
                case NMLDataType.Dictionary: return default;
                case var _ when shape.Length == 4: return new MLImageType(name, dtype.ManagedType(), shape);
                default: return new MLArrayType(name, dtype.ManagedType(), shape);
            }
        }

        /// <summary>
        /// Get the feature shape.
        /// Feature MUST be an array feature.
        /// </summary>
        /// <param name="feature">Native feature.</param>
        /// <returns>Feature shape.</returns>
        public static int[] FeatureShape (this in IntPtr feature) {
            feature.FeatureType(out var type);
            var shape = new int[type.FeatureTypeDimensions()];
            type.FeatureTypeShape(shape, shape.Length);
            type.ReleaseFeatureType();
            return shape;
        }

        /// <summary>
        /// Get the element count in a shape.
        /// </summary>
        /// <param name="shape"></param>
        /// <returns>Shape element count.</returns>
        public static int ElementCount (this IEnumerable<int> shape) => shape.Aggregate(1, (a, b) => a * b);
    }
}