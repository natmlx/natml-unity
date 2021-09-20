/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;

    public enum NMLDataType : int { // CHECK // Must match `NatML.h`
        Undefined = 0,
        UInt8 = 1,
        Int16 = 2,
        Int32 = 3,
        Int64 = 4,
        Float = 5,
        Double = 6,
        String = 7,
        Sequence = 8,
        Dictionary = 9
    }

    [Flags]
    public enum NMLFeatureFlags : int { }
}