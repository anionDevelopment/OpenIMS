# inventory-hierarchy Specification

## Purpose

The inventory of OpenIMS represents the physical world. Every inventory-object is the representative of a real, physical thing: a location (for example a company-site), a building, an area (a place where things are kept: a room of a building, or an area of a location which is not in a building, for example a yard or a terrace), a container (anything which contains other things, for example a cupboard, a shelf or a euro-box) or an item (a single inventoried thing, for example a drill). The inventory can therefore only describe situations which are possible in the real world. The rules are implemented in the business-logic of the backend (`BusinessLogicService`); the persistence only stores the result.

## Requirements

### Requirement: Inventory-object types

The system SHALL know exactly the inventory-object types `Location`, `Building`, `Area`, `Container` and `Item`. Every inventory-object SHALL have an `Id`, a `Name` and a `Description`. The name SHALL NOT be empty or consist of whitespace only.

#### Scenario: Create an inventory-object with a name

- **WHEN** an inventory-object is created with a non-empty name and a description
- **THEN** the object is stored with a new unique id, the given name and the given description

#### Scenario: Reject an empty name

- **WHEN** an inventory-object is created or updated with an empty name or a name which consists of whitespace only
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

### Requirement: Composite-structure

The business-types SHALL implement the composite-pattern: `Area` and `Container` SHALL be `IContainer`s, `Container` and `Item` SHALL be `IContainee`s. Every `IContainer` SHALL have a list of the `IContainee`s it directly contains. A `Location` SHALL have a list of its buildings and a list of its areas. A `Building` SHALL have a list of its areas. `Location` and `Building` SHALL be neither an `IContainer` nor an `IContainee`. Every inventory-object SHALL be an `IAuthorizableItem`.

#### Scenario: Nested containers

- **WHEN** a container is created in an area, a second container is created in the first container and an item is created in the second container
- **THEN** the area contains the first container, the first container contains the second container and the second container contains the item

### Requirement: Strict hierarchy

Every building SHALL be in exactly one location. Every area SHALL be in exactly one location or building. Every container and every item SHALL be in exactly one area or container. An item SHALL NOT contain anything. Ultimately every container and every item SHALL be in an area, every area SHALL be in a building or a location and every building SHALL be in a location.

#### Scenario: Building in a location

- **WHEN** a building is created in a location
- **THEN** the building is listed in the buildings of exactly that location

#### Scenario: Area in a building

- **WHEN** an area (for example a room) is created in a building
- **THEN** the area is listed in the areas of exactly that building

#### Scenario: Area directly in a location

- **WHEN** an area (for example a yard) is created in a location
- **THEN** the area is listed in the areas of exactly that location

#### Scenario: Reject a building outside of a location

- **WHEN** a building is created in or moved into an object which is not a location
- **THEN** the operation is rejected with the status-code 400

#### Scenario: Reject an area outside of a location or building

- **WHEN** an area is created in or moved into an object which is neither a location nor a building
- **THEN** the operation is rejected with the status-code 400

#### Scenario: Reject a containee outside of an area or container

- **WHEN** a container or an item is created in or moved into an object which is neither an area nor a container
- **THEN** the operation is rejected with the status-code 400

#### Scenario: Move a containee

- **WHEN** a container or an item is moved into another area or container
- **THEN** it is contained only in the target and not anymore in its previous area or container

#### Scenario: Move an area

- **WHEN** an area is moved into another location or building
- **THEN** it belongs only to the target and not anymore to its previous location or building

#### Scenario: Move a building

- **WHEN** a building is moved into another location
- **THEN** it belongs only to the target location and not anymore to its previous location

### Requirement: No cycles

A container SHALL NOT contain itself, neither directly nor indirectly.

#### Scenario: Reject moving a container into itself

- **WHEN** a container is moved into itself
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

#### Scenario: Reject moving a container into a container it contains

- **WHEN** a container is moved into a container which it contains directly or indirectly
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

### Requirement: Target place and real place of an item

The container (area or container) which contains an item SHALL be its target place, the place where the item belongs to. Every item SHALL additionally have a real place (`IRealPlace`), the place where it actually is. Every `IContainer` SHALL be an `IRealPlace`. The real place of an item SHALL be allowed to differ from its target place, also across locations.

#### Scenario: Item is where it belongs

- **WHEN** an item is created with the same area or container as target place and as real place
- **THEN** its target place and its real place are the same object

#### Scenario: Item is somewhere else

- **WHEN** the real place of an item is set to an area or container which is not its target place
- **THEN** the item stays in its target place and its real place is the given area or container

#### Scenario: Reject an invalid real place

- **WHEN** the real place of an item is set to an object which is not an `IRealPlace` (for example a location, a building or an item)
- **THEN** the operation is rejected with the status-code 400

### Requirement: Deletion

An inventory-object SHALL only be deletable when it contains nothing and when no item actually is there (it is not the real place of any item). Deleting an object SHALL NOT delete other inventory-objects. The groups assigned to a deleted object SHALL be removed together with it.

#### Scenario: Reject deleting an object which contains something

- **WHEN** a location with buildings or areas, a building with areas, or an area or container with containees is deleted
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

#### Scenario: Reject deleting the real place of an item

- **WHEN** an area or container which is the real place of at least one item is deleted
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

#### Scenario: Delete an empty and unused object

- **WHEN** an object which contains nothing and which is not the real place of any item is deleted
- **THEN** the object and the assignments of groups to it do not exist anymore

### Requirement: Unknown inventory-objects

An operation which references an inventory-object which does not exist SHALL be rejected with the status-code 404.

#### Scenario: Reference an unknown object

- **WHEN** an operation references an id for which no inventory-object exists
- **THEN** the operation is rejected with the status-code 404 and nothing is changed

### Requirement: Persistence of the inventory

Every supported persistence (PostgreSQL, MariaDB and the transient persistence) SHALL store the inventory with exactly one parent per inventory-object, so that an object can never be in two places at once. A location SHALL have no parent and only an item SHALL have a real place.

#### Scenario: Round-trip of the inventory

- **WHEN** inventory-objects are stored, updated and read back
- **THEN** every persistence returns the same type, name, description, parent and real place which were stored
