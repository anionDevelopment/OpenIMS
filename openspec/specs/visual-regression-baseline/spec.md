# Visual-Regression-Baseline Specification

## Purpose

Records how the baseline-images of a product's visual-regression-tests are named, independent of the technology the user-interface and the tests are built with.
This specification is product-independent and meant to be reused.
A product MAY add further, product-specific naming-rules in its own specification.

## Requirements

### Requirement: A baseline-image's filename contains only dash as word-separator

The filename of every visual-regression-baseline-image SHALL use only dash (`-`) as word-separator, for example `settings-page` or `login-form`.
Other separators like underscore, space or dot (except the dot before the file-extension) SHALL NOT be used.

#### Scenario: A baseline-image's filename uses another separator

- **WHEN** a baseline-image's filename contains a word-separator other than dash
- **THEN** this is a defect, resolved by renaming the baseline-image and the name the test refers to it by so that only dash is used as word-separator


<!-- This file is generated. Do not modify. -->