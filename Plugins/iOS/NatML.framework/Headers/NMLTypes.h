//
//  NMLTypes.h
//  NatML
//
//  Created by Yusuf Olokoba on 3/14/2021.
//  Copyright © 2023 NatML Inc. All rights reserved.
//

#pragma once

#ifdef __cplusplus
    #define NML_BRIDGE extern "C"
#else
    #define NML_BRIDGE extern
#endif

#ifdef _WIN64
    #define NML_EXPORT __declspec(dllexport)
#else
    #define NML_EXPORT
#endif

#ifdef __EMSCRIPTEN__
    #define NML_API EMSCRIPTEN_KEEPALIVE
#elif defined(_WIN64)
    #define NML_API APIENTRY
#else
    #define NML_API
#endif