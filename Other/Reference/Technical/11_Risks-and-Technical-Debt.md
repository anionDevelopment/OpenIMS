# 11. Risks and Technical Debt

## Risks

### Unauthenticated endpoints

You probably want to protect certain endpoints which are unauthenticated by default.
The design-concept behind is that you protect this endpoints by your reverse-proxy using basic-auth.

Endpoints you maybe want to protect are:

- `/API/Other/Maintenance/Metrics`: The metrics-endpoint is by design available without authentication. This is unauthenticated because only human user will be authenticated, but the metrics-scraper is typically a technical user.
- `/API/Other/Maintenance/CurrentVersion`: Querying the current version of the application should not be considered as weakness, but if you want to harden your server, you can disable this endpoint anyway.

It is possible to enable/disable the maintenance-endpoints using commandline-switch on initial-configuration-generation or later in the configuration-file.

### Initial admin-password

If `InitialAdminPassword` is not passed on the first start then the password of the admin-user is `admin`.
Change it (or pass a real password on the first start) before the installation is reachable from a network you do not control.

## Technical debts

### Reading the inventory loads the whole inventory

`IBusinessLogicService.GetLocation` and `GetAllLocations` load all inventory-objects and build the complete tree, because the real place of an item can be in another location.
This is simple and correct, but the effort grows with the size of the whole inventory and not with the size of the requested part.
If the inventory becomes large, reading has to be restricted (for example by loading only a subtree and resolving the real places separately).

### Computing the access walks up the hierarchy

`IBusinessLogicService.UserHasAccess` reads the assigned groups of every object from the requested object up to its location, one query per level.
This is fast for the usual depth of the hierarchy, but checking the access for many objects at once (for example for a list) should be done with one query instead.
