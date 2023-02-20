/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Hub;
    using Hub.Types;
    using Types;
    using static NatML;

    /// <summary>
    /// Utilities for working with predictor graphs.
    /// </summary>
    public static class MLGraphUtils {

        #region --Client API--
        /// <summary>
        /// Convert a native `NMLFeatureType` to a managed `MLFeatureType`.
        /// </summary>
        /// <param name="nativeType">Native `NMLFeatureType`.</param>
        /// <returns>Managed feature type.</returns>
        public static MLFeatureType ToFeatureType (this in IntPtr nativeType) {
            // Get dtype
            var dtype = nativeType.FeatureTypeDataType();
            if (dtype == DataType.Undefined)
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
                case DataType.Sequence:             return null;
                case DataType.Dictionary:           return null;
                case var _ when shape.Length == 4:  return new MLImageType(shape, dtype.ToManagedType(), name);
                default:                            return new MLArrayType(shape, dtype.ToManagedType(), name);
            }
        }

        /// <summary>
        /// Convert an edge data type to a managed type.
        /// </summary>
        /// <param name="type">Native `NMLDataType`.</param>
        /// <returns>Managed data type.</returns>
        public static Type ToManagedType (this DataType dtype) => dtype switch {
            DataType.Float16    => null, // Any support for this in C#?
            DataType.Float32    => typeof(float),
            DataType.Float64    => typeof(double),
            DataType.Int8       => typeof(sbyte),
            DataType.Int16      => typeof(short),
            DataType.Int32      => typeof(int),
            DataType.Int64      => typeof(long),
            DataType.UInt8      => typeof(byte),
            DataType.UInt16     => typeof(ushort),
            DataType.UInt32     => typeof(uint),
            DataType.UInt64     => typeof(ulong),
            DataType.String     => typeof(string),
            DataType.Sequence   => typeof(IList),
            DataType.Dictionary => typeof(IDictionary),
            _                   => null,
        };

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DataType ToEdgeType (this Type dtype) => dtype switch {
            var t when t == typeof(float)   =>  DataType.Float32,
            var t when t == typeof(double)  => DataType.Float64,
            var t when t == typeof(sbyte)   => DataType.Int8,
            var t when t == typeof(short)   => DataType.Int16,
            var t when t == typeof(int)     => DataType.Int32,
            var t when t == typeof(long)    => DataType.Int64,
            var t when t == typeof(byte)    => DataType.UInt8,
            var t when t == typeof(ushort)  => DataType.UInt16,
            var t when t == typeof(uint)    => DataType.UInt32,
            var t when t == typeof(ulong)   => DataType.UInt64,
            _                               => DataType.Undefined,
        };

        /// <summary>
        /// Create a predictor session secret.
        /// </summary>
        /// <returns>Predictor session secret.</returns>
        public static Task<string> CreateSecret () {
            var tcs = new TaskCompletionSource<string>();
            var context = (IntPtr)GCHandle.Alloc(tcs, GCHandleType.Normal);
            NatML.CreateSecret(OnCreateSecret, context);
            return tcs.Task;
        }

        /// <summary>
        /// Get the graph format for an ML graph file based on its file extension.
        /// </summary>
        /// <param name="path">Path to ML graph.</param>
        /// <returns>Graph format or `null` if unrecognized format.</returns>
        public static string FormatForFile (string path) => Path.GetExtension(path) switch {
            ".mlmodel"  => GraphFormat.CoreML,
            ".onnx"     => GraphFormat.ONNX,
            ".tflite"   => GraphFormat.TensorFlowLite,
            _           => null,
        };
        #endregion


        #region --Operations--

        [AOT.MonoPInvokeCallback(typeof(NatML.SecretCreationHandler))]
        private static void OnCreateSecret (IntPtr context, IntPtr secret) {
            // Get tcs
            var handle = (GCHandle)context;
            var tcs = handle.Target as TaskCompletionSource<string>;
            handle.Free();
            // Create secret
            if (secret != IntPtr.Zero)
                tcs.SetResult(Marshal.PtrToStringUTF8(secret));
            else
                tcs.SetException(new ArgumentException(@"Failed to create predictor session secret"));
        }
        #endregion
    }
}