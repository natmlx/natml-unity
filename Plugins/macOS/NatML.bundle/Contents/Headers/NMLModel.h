//
//  NMLModel.h
//  NatML
//
//  Created by Yusuf Olokoba on 3/14/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NMLFeature.h"

#pragma region --Types--
/*!
 @struct NMLModel

 @abstract Opaque type for a NatML model.
*/
struct NMLModel;
typedef struct NMLModel NMLModel;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function NMLCreateModel

 @abstract Create a NatML model.

 @discussion Create a NatML model.

 @param buffer
 Model data.

 @param bufferSize
 Model data size.

 @param reserved
 Reserved parameter. MUST be `0`.

 @param model
 Destination pointer to created model or `NULL` if model creation failed.
*/
BRIDGE EXPORT void APIENTRY NMLCreateModel (
    const void* buffer,
    int32_t bufferSize,
    int32_t reserved,
    NMLModel** model
);

/*!
 @function NMLReleaseModel

 @abstract Release a model.

 @discussion Release a model.

 @param model
 Opaque handle to a NatML model.
*/
BRIDGE EXPORT void APIENTRY NMLReleaseModel (NMLModel* model);
#pragma endregion


#pragma region --Metadata--
/*!
 @function NMLModelGetMetadataCount

 @abstract Get the number of metadata keys in a model.

 @discussion Get the number of metadata keys in a model.

 @param model
 Opaque handle to a NatML model.

 @returns Number of metadata keys in the model.
*/
BRIDGE EXPORT int32_t APIENTRY NMLModelGetMetadataCount (NMLModel* model);

/*!
 @function NMLModelGetMetadataKey

 @abstract Get the metadata key in a model.

 @discussion Get the metadata key in a model.

 @param model
 Opaque handle to a NatML model.

 @param index
 Metadata key index.

 @param key
 Destination UTF-8 encoded key string.

 @param size
 Size of destination buffer.
*/
BRIDGE EXPORT void APIENTRY NMLModelGetMetadataKey (NMLModel* model, int32_t index, char* key, int32_t size);

/*!
 @function NMLModelGetMetadataValue

 @abstract Get the metadata value for a correspondig key in a model.

 @discussion Get the metadata value for a correspondig key in a model.

 @param model
 Opaque handle to a NatML model.

 @param key
 Metadata key.

 @param value
 Destination UTF-8 encoded value string.

 @param size
 Size of destination buffer.
*/
BRIDGE EXPORT void APIENTRY NMLModelGetMetadataValue (NMLModel* model, const char* key, char* value, int32_t size);
#pragma endregion


#pragma region --Introspection--
/*!
 @function NMLModelGetInputFeatureCount

 @abstract Get the number of input features in a model.

 @discussion Get the number of input features in a model.

 @param model
 Opaque handle to a NatML model.

 @returns Number of input features in the model.
*/
BRIDGE EXPORT int32_t APIENTRY NMLModelGetInputFeatureCount (NMLModel* model);

/*!
 @function NMLModelGetInputFeatureType

 @abstract Get the input feature type in a model.

 @discussion Get the input feature type in a model.

 @param model
 Opaque handle to a NatML model.

 @param index
 Input feature index.

 @param type
 Output feature type. This type should be released once it is no longer in use.
*/
BRIDGE EXPORT void APIENTRY NMLModelGetInputFeatureType (NMLModel* model, int32_t index, NMLFeatureType** type);

/*!
 @function NMLModelGetOutputFeatureCount

 @abstract Get the number of output features in a model.

 @discussion Get the number of output features in a model.

 @param model
 Opaque handle to a NatML model.

 @returns Number of output features in the model.
*/
BRIDGE EXPORT int32_t APIENTRY NMLModelGetOutputFeatureCount (NMLModel* model);

/*!
 @function NMLModelGetOutputFeatureType

 @abstract Get the output feature type in a model.

 @discussion Get the output feature type in a model.

 @param model
 Opaque handle to a created model.

 @param index
 Output feature index.

 @param type
 Output feature type. This type should be released once it is no longer in use.
*/
BRIDGE EXPORT void APIENTRY NMLModelGetOutputFeatureType (NMLModel* model, int32_t index, NMLFeatureType** type);
#pragma endregion


#pragma region --Inference--
/*!
 @function NMLModelPredict

 @abstract Run inference on a model.

 @discussion Run inference on a model.

 @param model
 Opaque handle to a NatML model.

 @param inputs
 Input features to run prediction on. The length of this array must match the model's input count.

 @param outputs
 Output features. The length of this array must match the model's output count.
*/
BRIDGE EXPORT void APIENTRY NMLModelPredict (NMLModel* model, NMLFeature * const * inputs, NMLFeature** outputs);
#pragma endregion
