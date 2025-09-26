# Changelog

All changes to the package features and functionality will be documented in this file.

This project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html) after the initial release in `1.0.0`.
## [v0.5.5-alpha] - 2025-09-25

### Added
- Alpha Base Sketch Renderer Features and Texture Tools (#22), including:
    - **Texture Tools**
        - Tonal Art Map Texture Generator
        - Material (Paper Surface) Texture Generator
    - **Renderer Features**
        - Material Texture Projection
        - Luminance (Tonal Art Map) Texture Projection
        - Edge Detection with Color, Depth and Normals Buffers
        - Smooth Outlines with accented settings for hand-drawn effects
        - Sketchy Outlines with stroke SDF sampling
        - Compositor Feature for handling all sketch features
- Renderer Manager and Project Settings for dynamically handling feature dependencies and URP renderer data.

---

[v0.5.5-alpha]: https://github.com/LorenzoGrando/SketchRenderer/releases/tag/v0.5.5-alpha