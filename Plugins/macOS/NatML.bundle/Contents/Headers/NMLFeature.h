//
//  NMLFeature.h
//  NatML
//
//  Created by Yusuf Olokoba on 3/14/2021.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include "NMLFeatureType.h"

#pragma region --Enumerations--
/*!
 @enum NMLFeatureFlags

 @abstract Feature flags.

 @constant NML_FEATURE_FLAG_NONE
 No flags.

 @constant NML_ARRAY_FLAG_COPY_DATA
 Copy input tensor data when creating feature.
 When this flag is not set, the feature data MUST remain valid for the lifetime of the created feature.

 @constant NML_IMAGE_FLAG_ASPECT_SCALE
 Image feature is scaled to fit feature size.

 @constant NML_IMAGE_FLAG_ASPECT_FILL
 Image feature is aspect-filled to the feature size.

 @constant NML_IMAGE_FLAG_ASPECT_FIT
 Image feature is aspect-fit to the feature size.
*/
enum NMLFeatureFlags {
    NML_FEATURE_FLAG_NONE               = 0,
    // MLArrayFeature
    NML_ARRAY_FLAG_COPY_DATA            = 1,
    // MLImageFeature::AspectMode
    NML_IMAGE_FLAG_ASPECT_SCALE         = 0,
    NML_IMAGE_FLAG_ASPECT_FILL          = 1,
    NML_IMAGE_FLAG_ASPECT_FIT           = 2,
};
typedef enum NMLFeatureFlags NMLFeatureFlags;
#pragma endregion


#pragma region --Types--
/*!
 @struct NMLFeature
 
 @abstract ML model input or output feature.

 @discussion ML model input or output feature.
 This is loosely based on `DLPack::DLTensor`.
*/
struct NMLFeature;
typedef struct NMLFeature NMLFeature;
#pragma endregion


#pragma region --Operations--
/*!
 @function NMLReleaseFeature

 @abstract Release an ML feature.

 @discussion Release an ML feature.

 @param feature
 ML feature.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLReleaseFeature (NMLFeature* feature);

/*!
 @function NMLFeatureGetType

 @abstract Get the feature type.

 @discussion Get the feature type.

 @param feature
 ML feature.

 @param type
 Output feature type. This type should be released once it is no longer in use.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLFeatureGetType (
    NMLFeature* feature,
    NMLFeatureType** type
);

/*!
 @function NMLFeatureGetData

 @abstract Get the feature data.

 @discussion Get the feature data.

 @param feature
 ML feature.

 @returns Opaque pointer to feature data.
*/
NML_BRIDGE NML_EXPORT void* NML_API NMLFeatureGetData (NMLFeature* feature);
#pragma endregion


#pragma region --Constructors--
/*!
 @function NMLCreateArrayFeature

 @abstract Create an array feature from a data buffer.

 @discussion Create an array feature from a data buffer.
 The data will not be released when the feature is released.

 @param data
 Feature data.

 @param shape
 Feature shape.

 @param dims
 Number of dimensions in shape.

 @param dtype
 Feature data type.

 @param flags
 Feature creation flags.

 @param feature
 Output ML feature.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateArrayFeature (
    void* data,
    const int32_t* shape,
    int32_t dims,
    NMLDataType dtype,
    NMLFeatureFlags flags,
    NMLFeature** feature
);

/*!
 @function NMLCreateImageFeature

 @abstract Create an image feature from a pixel buffer.

 @discussion Create an image feature from a pixel buffer.
 The pixel buffer MUST have an RGBA8888 layout (32 bits per pixel).

 @param pixelBuffer
 Pixel buffer to convert to a feature.

 @param width
 Pixel buffer width.

 @param height
 Pixel buffer height.

 @param shape
 Feature shape.

 @param dtype
 Feature data type. This MUST be `NML_DTYPE_FLOAT32`.

 @param mean
 Per-channel normalization mean.

 @param std
 Per-channel normalization standard deviation.

 @param flags
 Feature creation flags.

 @param feature
 Output ML feature.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateImageFeature (
    const uint8_t* pixelBuffer,
    int32_t width,
    int32_t height,
    const int32_t shape[4],
    NMLDataType dtype,
    const float* mean,
    const float* std,
    NMLFeatureFlags flags,
    NMLFeature** feature
);

/*!
 @function NMLCreateAudioFeature

 @abstract Create an audio feature from a sample buffer.

 @discussion Create an audio feature from a sample buffer.
 The sample buffer MUST be linear PCM interleaved by channel in range [-1.0, 1.0].

 @param sampleBuffer
 Sample buffer to convert to a feature.

 @param bufferSampleRate
 Sample rate of the sample buffer.

 @param bufferShape
 Tensor shape of the sample buffer.
 This is always (1,F,C) where `F` is the frame count and `C` is the channel count.

 @param sampleRate
 Feature sample rate. Sample buffer will be resampled if necessary.

 @param channelCount
 Feature channel count. Sample buffer will be resampled if necessary.

 @param dtype
 Feature data type. This MUST be `NML_DTYPE_FLOAT32`.

 @param mean
 Per-channel normalization mean.

 @param std
 Per-channel normalization standard deviation.

 @param flags
 Feature creation flags.

 @param feature
 Output ML feature.
 This will always be planar with shape (1,C,F) where `C` is the feature channel count and `F` is the feature frame count.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateAudioFeature (
    const float* sampleBuffer,
    int32_t bufferSampleRate,
    const int32_t bufferShape[3],
    int32_t sampleRate,
    int32_t channelCount,
    NMLDataType dtype,
    const float* mean,
    const float* std,
    NMLFeatureFlags flags,
    NMLFeature** feature
);
#pragma endregion


#pragma region --Utilities--
/*!
 @function NMLImageFeatureCopyTo

 @abstract Copy image data from an image feature.

 @discussion Copy image data from an image feature.

 @param srcBuffer
 Source pixel buffer.
 This MUST have an `RGBA8888` layout.

 @param width
 Source pixel buffer width.

 @param height
 Source pixel buffer height.

 @param rect
 Region of interest rectangle in pixel coordinates.
 This MUST be in format (cx,cy,w,h).

 @param rotation
 Region of interest clockwise rotation in degrees.

 @param background
 Background color for out-of-bounds pixels. This is (R,G,B,A).

 @param dstBuffer
 Destination pixel buffer.
 This MUST have an `RGBA8888` layout.
 This MUST be large enough to hold the enough pixel data for the given rectangle size.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLImageFeatureCopyTo (
    const uint8_t* srcBuffer,
    int32_t width,
    int32_t height,
    const int32_t rect[4],
    float rotation,
    const uint8_t background[4],
    uint8_t* dstBuffer
);
#pragma endregion