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