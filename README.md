# NatML

![NatML](.media/wall.png)

NatML is a high-performance cross-platform machine learning runtime for Unity Engine. Designed for app and game developers, NatML completely removes the need to have any experience with machine learning in order to take advantage of the feautres that they can provide. Features include:

- **Bare Metal Performance**. NatML takes advantage of hardware machine learning accelerators, like CoreML on iOS and macOS, NNAPI on Android, and DirectML on Windows. As a result, it is [multiple times faster](https://github.com/natsuite/ML-Bench) than Unity's own Barracuda engine.

- **Extremely Easy to Use**. NatML exposes machine learning models with simple classes that return familiar data types, with all conversions to and from the model handled for you. No need to write pre-processing scripts or shaders, wrangle tensors, or anything of that sort.

- **Full Support for ONNX**. NatML supports the full ONNX specification, with all layers supported on the CPU, and a large number supported on dedicated hardware accelerators. You can drag and drop any ONNX model from anywhere and run it, no conversions needed.

- **Cross Platform**. NatML supports Android, iOS, macOS, and Windows alike. As a result, you can build your app once, test it in the Editor, and deploy it to the device all in one seamless workflow.

- **Growing Ecosystem**. NatML is designed with a singular focus on applications. As a result, there is a growing ecosystem of predictor packages for ML models and applications that run on NatML. [Check out NatML Hub](https://hub.natml.ai).

- **Augmented Reality**. NatML is particularly suited for augmented reality because it delegates work to ML accelerators, freeing up the GPU to render your app smoothly.

- **Lightweight Package**. NatML is distributed in a self-contained package, with no external dependencies. As a result, you can simply import the package and get going--no setup necessary.

## Installing NatML
Add the following items to your Unity project's `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "NatSuite Framework",
      "url": "https://registry.npmjs.com",
      "scopes": ["api.natsuite"]
    }
  ],
  "dependencies": {
    "api.natsuite.natml": "1.0.5"
  }
}
```

## Discover ML Models on Hub
**[Create an account on NatML Hub](https://hub.natml.ai/)** to find and download ML predictors to use in your project!

![NatML Hub](.media/hub.png)

You can also [upload your models to Hub](https://hub.natml.ai/predictor/create) and make them private or public. [Check out the online documentation](https://docs.natml.ai/advanced/authoring) for information on authoring predictors.

## Using ML Models in Three Simple Steps
You will always use NatML in three steps. First, create a **model** from model data:
```csharp
// Fetch model data from NatML Hub
var modelData = await MLModelData.FromHub("@author/some-model");
// Deserialize the model
var model = modelData.Deserialize();
```

Then create a **predictor** to make predictions with the model:
```csharp
// Create a predictor for the model
var predictor = new SomePredictor(model);
```

Finally, make predictions with the predictor:
```csharp
// Make prediction on an image
Texture2D input = ...;
var someOutput = predictor.Predict(input);
```

Different predictors accept and produce different data types, but the usage pattern will always be the same.

___

## Requirements
- Unity 2019.2+

## Supported Platforms
- Android API Level 24+
- iOS 13+
- macOS 10.15+ (Intel only)
- Windows 10+, 64-bit only

## Resources
- Join the [NatSuite community on Discord](https://discord.gg/y5vwgXkz2f).
- See the [NatML documentation](https://docs.natml.ai).
- See more [NatSuite projects on GitHub](https://github.com/natsuite).
- Read the [NatSuite blog](https://blog.natml.ai/).
- Discuss [NatML on Unity Forums](https://forum.unity.com/threads/open-beta-natml-machine-learning-runtime.1109339/).
- Contact us at [hi@natsuite.io](mailto:hi@natsuite.io).

Thank you very much!