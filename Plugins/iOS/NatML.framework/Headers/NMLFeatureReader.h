//
//  NMLMedia.h
//  NatML
//
//  Created by Yusuf Olokoba on 1/23/2022.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#include "NMLFeature.h"

#pragma region --Types--
/*!
 @struct NMLFeatureReader
 
 @abstract Feature reader for reading feature data from media sources.

 @discussion Feature reader for reading feature data from media sources.
*/
struct NMLFeatureReader;
typedef struct NMLFeatureReader NMLFeatureReader;
#pragma endregion


#pragma region --Inspection--
/*!
 @function NMLFeatureReaderGetVideoFormat

 @abstract Get the video format from a video file.

 @discussion Get the video format from a video file.

 @param path
 Path to video file on the system.

 @param outWidth
 Output width.

 @param outHeight
 Output width.

 @param outFrames
 Output frame count.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLFeatureReaderGetVideoFormat (
    const char* path,
    int32_t* outWidth,
    int32_t* outHeight,
    int32_t* outFrames
);

/*!
 @function NMLFeatureReaderGetAudioFormat

 @abstract Get the audio format from a video or audio file.

 @discussion Get the audio format from a video or audio file.

 @param path
 Path to video or audio file on the system.

 @param outSampleRate
 Output sample rate.

 @param outChannelCount
 Output channel count.

 @param outSampleCount
 Output total sample count.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLFeatureReaderGetAudioFormat (
    const char* path,
    int32_t* outSampleRate,
    int32_t* outChannelCount,
    int32_t* outSampleCount
);
#pragma endregion


#pragma region --Reader--
/*!
 @function NMLReleaseFeatureReader

 @abstract Release a feature reader.

 @discussion Release a feature reader.

 @param reader
 ML feature reader.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLReleaseFeatureReader (NMLFeatureReader* reader);

/*!
 @function NMLFeatureReaderReadNextFeature

 @abstract Read the next feature from a feature reader.
 
 @discussion Read the next feature from a feature reader.
 The feature must be released with `NMLReleaseFeature` when it is no longer needed.
 If there are no more features available, the output timestamp will be `-1`.

 @param reader
 ML feature reader.

 @param timestamp
 Output feature timestamp in nanoseconds.

 @param feature
 Output feature. The feature MUST be released when it is no longer needed.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLFeatureReaderReadNextFeature (
    NMLFeatureReader* reader,
    int64_t* timestamp,
    NMLFeature** feature
);
#pragma endregion


#pragma region --Constructors--
/*!
 @function NMLCreateImageFeatureReader

 @abstract Create an image feature reader.
 
 @discussion Create an image feature reader.
 This currently supports reading from `.mp4` files.

 @param path
 Path to video file.

 @param reader
 Output ML feature reader.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateImageFeatureReader (
    const char* path,
    NMLFeatureReader** reader
);

/*!
 @function NMLCreateAudioFeatureReader

 @abstract Create an audio feature reader.
 
 @discussion Create an audio feature reader.
 This currently supports reading from `.mp3` and `.mp4` files.

 @param path
 Path to audio or video file.

 @param reader
 Output ML feature reader.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateAudioFeatureReader (
    const char* path,
    NMLFeatureReader** reader
);
#pragma endregion