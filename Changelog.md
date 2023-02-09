## 1.1.1
+ Added model embedding support for encrypted models.

## 1.1.0
+ Added `MLCloudModel` class for making predictions with predictor endpoints.
+ Added `MLEdgeModel.Create` methods for creating an edge model from a predictor tag or an `MLModelData` instance.
+ Added `MLEdgeModel.ComputeTarget` enumeration for specifying the compute target for model predictions.
+ Added `MLEdgeModel.Configuration` data class for configuring the edge model creation process.
+ Added `MLEdgeModel.labels` property for inspecting classification labels required by the model.
+ Added `MLEdgeModel.aspectMode` property for inspecting the aspect mode required in image data consumed by the model.
+ Added `MLEdgeModel.audioFormat` property for inspecting the audio format required in audio data consumed by the model.
+ Added support for model encryption, protecting your valuable intellectual property.
+ Added support for pinning `MLArrayFeature` instances with `fixed` statements.
+ Added support for pinning `MLImageFeature` instances with `fixed` statements.
+ Added `MLArrayFeature` constructor from an `MLCloudFeature` for making predictions with predictor endpoints.
+ Added `MLImageFeature` constructor from an `MLCloudFeature` for making predictions with predictor endpoints.
+ Added `MLTextFeature` constructor from an `MLCloudFeature` for making predictions with predictor endpoints.
+ Added `MLArrayFeature.CopyTo(MLArrayFeature)` method for copying image data from one image feature to another.
+ Added `MLImageFeature.CopyTo(MLImageFeature)` method for copying image data from one image feature to another.
+ Added implicit conversion from `float` to `MLFeature`.
+ Added implicit conversion from `int` to `MLFeature`.
+ Added implicit conversion from `bool` to `MLFeature`.
+ Added implicit conversion from `int[]` to `MLFeature`.
+ Updated `MLImageFeature` class to be `sealed` thus preventing inheritance.
+ Updated `float[]` implicit conversion to `MLArrayFeature` to have a 1D shape instead of a `null` shape.
+ Refactored `MLModelData.ComputeTarget` enumeration to `MLEdgeModel.ComputeTarget`.
+ Refactored `MLModelData.Normalization` enumeration to `MLEdgeModel.Normalization`.
+ Refactored `MLModelData.AudioFormat` enumeration to `MLEdgeModel.AudioFormat`.
+ Deprecated `MLImageFeature.RegionOfInterest` method. Use `MLImageFeature.CopyTo` method instead.
+ Deprecated `MLModelData.ComputeTarget.CPUOnly` enumeration member. Use `ComputeTarget.CPU` instead.
+ Removed `MLEdgeModel` constructor. Use `MLEdgeModel.Create` method instead.
+ Removed `MLModelData.Deserialize` method. Use `MLEdgeModel.Create` method instead.
+ Removed `MLModelData.FromHub` method. Use `MLEdgeModel.Create` method instead.
+ Removed `MLModelData.FromFile` method. Use `MLEdgeModel.Create` method instead.
+ Removed `MLModelData.tag` property.
+ Removed `MLModelData.computeTarget` property. Use `MLEdgeModel.Configuration.computeTarget` property instead.
+ Removed `MLModelData.computeDevice` property. Use `MLEdgeModel.Configuration.computeDevice` property instead.
+ Removed `MLModelData.labels` property. Use `MLEdgeModel.labels` property instead.
+ Removed `MLModelData.normalization` property. Use `MLEdgeModel.normalization` property instead.
+ Removed `MLModelData.audioFormat` property. Use `MLEdgeModel.audioFormat` property instead.
+ Removed `MLModelData.ComputeTarget` enumeration.
+ Removed `MLArrayFeature.CopyTo(T[])` method overload.
+ Removed `MLArrayFeature.CopyTo(NativeArray<T>)` method overload.
+ Removed `MLArrayFeature.CopyTo(T*)` method overload.
+ Removed `MLImageFeature.CopyTo(T[])` method overload.
+ Removed `MLImageFeature.CopyTo(NativeArray<T>)` method overload.
+ Removed `MLImageFeature.CopyTo(T*)` method overload.
+ NatML now requires iOS 14+.

## 1.0.19
+ Added `MLEdgeModel` public constructor and moved the class to the top-level `NatML` namespace.
+ Fixed app storage size increasing every time that `MLModelData.Deserialize` was invoked on iOS.
+ Deprecated `MLModelData.Deserialize` method. Use `MLEdgeModel` constructor instead.

