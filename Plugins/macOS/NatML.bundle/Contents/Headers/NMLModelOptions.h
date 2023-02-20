//
//  NMLModelOptions.h
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
 @struct NMLModelOptions

 @abstract Opaque type for NatML model options.
*/
struct NMLModelOptions;
typedef struct NMLModelOptions NMLModelOptions;

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
 @function NMLCreateModelOptions

 @abstract Create ML model options.

 @discussion Create ML model options.

 @param options
 Destination pointer to created ML model options or `NULL` if model options creation failed.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLCreateModelOptions (NMLModelOptions** options);

/*!
 @function NMLReleaseModelOptions

 @abstract Release ML model options.

 @discussion Release ML model options.

 @param options
 ML model options.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLReleaseModelOptions (NMLModelOptions* options);
#pragma endregion


#pragma region --Options--
/*!
 @function NMLModelOptionsSetComputeTarget

 @abstract Specify the compute target used for ML model predictions.

 @discussion Specify the compute target used for ML model predictions.

 @param options
 ML model options.

 @param target
 Compute target used for ML model predictions.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelOptionsSetComputeTarget (
    NMLModelOptions* options,
    NMLComputeTarget target
);

/*!
 @function NMLModelOptionsSetComputeDevice

 @abstract Specify the compute device used for ML model predictions.

 @discussion Specify the compute device used for ML model predictions.

 @param options
 ML model options.

 @param device
 Compute device used for ML model predictions.
 The type of this device is platform-dependent.
 Pass `NULL` to use the default compute device.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelOptionsSetComputeDevice (
    NMLModelOptions* options,
    void* device
);

/*!
 @function NMLModelOptionsSetFingerprint

 @abstract Set predictor session fingerprint.

 @discussion Set predictor session fingerprint.

 @param options
 ML model options.

 @param fingerprint
 Predictor session fingerprint. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelOptionsSetFingerprint (
    NMLModelOptions* options,
    const char* fingerprint
);

/*!
 @function NMLModelOptionsSetSecret

 @abstract Set predictor session secret.

 @discussion Set predictor session secret.

 @param options
 ML model options.

 @param secret
 Predictor session secret. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelOptionsSetSecret (
    NMLModelOptions* options,
    const char* secret
);
#pragma endregion


#pragma region --Secret--
/*!
 @function NMLModelOptionsCreateSecret

 @abstract Create a predictor session secret.

 @discussion Create a predictor session secret.

 @param handler
 Callback to receive the created secret.

 @param context
 User context to pass to handler. Can be `NULL`.
*/
NML_BRIDGE NML_EXPORT void NML_API NMLModelOptionsCreateSecret (
    NMLSecretCreationHandler handler,
    void* context
);
#pragma endregion