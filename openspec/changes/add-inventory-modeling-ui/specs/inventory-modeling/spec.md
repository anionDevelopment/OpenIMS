# Spec Delta

## Purpose

Lets a user model the inventory (locations, buildings, areas, containers and items) through the web-api and the page "Inventory" of the web-ui, so that the physical world can be represented in OpenIMS. The rules of the hierarchy are the ones of `inventory-hierarchy`; this capability only defines how they are made available.

## ADDED Requirements

### Requirement: Modeling requires a logged-in user

Every operation of this capability SHALL require a logged-in user. Every logged-in user SHALL be able to use every operation of this capability; administrator-privileges SHALL NOT be required.

#### Scenario: Not logged in

- **WHEN** an operation of this capability is requested without a valid access-token
- **THEN** the request is rejected as not authenticated and nothing is changed

#### Scenario: Logged-in user without administrator-privileges

- **WHEN** a logged-in user who is not an administrator creates, changes or deletes an inventory-object or reads the inventory-tree
- **THEN** the operation is executed

### Requirement: Modeling is not restricted by groups

As long as it is not specified which inventory-operation requires which access, the operations of this capability SHALL NOT be restricted by the groups which are assigned to inventory-objects: every logged-in user SHALL see and change the complete inventory.

#### Scenario: Object without an assigned group of the user

- **WHEN** a logged-in user reads the inventory-tree and none of the groups of the user is assigned to a location
- **THEN** the location and everything in it are nevertheless contained in the tree

### Requirement: Read the inventory-tree

The backend SHALL return the complete inventory as a tree. Every node SHALL contain the id, the type, the name and the description of its inventory-object and its direct children: the buildings and areas of a location, the areas of a building and the containers and items of an area or container.

#### Scenario: Nested inventory

- **WHEN** a location with a building, an area in the building, a container in the area and an item in the container exists and the inventory-tree is read
- **THEN** the tree contains the location with the building as child, the building with the area as child, the area with the container as child and the container with the item as child, each with its id, type, name and description

#### Scenario: Empty inventory

- **WHEN** no location exists and the inventory-tree is read
- **THEN** the result is an empty list

### Requirement: Create inventory-objects

The backend SHALL create a location (without parent), a building in a location, an area in a location or building, a container in an area or container and an item in an area or container, each with the given name and description, and SHALL return the id of the created object. The rules of `inventory-hierarchy` SHALL apply unchanged.

#### Scenario: Create a building in a location

- **WHEN** a user creates a building with a name and a description in an existing location
- **THEN** the id of the new building is returned and the building is a child of that location in the inventory-tree

#### Scenario: Reject a parent of the wrong type

- **WHEN** a user creates a building in an area
- **THEN** the request is rejected with the status-code 400 and nothing is changed

#### Scenario: Reject an empty name

- **WHEN** a user creates an inventory-object with an empty name
- **THEN** the request is rejected with the status-code 400 and nothing is changed

### Requirement: Real place of a new item

An item created through this capability SHALL have its target place as its real place.

#### Scenario: Create an item

- **WHEN** a user creates an item in a container
- **THEN** the container is the target place and the real place of the item

### Requirement: Change name and description

The backend SHALL change the name and the description of any existing inventory-object. The type and the place of the object SHALL NOT change.

#### Scenario: Rename a container

- **WHEN** a user changes name and description of a container
- **THEN** the inventory-tree contains the container with the new name and description at the same place

### Requirement: Delete inventory-objects

The backend SHALL delete an inventory-object under the conditions of `inventory-hierarchy` and SHALL reject the deletion of an object which contains something or which is the real place of an item with the status-code 400.

#### Scenario: Delete an empty area

- **WHEN** a user deletes an area which contains nothing and where no item actually is
- **THEN** the area is not contained in the inventory-tree anymore

#### Scenario: Reject deleting a building with areas

- **WHEN** a user deletes a building which contains an area
- **THEN** the request is rejected with the status-code 400 and the building and its area still exist

### Requirement: Page "Inventory"

The user-area of the frontend SHALL have the menu-entry "Inventory", which opens a page that displays the inventory-tree. Every node SHALL display the name of its object and SHALL be expandable when it has children.

#### Scenario: Open the page

- **WHEN** a logged-in user selects the menu-entry "Inventory"
- **THEN** the page displays all locations, and the content of a location is displayed after it was expanded

### Requirement: Actions on the page "Inventory"

The page SHALL offer an action to create a location, and on every node exactly the creations which the hierarchy allows for its type (location: building and area; building: area; area and container: container and item; item: none), plus editing and deleting. Creating and editing SHALL ask for name and description; deleting SHALL ask for a confirmation. After a successful action the tree SHALL be reloaded and SHALL keep the expanded nodes expanded.

#### Scenario: Add a room to a building

- **WHEN** a user selects the action to add an area on a building and enters a name
- **THEN** the area is created in that building and is displayed as child of the building

#### Scenario: No creation on an item

- **WHEN** the page displays an item
- **THEN** the item offers editing and deleting but no creation

#### Scenario: Rejected action

- **WHEN** an action is rejected by the backend (for example deleting a building which contains areas)
- **THEN** the page displays that the action could not be executed and the tree is unchanged