## 1.0.18
+ NatML now defaults to accelerating predictions on Windows by using the GPU. This results in much better performance and lower CPU usage for many models.
+ Added `MLModelData.computeTarget` property for specifying the compute target used for prediction.
+ Added `MLModelData.computeDevice` property for specifying the compute device used for prediction.
+ Added `MLImageFeature.CopyTo` overloads that accepted pixel buffers in managed and unmanaged memory.
+ Fixed `MLImageFeature` producing incorrect prediction results on WebGL.
+ Fixed rare crash when calling `MLModelData.Deserialize` on low-end Android devices.

## 1.0.17
+ Upgraded to Hub 1.0.12.

## 1.0.16
+ Refactored `MLDepthFeature.TransformPoint` method to `ViewportToWorldPoint`.
+ Removed `MLImageFeature.CopyTo` overloads that accepted pixel buffers. Use `Texture2D` overload instead.
+ Removed `MLImageFeature.ToTexture` method. Use `MLImageFeature.CopyTo` method instead.

## 1.0.15
+ Added initial support for WebGL! NatML can now be used in the browser.
+ Improved `MLImageFeature` texture constructor to avoid copying pixel buffers when possible.
+ Improved performance of `MLImageFeature.NonMaxSuppression` for large number of candidate boxes.
+ Changed `MLEdgeFeature.dataType` property type from `System.Type` to `NatML.DataType`.

## 1.0.13
+ Added support for rotated ROI in `MLImageFeature.RegionOfInterest` method.
+ Added `MLModelData.FromFile` method to load ML model data from model files.
+ Added `MLImageFeature.TransformPoint` method for transforming detection points from feature space to image space.
+ Added `MLDepthFeature.TransformPoint` method for projecting 2D points into 3D space using depth.
+ Added `MLEdgeFeature.dataType` property for inspecting the data type of Edge features.
+ Added `MLAsyncPredictor` class for making predictions on a background thread.
+ Added `MLPredictorExtensions.ToAsync` extension method for converting predictor to an async predictor.
+ Improved prediction performance on Android devices with dedicated neural processing units.
+ Changed `MLEdgeFeature` class to `readonly struct` to prevent GC pressure.
+ Fixed `MLImageFeature` not respecting `AspectMode.AspectFit` when making predictions.
+ Fixed sporadic `NullReferenceException` when `MLModelData.FromHub` is called on some Android devices.
+ Updated `MLDepthFeature.Sample` method to accept a `Vector2` point instead of individual coordinates.
+ Removed `IMLCloudFeature` interface as it is no longer supported by the NatML Hub API.
+ Removed `MLCloudFeature` class as it is no longer supported by the NatML Hub API.
+ Removed `MLCloudModel` class as it is no longer supported by the NatML Hub API.
+ Removed `IMLAsyncPredictor` interface.
+ Removed `MLFeature.CloudType` utility method as it is no longer supported by the NatML Hub API.
+ Removed `MLArrayFeature` constructor that accepted an `MLCloudFeature`.
+ Removed `MLAudioFeature` constructor that accepted an `MLCloudFeature`.
+ Removed `MLImageFeature` constructor that accepted an `MLCloudFeature`.
+ Removed `MLImageFeature` constructor that accepted an encoded image `byte[]`.
+ Removed `MLImageFeature.Contiguous` method.
+ Removed `MLTextFeature` constructor that accepted an `MLCloudFeature`.
+ Removed `MLEdgeFeature.ReleaseFeature` method as it has long been deprecated.

## 1.0.12
+ Upgraded to Hub 1.0.8.

## 1.0.11
+ Added `MLModelDataEmbed` attribute for embedding model data at build time, making models immediately available in builds without downloads.
+ Added `MLImageFeature.NonMaxSuppression` method for performing non-maximum suppression on detection proposals.
+ Added `MLImageFeature.TransformRect` method for transforming detection rectangles from feature space to image space.
+ Added custom icon for identifying ML model files imported by NatML.
+ Migrated `MLPredictorExtensions.ToAsync` extension method and `MLAsyncPredictor` class to NatMLX.
+ Removed `MLPredictorExtensions.RectifyAspect` method. Use `MLImageFeature.TransformRect` method instead.
+ Removed `MLPredictorExtensions.NonMaxSuppression` method. Use `MLImageFeature.NonMaxSuppression` method instead.
+ Refactored top-level namespace from `NatSuite.ML` to `NatML` for parity with our other API's.

