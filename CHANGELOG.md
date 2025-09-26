# Changelog

All changes to the package features and functionality will be documented in this file.

This project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html) after the initial release in `1.0.0`.

## [Unreleased]

## [v0.5.6-alpha] - 2025-09-25

### Changed

- Hotfix so Package validates the default the target assets directory upon initialization.


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
[unreleased]: https://github.com/LorenzoGrando/SketchRenderer/compare/v0.5.6-alpha...HEAD
[v0.5.6-alpha]: https://github.com/LorenzoGrando/SketchRenderer/compare/v0.5.5-alpha...v0.5.6-alpha
[v0.5.5-alpha]: https://github.com/LorenzoGrando/SketchRenderer/releases/tag/v0.5.5-alpha