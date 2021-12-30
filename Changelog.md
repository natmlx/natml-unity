## 1.0.7
+ Added `MLArrayFeature` constructor that accepts an `MLHubFeature` for working with Hub predictors.
+ Added `MLAudioFeature` constructor that accepts an `MLHubFeature` for working with Hub predictors.
+ Added `MLImageFeature` constructor that accepts an `MLHubFeature` for working with Hub predictors.
+ Added `MLTextFeature` constructor that accepts an `MLHubFeature` for working with Hub predictors.
+ Fixed `DirectoryNotFoundException` when loading cached `MLModelData` on iOS.
+ Removed prediction analytics reporting to NatML Hub, relieving network bandwidth pressure.
+ Removed `cache` flag in `MLModelData.FromHub` method.

## 1.0.6
+ Introduced Hub Predictors, which make predictions using server-side processing on [NatML Hub](https://hub.natml.ai).
+ Added `MLHubModel` class for authoring predictors that make cloud-based predictions using NatML Hub.
+ Added `MLEdgeModel` class for authoring predictors that make edge (on-device) predictions.
+ Added `IMLHubFeature` interface for creating server-side features when making predictions with NatML Hub.
+ Added `IMLEdgeFeature` interface for creating native features when making edge (on-device) predictions.
+ Added `MLTextType` feature type for inspecting `MLTextFeature` instances.
+ Added `MLModelData.tag` property to identify the predictor tag from [NatML Hub](https://hub.natml.ai).
+ Added `MLModel.metadata` dictionary for inspecting model metadata.
+ Added `MLArrayFeature.Squeeze` to remove singleton dimensions from an array feature.
+ Added `MLArrayFeature.Flatten` to flatten an array feature into one-dimensional array feature.
+ Added `MLArrayFeature.ToArray` to convert an array feature into a flattened primitive array.
+ Added `MLImageFeature.ToTexture` to convert an image feature into a `Texture2D`.
+ Added `MLImageType.FromType` static method for converting arbitrary feature types to image types.
+ Added implicit conversion from `MLFeatureType` to `bool` indicating if the type is non-`null`.
+ Added implicit conversion from `MLTextFeature` to `string`.
+ Fixed `MLImageType` image resolution constructor assuming planar format instead of interleaved format.
+ Moved `IMLPredictor` interface to the top-level `NatSuite.ML` namespace.
+ Moved `IMLAsyncPredictor` interface to the top-level `NatSuite.ML` namespace.
+ Deprecated `IMLModel` interface. Cast model to `MLEdgeModel` class instead.
+ Deprecated `IMLFeature` interface. Cast feature to `IMLEdgeFeature` interface instead.
+ Deprecated `MLPredictorExtensions.GetImageSize` static method. Use `MLImageType.FromType` instead.
+ Removed `MLModelData.FromFile` method. Use [NatML Hub](https://hub.natml.ai) instead.
+ Removed `MLModelData.FromStreamingAssets` method. Use [NatML Hub](https://hub.natml.ai) instead.
+ Removed `MLPredictorExtensions.SerializeAudio` method.
+ Removed `MLPredictorExtensions.SerializeImage` method.
+ Removed `MLModel` dictionary indexers. Use `MLModel.metadata` property instead.

## 1.0.5
+ Changed `MLImageFeature.mean` and `std` types to `Vector4` to support normalization for alpha channel.
+ Fixed bitcode not being generated for iOS `NatML.framework`.
+ Removed metadata accessors from `IMLModel` interface. Cast to `MLModel` instead.

## 1.0.4
+ Greatly improved performance and memory pressure when performing multi-indexing with `MLArrayFeature<T>`.
+ Added `MLPredictorExtensions.RectifyAspect` extension method for correcting detection rects from aspect-scaled images.
+ Fixed crash when making predictions with recurrent models on previous state features.
+ Fixed crash when getting native array feature shape for `MLArrayFeature<T>`.
+ Fixed memory leak when making predictions with image features on iOS and macOS.

## 1.0.3
+ Added `IMLAsyncPredictor` interface for making server-side ML predictions with NatML Hub.
+ Added `MLArrayFeature<T>` constructor which accepts a native array feature for easy interop.
+ Added multi-indexing support to `MLArrayFeature<T>` for post-processing native array features.
+ Added `MLArrayType.elementCount` property to get the total number of elements for an array type.
+ Added `MLArrayFeature<T>.shape` property which returns the feature type's shape for convenience.
+ Added `MLArrayFeature<T>.elementCount` property which returns the feature type's element count for convenience.
+ Added `MLArrayFeature<T>.CopyTo` method to copy feature data into an array.
+ Added `MLArrayFeature<T>.Permute` method to create a shallow array view with permuted dimensions.
+ Added `MLArrayFeature<T>.View` method to create a shallow array view with a different shape.
+ Added `MLPredictorExtensions.NonMaxSuppression` method for working with detection models.
+ Added `MLPredictorExtensions.GetImageSize` method for making predictions with image features.
+ Added `MLPredictorExtensions.SerializeAudio` method for making Hub predictions with audio features.
+ Added `MLPredictorExtensions.SerializeImage` method for making Hub predictions with image features.
+ Fixed `MLAsyncPredictor` predictions never completing if backing predictor encountered exception.

## 1.0.2
+ Added `MLModelData.audioFormat` property for working with audio and speech ML models.
+ Added `MLTextFeature` for working with natural language processing models.
+ Added NatML menu items for fetching access key, viewing models, and more.
+ Exposed `mean` and `std` arrays in `MLModelData.Normalization` struct for models that require arbitrary normalization.
+ Removed generic `MLClassificationPredictor` and `MLDenseClassificationPredictor` predictors.
+ Removed ability to specify class labels for local `.onnx` file in project. Use NatML Hub instead.

## 1.0.0
+ First release.