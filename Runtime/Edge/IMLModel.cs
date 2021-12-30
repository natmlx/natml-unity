/* 
*   NatML
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.ML.Internal {

    using System;

    [Obsolete("Deprecated in NatML 1.0.6. Use `MLEdgeModel` instead.", false)]
    public interface IMLModel : IDisposable {

        MLFeatureType[] inputs { get; }

        MLFeatureType[] outputs { get; }

        IntPtr[] Predict (params IntPtr[] inputs);
    }
}