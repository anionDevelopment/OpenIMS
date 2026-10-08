# 12. Glossary

## Terms

| Term | Meaning |
| ---- | ------- |
| Access-token | The value which a client receives on a successful login and sends in the header `X-AccessToken` of every authenticated request. |
| Area | Represents a place where things are kept: a room of a building or an area of a location which is not in a building (for example a yard or a terrace). It contains containers and items. |
| Authorizable item | Something to which groups can be assigned, so that their members have read-access to it. Currently every inventory-object. The access is inherited by everything which is contained in it. |
| Building | Represents a physical building. It is in exactly one location and consists of areas (its rooms). |
| Containee | An object which can be contained in a container: a container or an item. |
| Container | Represents anything which contains other things (for example a cupboard, a shelf or a euro-box). It is in an area or in another container. |
| Group | A role, used to grant its members access to authorizable items. |
| Inventory-object | A location, a building, an area, a container or an item. Every inventory-object represents a physical thing. |
| Item | Represents a single inventoried thing (for example a drill). It is in an area or in a container and has a real place. |
| Location | Represents a site (for example a company-site). It contains buildings and areas and is the root of the inventory-hierarchy. |
| Real place | The place where an item actually is. Can differ from its target place. |
| Role | A named set of permissions. A role can inherit other roles, so a permission of an inherited role applies to the inheriting role as well. |
| Target place | The area or container which contains an item in the inventory-hierarchy: the place where the item belongs to. |
| Transient persistence | An in-memory-persistence which is included in the backend itself. It is used for testcases and for demonstration-purposes and loses all data when the backend is stopped. |

## Abbreviations

| Abbreviation | Meaning |
| ------------ | ------- |
| IMS | inventory-management-system |
