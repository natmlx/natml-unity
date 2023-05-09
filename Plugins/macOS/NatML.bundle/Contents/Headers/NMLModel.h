//
//  NMLModel.h
//  NatML
//
//  Created by Yusuf Olokoba on 3/14/2021.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include "NMLFeature.h"
#include "NMLModelConfiguration.h"

#pragma region --Types--
/*!
 @struct NMLModel

 @abstract Opaque type for a NatML model.
*/
struct NMLModel;
typedef struct NMLModel NMLModel;

/*!
 @abstract Callback invoked with created model.
 
 @param context
 User context provided to the model creation function.
 
 @param model
 ML model. If model creation fails for any reason, this will be `NULL`.
*/
typedef void (*NMLModelCreationHandler) (void* context, NMLModel* model);
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function NMLCreateModel

 @abstract Create an ML model.

 @discussion Create an ML model.

 @param buffer
 ML model data.
 The buffer can be released immediately after calling this function.

 @param bufferSize
 ML model data size.

 @param configuration
 ML model configuration.
 This can be `NULL` in which case reasonable defaults will be used.
 The configuration can be released immediately after calling this function.

 @param handler
 Callback to receive created model.

 @param context
 User context to pass to handler. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateModel (
    const uint8_t* buffer,
    int64_t bufferSize,
    NMLModelConfiguration* configuration,
    NMLModelCreationHandler handler,
    void* context
);

/*!
 @function NMLReleaseModel

 @abstract Release an ML model.

 @discussion Release an ML model.

 @param model
 ML model.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLReleaseModel (NMLModel* model);
#pragma endregion


#pragma region --Metadata--
/*!
 @function NMLModelGetMetadataCount

 @abstract Get the number of metadata keys in a model.

 @discussion Get the number of metadata keys in a model.

 @param model
 ML model.

 @returns Number of metadata keys in the model.
*/
NML_BRIDGE NML_EXPORT int32_t NML_API NMLModelGetMetadataCount (NMLModel* model);

/*!
 @function NMLModelGetMetadataKey

 @abstract Get the metadata key in a model.

 @discussion Get the metadata key in a model.

 @param model
 ML model.

 @param index
 Metadata key index.

 @param key
 Destination UTF-8 encoded key string.

 @param size
 Size of destination buffer.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelGetMetadataKey (
    NMLModel* model,
    int32_t index,
    char* key,
    int32_t size
);

/*!
 @function NMLModelGetMetadataValue

 @abstract Get the metadata value for a correspondig key in a model.

 @discussion Get the metadata value for a correspondig key in a model.

 @param model
 ML model.

 @param key
 Metadata key.

 @param value
 Destination UTF-8 encoded value string.

 @param size
 Size of destination buffer.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelGetMetadataValue (
    NMLModel* model,
    const char* key,
    char* value,
    int32_t size
);
#pragma endregion


#pragma region --Inspection--
/*!
 @function NMLModelGetInputFeatureCount

 @abstract Get the number of input features in a model.

 @discussion Get the number of input features in a model.

 @param model
 ML model.

 @returns Number of input features in the model.
*/
NML_BRIDGE NML_EXPORT int32_t NML_API NMLModelGetInputFeatureCount (NMLModel* model);

/*!
 @function NMLModelGetInputFeatureType

 @abstract Get the input feature type in a model.

 @discussion Get the input feature type in a model.

 @param model
 ML model.

 @param index
 Input feature index.

 @param type
 Output feature type. This type should be released once it is no longer in use.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelGetInputFeatureType (
    NMLModel* model,
    int32_t index,
    NMLFeatureType** type
);

/*!
 @function NMLModelGetOutputFeatureCount

 @abstract Get the number of output features in a model.

 @discussion Get the number of output features in a model.

 @param model
 ML model.

 @returns Number of output features in the model.
*/
NML_BRIDGE NML_EXPORT int32_t NML_API NMLModelGetOutputFeatureCount (NMLModel* model);

/*!
 @function NMLModelGetOutputFeatureType

 @abstract Get the output feature type in a model.

 @discussion Get the output feature type in a model.

 @param model
 ML model.

 @param index
 Output feature index.

 @param type
 Output feature type. This type should be released once it is no longer in use.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelGetOutputFeatureType (
    NMLModel* model,
    int32_t index,
    NMLFeatureType** type
);
#pragma endregion


#pragma region --Prediction--
/*!
 @function NMLModelPredict

 @abstract Make a prediction with a model.

 @discussion Make a prediction with a model.

 @param model
 ML model.

 @param inputs
 Input features to run prediction on. The length of this array must match the model's input count.

 @param outputs
 Output features. The length of this array must match the model's output count.
 Each feature MUST be released when no longer needed.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelPredict (
    NMLModel* model,
    NMLFeature * const * inputs,
    NMLFeature** outputs
);
#pragma endregion