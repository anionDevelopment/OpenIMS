# inventory-access Specification

## Purpose

Access to the inventory is granted to groups. A group is a role of the GRYLibrary (`GRYLibrary.Core.APIServer.CommonDBTypes.Role`), the same type which decides about the permissions of a user (for example `Users` and `Administrators`). Groups are assigned to authorizable items (`IAuthorizableItem`); currently every inventory-object (location, building, area, container, item) is an authorizable item. The access which an assigned group grants is read-access: its members can see the object and what is in it (which items are where). Because the inventory represents the physical world, who has access to a place has access to everything which is in that place as well. The rules are implemented in the business-logic of the backend (`BusinessLogicService`).

## Requirements

### Requirement: Creation of groups

An administrator SHALL be able to create a group with a name which is not empty, does not start or end with whitespace and is not used by another group yet. Only an administrator SHALL be able to create a group.

#### Scenario: Create a group

- **WHEN** an administrator creates a group with a valid and unused name
- **THEN** the group exists, has no members and is assigned to no authorizable item

#### Scenario: Reject an invalid or used name

- **WHEN** a group is created with an empty name, a name which starts or ends with whitespace, or the name of an existing group
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

### Requirement: Deletion of groups

An administrator SHALL be able to delete a group; deleting a group SHALL remove its memberships and its assignments to authorizable items as well. The groups `Users` and `Administrators`, which OpenIMS needs itself, and a group which another group inherits SHALL NOT be deletable. Only an administrator SHALL be able to delete a group.

#### Scenario: Delete a group

- **WHEN** an administrator deletes a group which has members and is assigned to authorizable items
- **THEN** the group, its memberships and its assignments do not exist anymore, and its former members lose the access which they had only through this group

#### Scenario: Reject deleting a required group

- **WHEN** the group `Users` or `Administrators` is deleted
- **THEN** the operation is rejected with the status-code 400 and nothing is changed

### Requirement: User-interface for groups

The admin-area of the frontend SHALL have the menu-entry "Users", where an administrator decides for every user which groups the user is member of, and the menu-entry "Groups", where an administrator sees the list of all groups, creates and deletes groups and, for a selected group, sees the directly assigned inventory-objects, removes an assignment and adds an inventory-object which is selected with the inventory-object-selection.

#### Scenario: Assign an object to a group

- **WHEN** an administrator selects a group on the page "Groups" and selects an inventory-object in the search-dropdown
- **THEN** the object is assigned to the group and is listed in the objects of the group

#### Scenario: Remove an object from a group

- **WHEN** an administrator removes an object from the list of the objects of the selected group
- **THEN** the assignment does not exist anymore and the object is not listed anymore

### Requirement: Assignment of groups to authorizable items

Every group SHALL be assignable to any number of authorizable items and every authorizable item SHALL be able to have any number of groups assigned (m:n). The persistence SHALL store an assignment as nothing but the id of the group and the id of the authorizable item. Only an administrator SHALL be able to assign a group to an authorizable item, to remove an assignment or to read the assignments of an authorizable item.

#### Scenario: Assign a group

- **WHEN** an administrator assigns a group to an authorizable item
- **THEN** the group is listed in the groups which are directly assigned to that item

#### Scenario: Assign a group twice

- **WHEN** an administrator assigns a group which is already assigned to the same authorizable item
- **THEN** nothing changes and the group is assigned exactly once

#### Scenario: Remove a group

- **WHEN** an administrator removes the assignment of a group from an authorizable item
- **THEN** the group is not listed anymore in the groups of that item and its members lose the access which they had only because of this assignment

#### Scenario: Reject a non-administrator

- **WHEN** a user who is not an administrator assigns a group, removes an assignment or reads the assignments of an authorizable item
- **THEN** the operation is rejected as not authorized and nothing is changed

#### Scenario: Reject an unknown group or item

- **WHEN** a group which does not exist or an authorizable item which does not exist is referenced
- **THEN** the operation is rejected with the status-code 404 and nothing is changed

### Requirement: Inheritance of the access

A user SHALL have access to an authorizable item if at least one of the groups of the user is assigned to the item itself or to an object which contains the item, directly or indirectly. This applies to locations, buildings, areas and containers: access to one of them SHALL grant access to everything which is in it. The access SHALL NOT be inherited upwards: access to an object SHALL NOT grant access to the object which contains it.

#### Scenario: Access to a building

- **WHEN** a group is assigned to a building
- **THEN** its members have access to the building, to all areas of the building, to all containers in these areas (to any depth) and to all items in them

#### Scenario: Access to a location

- **WHEN** a group is assigned to a location
- **THEN** its members have access to the location and to everything which is in it, directly or indirectly

#### Scenario: No access upwards

- **WHEN** a group is assigned to a building
- **THEN** its members do not have access to the location of the building because of this assignment

#### Scenario: No access without an assigned group

- **WHEN** none of the groups of a user is assigned to an object or to an object which contains it
- **THEN** the user does not have access to the object

### Requirement: Relevant groups and places

The groups of a user SHALL include the groups which they inherit (for example the group `Administrators` inherits the group `Users`). The real place of an item SHALL NOT influence the access; only the containment-hierarchy SHALL.

#### Scenario: Real place does not grant access

- **WHEN** a group is assigned to an area and an item of another area actually is in that area
- **THEN** the members of the group do not have access to that item because of this assignment

#### Scenario: Access of an inherited group

- **WHEN** a group is assigned to an object and a user is member of a group which inherits that group
- **THEN** the user has access to the object

### Requirement: The inherited access is computed at runtime

Only the direct assignments of groups to authorizable items SHALL be stored. The access which is inherited from a containing object SHALL NOT be stored but SHALL be computed at runtime from the current hierarchy. Therefore the inherited access SHALL always follow the current hierarchy, also after an object was moved.

#### Scenario: Inherited access is not stored

- **WHEN** a group is assigned to an area which contains an item
- **THEN** the group is listed in the directly assigned groups of the area but not in the directly assigned groups of the item, and the members of the group nevertheless have access to the item

#### Scenario: Access follows a move

- **WHEN** a container is moved out of an area to whose group a user has access only through that area
- **THEN** the user does not have access to the container and to its content anymore

### Requirement: Deletion of an authorizable item

When an authorizable item is deleted, the assignments of groups to it SHALL be deleted as well. The assignments of other authorizable items SHALL NOT be changed.

#### Scenario: Delete an item with assigned groups

- **WHEN** an inventory-object to which groups are assigned is deleted
- **THEN** the object and its assignments do not exist anymore and the assignments of all other objects are unchanged
