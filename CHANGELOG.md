# Changelog

All changes to the package features and functionality will be documented in this file.

This project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html) after the initial release in `1.0.0`.

## [Unreleased]

## [v0.7.0-beta] - 2025-10-14

### Changed

- Tweaked Normals Outline to improve consistency and reduce the impact of small changes in parameters.
- Fixed composition of geometry and color source outline causing one of the two to lose stroke directions.
- Fixed composition causing additional lines in Accented Outlines to disappear when using the Material pass.
- Fixed SketchRendererManagerSettings not working until the Project Settings window was open.

### Added
- Sketchy Outlines can now optionally combine similar strokes along the detected outline, creating longer strokes if possible.
- Introduced some Quality of Life integration between the active SketchRendererContext and the Texture Tools to make iterating faster.

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
[unreleased]: https://github.com/LorenzoGrando/SketchRenderer/compare/v0.7.0-beta...HEAD
[v0.7.0-belta]: https://github.com/LorenzoGrando/SketchRenderer/compare/v0.5.6-alpha...v0.7.0-beta
[v0.5.6-alpha]: https://github.com/LorenzoGrando/SketchRenderer/compare/v0.5.5-alpha...v0.5.6-alpha
[v0.5.5-alpha]: https://github.com/LorenzoGrando/SketchRenderer/releases/tag/v0.5.5-alpha