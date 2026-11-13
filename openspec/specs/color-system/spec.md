# Color-System Specification

## Purpose

Records how the colors of a product with a user-interface are defined and applied, independent of the technology the user-interface is built with.
They are not derivable from the source-code itself:

- They state that the colors have one single source of truth - the color-palette - so that the appearance of the product can be changed in one place and a component can never disagree with the rest of the user-interface about what a color is.
- They state that the product has a light-mode and a dark-mode with one palette each, so that both modes are defined deliberately instead of one of them being an afterthought that drifts out of sync.
- They state that the design-system is Material 3 and that the palettes are applied through it, so that every component and every page follows the palette automatically instead of each one being styled by hand.

## Requirements

### Requirement: The colors of the user-interface have a single source of truth

All colors of the user-interface SHALL be defined in the color-palette and nowhere else.
The palette SHALL be the single source of truth for color: no component, page, widget, stylesheet or template SHALL define a color of its own.
Changing the appearance of the product SHALL be possible by changing the palette alone.

#### Scenario: A component needs a color

- **WHEN** a component, page or widget needs a color for text, a background, an accent, a border or any other purpose
- **THEN** it reads that color from the palette through the theme instead of naming a color-value of its own

#### Scenario: A color-value is defined outside the palette

- **WHEN** a color-value for example a hex-literal or a named framework-color is defined in a component, page, widget, stylesheet or template instead of in the palette
- **THEN** this is a defect, which is resolved by moving the color into the palette and reading it from there

#### Scenario: The appearance of the product is changed

- **WHEN** the colors of the product are to be changed
- **THEN** only the palette is changed, and the change takes effect everywhere in the user-interface without any component having to be touched

### Requirement: The product has a light-mode and a dark-mode, each with its own color-palette

The user-interface SHALL offer both a light-mode and a dark-mode.
There SHALL be exactly two color-palettes: one for light-mode and one for dark-mode.
Both SHALL be defined together as the single source of truth described above, so that each theme-mode has its own deliberately chosen colors.

#### Scenario: A theme-mode is missing

- **WHEN** the user-interface offers only a light-mode or only a dark-mode
- **THEN** this is a defect, resolved by adding the missing theme-mode together with its palette

#### Scenario: The product is themed

- **WHEN** the colors of the product are defined
- **THEN** there is one palette for light-mode and one palette for dark-mode, and each theme-mode takes its colors from its own palette

#### Scenario: A theme-mode is missing a palette

- **WHEN** only one of the two theme-modes has a palette, or the colors of a theme-mode are scattered instead of being collected in its palette
- **THEN** this is a defect, which is resolved by defining the missing palette as part of the single source of truth

### Requirement: The design-system is Material 3 and the palettes are applied through it

The user-interface SHALL use Material Design in its version 3 Material 3 as its design-system, and the light- and the dark-palette SHALL be applied to the product through the Material theme.
Every Material component and every page SHALL take its colors from the applied palette automatically, so that the palette reaches the whole user-interface without a component having to apply colors on its own.

#### Scenario: The theme of the product is built

- **WHEN** the theme of the product is built
- **THEN** it is a Material 3 theme built from the two palettes, so that selecting light-mode or dark-mode selects the corresponding palette

#### Scenario: A new component is added to the user-interface

- **WHEN** a new component or page is added to the user-interface
- **THEN** it follows the applied palette through the Material theme automatically and does not define colors of its own


<!-- This file is generated. Do not modify. -->