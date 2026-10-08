# inventory-object-selection Specification

## Purpose

Several pages of OpenIMS need to select an inventory-object (a location, a building, an area, a container or an item). Because the inventory can be large, the objects are not loaded completely in advance: a reusable dropdown with an inline-search loads only the objects which match the entered text, searched directly in the database.

## Requirements

### Requirement: Search of inventory-objects in the backend

The backend SHALL search the inventory-objects whose name contains a given text, ignoring the case. The search SHALL be executed in SQL in the database (and equivalently in the transient persistence). The text SHALL be taken literally: the wildcard-characters of `like` SHALL be escaped. An empty text SHALL match every name. The search SHALL be restricted to the requested types, of which at least one SHALL be given.

#### Scenario: Search ignoring the case

- **WHEN** the text "BOX" is searched for containers and containers named "Big box" and "small box" exist
- **THEN** both containers are returned with their id and their name as label

#### Scenario: Wildcards are taken literally

- **WHEN** the text "_100%" is searched
- **THEN** only objects whose name really contains "_100%" are returned

#### Scenario: Restrict the types

- **WHEN** only items are searched
- **THEN** no location, building, area or container is returned, even if its name contains the text

#### Scenario: Reject an invalid request

- **WHEN** no type or an unknown type is requested
- **THEN** the search is rejected with the status-code 400

### Requirement: Result of the search

The result of the search SHALL be ordered by name, SHALL be limited to 20 objects and SHALL consist of pairs of the id and a label of each found object. The label SHALL be the name of the object. As long as the result is not restricted to the objects the requesting user has access to, only an administrator SHALL be able to search.

#### Scenario: Limit the result

- **WHEN** more than 20 objects match
- **THEN** only the first 20 objects ordered by name are returned

#### Scenario: Reject a non-administrator

- **WHEN** a user who is not an administrator searches
- **THEN** the search is rejected as not authorized

### Requirement: Reusable selection-component in the frontend

The frontend SHALL provide one reusable component (`app-inventory-object-selector` of the `SharedComponentsModule`) which selects an inventory-object. It SHALL be a dropdown with an inline-search which displays the labels of the objects which the search of the backend returns. The types which can be selected SHALL be configurable.

#### Scenario: Configured types

- **WHEN** the component is used with the types "Area" and "Container"
- **THEN** its searches only return areas and containers

### Requirement: Behavior of the selection-component

The component SHALL send the entered text to the search of the backend when the user stops typing for a moment, and not for every keystroke. When the user selects an object, the component SHALL emit its id and label and SHALL clear the input, so that the next object can be searched.

#### Scenario: Search while typing

- **WHEN** the user types a text and stops typing for a moment
- **THEN** exactly one search with the complete text and the configured types is sent to the backend and its results are offered in the dropdown

#### Scenario: No search for every keystroke

- **WHEN** the user types several characters quickly one after the other
- **THEN** no search is sent for the intermediate texts

#### Scenario: Select an object

- **WHEN** the user selects an offered object
- **THEN** the component emits the id and the label of the object and the input is empty again
