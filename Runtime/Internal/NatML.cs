/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

namespace NatML.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Dtype = API.Types.Dtype;

    public static class NatML {

        public const string Version = @"1.1.16";
        public const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatML";
        #endif

        #region --Delegates--
        public delegate void ModelCreationHandler (IntPtr context, IntPtr model);
        public delegate void SecretCreationHandler (IntPtr context, IntPtr secret);
        #endregion


        #region --NMLModelConfiguration--
        [DllImport(Assembly, EntryPoint = @"NMLCreateModelConfiguration")]
        public static extern void CreateModelConfiguration (out IntPtr configuration);

        [DllImport(Assembly, EntryPoint = @"NMLReleaseModelConfiguration")]
        public static extern void ReleaseModelConfiguration (this IntPtr configuration);

        [DllImport(Assembly, EntryPoint = @"NMLModelConfigurationSetComputeTarget")]
        public static extern void SetComputeTarget (
            this IntPtr configuration,
            MLEdgeModel.ComputeTarget target
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelConfigurationSetComputeDevice")]
        public static extern void SetComputeDevice (
            this IntPtr configuration,
            IntPtr device
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelConfigurationSetFingerprint")]
        public static extern void SetFingerprint (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string fingerprint
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelConfigurationSetSecret")]
        public static extern void SetSecret (
            this IntPtr configuration,
            [MarshalAs(UnmanagedType.LPStr)] string secret
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelConfigurationCreateSecret")]
        public static extern void CreateSecret (
            SecretCreationHandler handler,
            IntPtr context
        );
        #endregion


        #region --NMLModel--
        [DllImport(Assembly, EntryPoint = @"NMLCreateModel")]
        public static extern void CreateModel (
            byte[] buffer,
            long bufferSize,
            IntPtr options,
            ModelCreationHandler handler,
            IntPtr context
        );

        [DllImport(Assembly, EntryPoint = @"NMLReleaseModel")]
        public static extern void ReleaseModel (this IntPtr model);

        [DllImport(Assembly, EntryPoint = @"NMLModelGetMetadataCount")]
        public static extern int MetadataCount (this IntPtr model);

        [DllImport(Assembly, EntryPoint = @"NMLModelGetMetadataKey")]
        public static extern void MetadataKey (
            this IntPtr model,
            int index,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest,
            int size
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelGetMetadataValue")]
        public static extern void MetadataValue (
            this IntPtr model,
            [MarshalAs(UnmanagedType.LPStr)] string key,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest,
            int size
        );
        [DllImport(Assembly, EntryPoint = @"NMLModelGetInputFeatureCount")]
        public static extern int InputFeatureCount (this IntPtr model);

        [DllImport(Assembly, EntryPoint = @"NMLModelGetInputFeatureType")]
        public static extern void InputFeatureType (
            this IntPtr model, 
            int index, 
            out IntPtr type
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelGetOutputFeatureCount")]
        public static extern int OutputFeatureCount (this IntPtr model);

        [DllImport(Assembly, EntryPoint = @"NMLModelGetOutputFeatureType")]
        public static extern void OutputFeatureType (
            this IntPtr model,
            int index,
            out IntPtr type
        );

        [DllImport(Assembly, EntryPoint = @"NMLModelPredict")]
        public static extern void Predict (
            this IntPtr model,
            [In] IntPtr[] inputs,
            [Out] IntPtr[] outputs
        );
        #endregion


        #region --NMLFeature--
        [DllImport(Assembly, EntryPoint = @"NMLReleaseFeature")]
        public static extern void ReleaseFeature (this IntPtr feature);

        [DllImport(Assembly, EntryPoint = @"NMLFeatureGetType")]
        public static extern void FeatureType (
            this IntPtr feature,
            out IntPtr type
        );

        [DllImport(Assembly, EntryPoint = @"NMLFeatureGetData")]
        public static extern IntPtr FeatureData (this IntPtr feature);

        [DllImport(Assembly, EntryPoint = @"NMLCreateArrayFeature")]
        public static unsafe extern void CreateFeature (
            void* data,
            [In] int[] shape,
            int dims,
            Dtype type,
            int flags,
            out IntPtr feature
        );

        [DllImport(Assembly, EntryPoint = @"NMLCreateImageFeature")]
        public static unsafe extern void CreateFeature (
            void* pixelBuffer,
            int width,
            int height,
            [In] int[] shape,
            Dtype type,
            float* mean,
            float* std,
            int flags,
            out IntPtr feature
        );

        [DllImport(Assembly, EntryPoint = @"NMLCreateAudioFeature")]
        public static unsafe extern void CreateFeature (
            float* sampleBuffer,
            int bufferSampleRate,
            [In] int[] bufferShape,
            int sampleRate,
            int channelCount,
            Dtype type,
            [In] float[] mean,
            [In] float[] std,
            int flags,
            out IntPtr feature
        );

        [DllImport(Assembly, EntryPoint = @"NMLImageFeatureCopyTo")]
        public static unsafe extern void CopyTo (
            void* srcBuffer,
            int width,
            int height,
            int* rect,
            float rotation,
            byte* background,
            void* dstBuffer
        );
        #endregion


        #region --NMLFeatureType--
        [DllImport(Assembly, EntryPoint = @"NMLReleaseFeatureType")]
        public static extern void ReleaseFeatureType (this IntPtr type);

        [DllImport(Assembly, EntryPoint = @"NMLFeatureTypeGetName")]
        public static extern void FeatureTypeName (
            this IntPtr type,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder dest,
            int size
        );

        [DllImport(Assembly, EntryPoint = @"NMLFeatureTypeGetDataType")]
        public static extern Dtype FeatureTypeDataType (this IntPtr type);

        [DllImport(Assembly, EntryPoint = @"NMLFeatureTypeGetDimensions")]
        public static extern int FeatureTypeDimensions (this IntPtr type);

        [DllImport(Assembly, EntryPoint = @"NMLFeatureTypeGetShape")]
        public static extern void FeatureTypeShape (
            this IntPtr type,
            [Out] int[] shape,
            int length
        );
        #endregion


        #region --NMLFeatureReader--
        [DllImport(Assembly, EntryPoint = @"NMLFeatureReaderGetVideoFormat")]
        public static extern void GetVideoFormat (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            out int width,
            out int height,
            out int frames
        );

        [DllImport(Assembly, EntryPoint = @"NMLFeatureReaderGetAudioFormat")]
        public static extern void GetAudioFormat (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            out int sampleRate,
            out int channelCount,
            out int sampleCount
        );

        [DllImport(Assembly, EntryPoint = @"NMLReleaseFeatureReader")]
        public static extern void ReleaseFeatureReader (this IntPtr reader);

        [DllImport(Assembly, EntryPoint = @"NMLFeatureReaderReadNextFeature")]
        public static extern void ReadNextFeature (
            this IntPtr reader,
            out long timestamp,
            out IntPtr feature
        );

        [DllImport(Assembly, EntryPoint = @"NMLCreateImageFeatureReader")]
        public static extern void CreateImageFeatureReader (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            out IntPtr reader
        );

        [DllImport(Assembly, EntryPoint = @"NMLCreateAudioFeatureReader")]
        public static extern void CreateAudioFeatureReader (
            [MarshalAs(UnmanagedType.LPStr)] string path,
            out IntPtr reader
        );
        #endregion
    }
}