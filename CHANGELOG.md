# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## (1.0.1) - 2025-03-18

### Changed
- GHRegisterComponentMembers got a MenuItem "Replace Existing" which is true by default. When true it replaces the member-geometry of a component; When false it adds to the existing member-geometries.

### Fixed
- Fixed [[Bug] Member-Geometry is not replaced](https://github.com/design-to-production/D2P-Components/issues/23)

## (1.0.0) - 2024-12-02

### Added

- GH_StreamComponentTypes is a new Grasshopper component to stream existing component-types
- GH_StreamComponentsByType has an additional MenuItem to reverse the result of the regex-pattern
- GH_CreateComponent has an additional (optional) input parameter to create a parent-child relationship for a new component
- ShowChangelog menu item for all GH Components opens the Changelog in the browser

### Changed

- GH_BakeComponents Name has changed from "Bake" to "BakeComponents"
- GH_StreamComponentsByType accepts now both, a type-id and the type itself as input parameter
- GH_CreateComponent accepts now both, a type-id and the type itself as input parameter

### Fixed
- GH_StreamComponentsByType now accepts real regex-patterns to filter component names

## (1.0.0-beta) - 2024-09-15

[unreleased]: https://github.com/design-to-production/D2P-Components/tree/v1.0.0

[1.0.0-beta]: https://github.com/design-to-production/D2P-Components/tree/c1305e7056bfe2cb6514c94fada562b278fbf244