## 1.0.10
+ Improved prediction performance on Windows systems with dedicated GPU's.
+ Added `MLArrayFeature` constructors that accept `NativeArray<T>` native arrays to minimize memory copies.
+ Added setter accessors to `MLArrayFeature` indexers allowing for writing values to feature data.
+ Added `MLAudioFeature.Contiguous` method for decoding encoded audio feature into memory.
+ Added support for creating greyscale image `MLEdgeFeature` features from `MLImageFeature` features.
+ Added `MLImageFeature.RegionOfInterest` method for extracting an ROI from an image.
+ Added `MLImageFeature.Contiguous` method for decoding encoded image feature into memory.
+ Added `MLDepthFeature.width` convenience property for getting width of depth features.
+ Added `MLDepthFeature.height` convenience property for getting height of depth features.
+ Added `MLImageType.interleaved` property for checking whether image feature is interleaved or planar.
+ Added `IMLCloudFeature` interface to create cloud ML features for making cloud predictions.
+ Added "Clear Predictor Cache" menu item for clearing predictor cache in the editor.
+ Added `NatMLHub.Subscribe` method for making subscription requests to the NatML API.
+ Fixed memory leak when using certain vision predictors like Robust Video Matting.
+ Refactored `MLHubModel` class to `MLCloudModel`.
+ Refactored `MLHubFeature` class to `MLCloudFeature`.
+ Refactored `HubDataType` class to `DataType`.
+ Deprecated `IMLHubFeature` interface.
+ Removed `MLAudioFeature.ReadToEnd` method. Use `MLAudioFeature.Contiguous` method instead.

## 1.0.9
+ Added exclusive support for running CoreML graphs on iOS and macOS.
+ Added exclusive support for running TensorFlow Lite graphs on Android.
+ Added support for working with CoreML `.mlmodel` files in Unity projects.
+ Added support for working with TensorFlow Lite `.tflite` files in Unity projects.
+ Added support for Apple Silicon on macOS.
+ Added `MLVideoFeature` class for making ML predictions on video files.
+ Added `MLVideoType` feature type for inspecting video features.
+ Added `MLDepthFeature` abstract class for working with predictors that use depth data.
+ Added support for audio feature resampling in `MLAudioFeature` with `sampleRate` and `channelCount` fields.
+ Added `MLEdgeFeature` type for added type safety when authoring edge predictors.
+ Added `MLImageFeature` constructor which accepts a `NativeArray<byte>` pixel buffer.
+ Added `MLImageFeature.width` convenience property for getting width of image features.
+ Added `MLImageFeature.height` convenience property for getting height of image features.
+ Added `MLImageFeature.CopyTo` methods for copying pixel data from image feature.
+ Added `MLAudioFeature` constructor that accepts a `NativeArray<float>` to minimize memory copies.
+ Added `MLAudioFeature` path constructor for reading audio features from audio and video files.
+ Added `MLAudioFeature.CopyTo` methods for copying audio data from audio feature.
+ Added `MLAudioFeature.FromStreamingAssets` method for creating an audio feature from an audio file in the `StreamingAssets` folder. 
+ Added `MLAudioFeature.ToAudioClip` method for converting audio feature to an `AudioClip`.
+ Added `MLAudioFeature.ReadToEnd` method to read audio data into memory for audio features backed by an audio file.
+ Added `MLAudioType.FromFile` method for inspecting the audio type of a video or audio file.
+ Added `MLAudioType.FromAudioClip` method for inspecting the audio type of an audio clip.
+ Added `MLAudioType.FromVideoClip` method for inspecting the audio type of a video clip.
+ Added `MLAudioType.FromStreamingAssets` method for inspecting the audio type of a video file in the `StreamingAssets` folder.
+ Added `MLImageType` constructor that accepts image `channels`.
+ Fixed `MLImageFeature` type incorrectly reporting 3 channels instead of 4.
+ Fixed `MLImageFeature` default normalization standard deviation having `0` for alpha channel.
+ Removed `IMLModel` interface as it has long been deprecated.
+ Removed `IMLFeature` interface as it has long been deprecated.

## 1.0.8
+ Fixed `Cannot deserialize graph` exception when deserializing cached predictors.
+ Fixed `MLModelData` being cached for `DRAFT` predictors.

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
+ Added `MLTextType` feature type for inspecting `MLTextFeature` features.
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