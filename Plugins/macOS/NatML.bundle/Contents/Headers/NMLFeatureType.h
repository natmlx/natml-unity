//
//  NMLFeatureType.h
//  NatML
//
//  Created by Yusuf Olokoba on 3/14/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NMLTypes.h"

#pragma region --Enumerations--
/*!
 @enum NMLDataType

 @abstract Feature data type.

 @constant NML_DTYPE_UNDEFINED
 Type is undefined or invalid.

 @constant NML_DTYPE_UINT8
 Type is `uint8_t` in C/C++ and `byte` in C#.

 @constant NML_DTYPE_INT16
 Type is `int16_t` in C/C++ and `short` in C#.

 @constant NML_DTYPE_INT32
 Type is `int32_t` in C/C++ and `int` in C#.

 @constant NML_DTYPE_INT64
 Type is `int64_t` in C/C++ and `long` in C#.

 @constant NML_DTYPE_FLOAT32
 Type is `float` in C/C++/C#.

 @constant NML_DTYPE_FLOAT64
 Type is `double` in C/C++/C#.

 @constant NML_DTYPE_STRING
 Type is `std::string` in C++ and `string` in C#.

 @constant NML_DTYPE_SEQUENCE
 Type is a sequence.

 @constant NML_DTYPE_DICTIONARY
 Type is a dictionary.

 @constant NML_DTYPE_INT8
 Type is a `int8_t` in C/C++ and `sbyte` in C#.

 @constant NML_DTYPE_UINT16
 Type is a `uint16_t` in C/C++ and `ushort` in C#.

 @constant NML_DTYPE_UINT32
 Type is a `uint32_t` in C/C++ and `uint` in C#.

 @constant NML_DTYPE_UINT64
 Type is a `uint64_t` in C/C++ and `ulong` in C#.

 @constant NML_DTYPE_FLOAT16
 Type is a generic half-precision float.
*/
enum NMLDataType {
    NML_DTYPE_UNDEFINED     = 0,
    NML_DTYPE_UINT8         = 1,
    NML_DTYPE_INT16         = 2,
    NML_DTYPE_INT32         = 3,
    NML_DTYPE_INT64         = 4,
    NML_DTYPE_FLOAT32       = 5,
    NML_DTYPE_FLOAT64       = 6,
    NML_DTYPE_STRING        = 7,
    NML_DTYPE_SEQUENCE      = 8,
    NML_DTYPE_DICTIONARY    = 9,
    NML_DTYPE_INT8          = 10,
    NML_DTYPE_UINT16        = 11,
    NML_DTYPE_UINT32        = 12,
    NML_DTYPE_UINT64        = 13,
    NML_DTYPE_FLOAT16       = 14,
};
typedef enum NMLDataType NMLDataType;
#pragma endregion


#pragma region --Types--
/*!
 @struct NMLFeatureType
 
 @abstract Descriptor for an ML feature.

 @discussion Descriptor for an ML feature.
*/
struct NMLFeatureType;
typedef struct NMLFeatureType NMLFeatureType;
#pragma endregion


#pragma region --Operations--
/*!
 @function NMLReleaseFeatureType

 @abstract Release an ML feature type.

 @discussion Release an ML feature type.

 @param type
 Feature type to release.
*/
BRIDGE EXPORT void APIENTRY NMLReleaseFeatureType (NMLFeatureType* type);

/*!
 @function NMLFeatureTypeName

 @abstract Get the name of a given feature type.

 @discussion Get the name of a given feature type.

 @param type
 Feature type.

 @param name
 Destination UTF-8 string.

 @param size
 Size of destination buffer in bytes.
*/
BRIDGE EXPORT void APIENTRY NMLFeatureTypeGetName (NMLFeatureType* type, char* name, int32_t size);

/*!
 @function NMLFeatureTypeDataType

 @abstract Get the data type of a given feature type.

 @discussion Get the data type of a given feature type.

 @param type
 Feature type.
*/
BRIDGE EXPORT NMLDataType APIENTRY NMLFeatureTypeGetDataType (NMLFeatureType* type);

/*!
 @function NMLFeatureTypeDimensions

 @abstract Get the number of dimensions for a given feature type.

 @discussion Get the number of dimensions for a given feature type.
 If the type is not a tensor, this function will return `0`.

 @param type
 Feature type.
*/
BRIDGE EXPORT int32_t APIENTRY NMLFeatureTypeGetDimensions (NMLFeatureType* type);

/*!
 @function NMLFeatureTypeShape

 @abstract Get the shape of a given feature type.

 @discussion Get the shape of a given feature type.
 The length of the shape array must be at least as large as the number of dimensions present in the type.

 @param type
 Feature type.

 @param shape
 Destination shape array.

 @param shapeLen
 Destination shape array length.
*/
BRIDGE EXPORT void APIENTRY NMLFeatureTypeGetShape (NMLFeatureType* type, int32_t* shape, int32_t shapeLen);
#pragma endregion
