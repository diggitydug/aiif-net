# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project follows Semantic Versioning where practical.

## [Unreleased]

## [0.1.4] - 2026-03-23

### Added
- Added configurable AIIF endpoint description overrides through AiifOptions.EndpointDescriptions:
  - Document
  - Summary
  - EndpointDetail
  - Auth
- Added built-in default descriptions for AIIF endpoints:
  - GET /ai-docs
  - GET /ai-docs/summary
  - GET /ai-docs/{endpoint}
  - GET /ai-docs/auth
- Added tests covering default and overridden AIIF endpoint descriptions.
- Added path-based endpoint document resolution tests in addition to name-based resolution tests.
- Added `AutoMapAiifFromSwagger(OpenApiInfo)` extension to populate AIIF `ApiName` and `ApiDescription` from Swagger metadata.

### Changed
- AIIF endpoint mappings now set explicit endpoint descriptions instead of falling back to generic endpoint-name text.
- AIIF endpoint-detail lookup supports matching by endpoint name or normalized route path.
- Changed `AiifOptions.ApiName` and `AiifOptions.ApiDescription` to optional values with runtime defaults.
- Changed `AiifOptions.BaseDocsPath` to automatic/internal configuration (defaults to `/ai-docs`).
- Updated README with Swagger/AIIF shared metadata setup guidance to reduce duplicate configuration.

## [0.1.3-local] - 2026-03-22

### Added
- Added Swashbuckle.AspNetCore.Annotations dependency for richer endpoint metadata support.
- Added AIIF endpoint auth policy options with per-route toggles via AiifOptions.EndpointAuth.
- Added AIIF endpoint access policy helper and related tests.

### Changed
- Updated endpoint docs route to catch-all form for nested paths: /ai-docs/{**endpoint}.
- Improved endpoint description extraction from endpoint metadata in AIIF document generation.

## [0.1.2-local] - 2026-03-22

### Added
- Added local package build script at scripts/build-local-package.sh.
- Added local package feed integration guidance and setup notes in README.

### Changed
- Improved AIIF endpoint document resolution for path-style endpoint identifiers.

## [0.1.0] - Initial

### Added
- Initial Aiif.Net library release with AIIF endpoint mapping and document generation.
