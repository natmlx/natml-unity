//
//  NMLModelConfiguration.h
//  NatML
//
//  Created by Yusuf Olokoba on 9/2/2022.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include "NMLTypes.h"

#pragma region --Enumerations--
/*!
 @enum NMLComputeTarget

 @abstract Compute target used for ML model predictions.

 @constant NML_COMPUTE_TARGET_DEFAULT
 Use the default compute target for the given platform.

 @constant NML_COMPUTE_TARGET_CPU
 Use the CPU.
 
 @constant NML_COMPUTE_TARGET_GPU
 Use the GPU.
 
 @constant NML_COMPUTE_TARGET_NPU
 Use the neural processing unit
 
 @constant NML_COMPUTE_TARGET_ALL
 Use all available compute targets including the CPU, GPU, and neural processing units.
*/
enum NMLComputeTarget {
    NML_COMPUTE_TARGET_DEFAULT  = 0,
    NML_COMPUTE_TARGET_CPU      = 1 << 0,
    NML_COMPUTE_TARGET_GPU      = 1 << 1,
    NML_COMPUTE_TARGET_NPU      = 1 << 2,
    NML_COMPUTE_TARGET_ALL      = NML_COMPUTE_TARGET_CPU | NML_COMPUTE_TARGET_GPU | NML_COMPUTE_TARGET_NPU
};
typedef enum NMLComputeTarget NMLComputeTarget;
#pragma endregion


#pragma region --Types--
/*!
 @struct NMLModelConfiguration

 @abstract Opaque type for NatML edge model configuration.
*/
struct NMLModelConfiguration;
typedef struct NMLModelConfiguration NMLModelConfiguration;

/*!
 @abstract Callback invoked with created secret.
 
 @param context
 User context provided to the secret creation function.
 
 @param secret
 Predictor session secret. If secret creation fails for any reason, this will be `NULL`.
*/
typedef void (*NMLSecretCreationHandler) (void* context, const char* secret);
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function NMLCreateModelConfiguration

 @abstract Create ML model configuration.

 @discussion Create ML model configuration.

 @param configuration
 Destination configuration. Must not be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateModelConfiguration (NMLModelConfiguration** configuration);

/*!
 @function NMLReleaseModelConfiguration

 @abstract Release ML model configuration.

 @discussion Release ML model configuration.

 @param configuration
 ML model configuration.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLReleaseModelConfiguration (NMLModelConfiguration* configuration);
#pragma endregion


#pragma region --Configuration--
/*!
 @function NMLModelConfigurationSetComputeTarget

 @abstract Specify the compute target used for ML model predictions.

 @discussion Specify the compute target used for ML model predictions.

 @param configuration
 ML model configuration.

 @param target
 Compute target used for ML model predictions.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelConfigurationSetComputeTarget (
    NMLModelConfiguration* configuration,
    NMLComputeTarget target
);

/*!
 @function NMLModelConfigurationSetComputeDevice

 @abstract Specify the compute device used for ML model predictions.

 @discussion Specify the compute device used for ML model predictions.

 @param configuration
 ML model configuration.

 @param device
 Compute device used for ML model predictions.
 The type of this device is platform-dependent.
 Pass `NULL` to use the default compute device.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelConfigurationSetComputeDevice (
    NMLModelConfiguration* configuration,
    void* device
);

/*!
 @function NMLModelCofigurationSetFingerprint

 @abstract Set predictor session fingerprint.

 @discussion Set predictor session fingerprint.

 @param configuration
 ML model configuration.

 @param fingerprint
 Predictor session fingerprint. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelConfigurationSetFingerprint (
    NMLModelConfiguration* configuration,
    const char* fingerprint
);

/*!
 @function NMLModelConfigurationSetSecret

 @abstract Set predictor session secret.

 @discussion Set predictor session secret.

 @param configuration
 ML model configuration.

 @param secret
 Predictor session secret. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelConfigurationSetSecret (
    NMLModelConfiguration* configuration,
    const char* secret
);
#pragma endregion


#pragma region --Secret--
/*!
 @function NMLModelConfigurationCreateSecret

 @abstract Create a predictor session secret.

 @discussion Create a predictor session secret.

 @param handler
 Callback to receive the created secret.

 @param context
 User context to pass to handler. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelConfigurationCreateSecret (
    NMLSecretCreationHandler handler,
    void* context
);
#pragma endregion