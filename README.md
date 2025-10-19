# **Package Information**

The Sketch Renderer package provides a complete set of Tools and PostProcessing features specialized in reproducing hand-drawn and sketchy techniques from traditional art.
The package is available for the Univeral Rendering Pipeline (version 17.0.0+) in Unity 6.0 onwards.

By installing it, you gain access to the following features, fully integrated into your project editor:

## **Texture Generating Tools**
Systems for generating all the necessary stylized textured utilized by the package integrated into editor windows, with a wide range of customizations.
- Material (Paper) Texture Generator
  - Up to 5 different toggleable surface characteristics
- Tonal Art Map (Stroke Pattern) Texture Generator
  - Multiple stroke primitives for further customization

## **Renderer Features**
Dynamic Post-processing effects, independently toggleable.
- Material (Paper) Texture Projection
- Luminance (Tonal Art Map) Texture Projection
- Edge Detection with Color, Depth and Normals Buffers
- Smooth Outlines with accented settings for hand-drawn effects and thickness control
- Sketchy Outlines with multiple stroke generation along object boundaries.
- Compositor Feature for handling all sketch features and the final render.

## **Renderer Manager**
Provides an internal Sketch Renderer Manager which automatically handles post-processing dependencies and package initialization, including:
- Create and save SketchRendererContexts, which store templates of renderer feature configuration, and quickly swap between existing templates.
- Automatically manage internal dependencies and configure the active URP Renderer with the selected context's configuration.
- Integrated Texture Tool outputs with the active context, allowing for quick iteration of different visuals.

# **Installation**

The package can be installed in any Unity 6.0 project with a supported UniversalRenderingPipeline version imported.

To install the latest release, import the package into your Unity project with the package url:
```
https://github.com/LorenzoGrando/SketchRenderer.git
```

For more information on importing packages from Github, please refer to the [Unity Documentation](https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui-giturl.html)
