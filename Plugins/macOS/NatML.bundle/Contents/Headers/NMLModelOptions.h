//
//  NMLModelOptions.h
//  NatML
//
//  Created by Yusuf Olokoba on 9/2/2022.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NMLTypes.h"

#pragma region --Enumerations--
/*!
 @enum NMLComputeTarget

 @abstract Compute target used for ML model predictions.

 @constant NML_COMPUTE_TARGET_ALL
 Use all available compute targets including the CPU, GPU, and neural processing units.

 @constant NML_COMPUTE_TARGET_CPU_ONLY
 Use only the CPU.
*/
enum NMLComputeTarget {
    NML_COMPUTE_TARGET_ALL          = 0,
    NML_COMPUTE_TARGET_CPU_ONLY     = 1
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
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function NMLCreateModelOptions

 @abstract Create ML model options.

 @discussion Create ML model options.

 @param options
 Destination pointer to created ML model options or `NULL` if model options creation failed.
*/
BRIDGE EXPORT void APIENTRY NMLCreateModelOptions (NMLModelOptions** options);

/*!
 @function NMLReleaseModelOptions

 @abstract Release ML model options.

 @discussion Release ML model options.

 @param options
 ML model options.
*/
BRIDGE EXPORT void APIENTRY NMLReleaseModelOptions (NMLModelOptions* options);
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
BRIDGE EXPORT void APIENTRY NMLModelOptionsSetComputeTarget (
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
BRIDGE EXPORT void APIENTRY NMLModelOptionsSetComputeDevice (
    NMLModelOptions* options,
    void* device
);

/*!
 @function NMLModelOptionsSetHubFlags

 @abstract Set NatML Hub prediction session flags.

 @discussion Set NatML Hub prediction session flags.

 @param options
 ML model options.

 @param flags
 NatML Hub session flags.
*/
BRIDGE EXPORT void APIENTRY NMLModelOptionsSetHubSessionFlags (
    NMLModelOptions* options,
    int32_t flags
);
#pragma endregion