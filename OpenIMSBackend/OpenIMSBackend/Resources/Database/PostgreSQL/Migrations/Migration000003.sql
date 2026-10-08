
-- Since this version the hierarchy of the inventory contains areas (Type "Area"): location -> building -> area, location -> area, area -> container/item.
-- This does not change the table "InventoryObjects"; the rules of the hierarchy are business-rules which are checked by the backend.

-- The m:n-assignment of groups (roles) to authorizable items (currently: inventory-objects). The members of an assigned group have access to the item.
-- Only the direct assignments are stored. That the access applies to everything which is contained in the item as well is computed at runtime and is not stored.
CREATE TABLE "Role_AuthorizableItems" (
    "RoleId" varchar(255) not null,
    "AuthorizableItemId" varchar(255) not null,
    CONSTRAINT "PK_Role_AuthorizableItems" PRIMARY KEY ("RoleId", "AuthorizableItemId"),
    CONSTRAINT "FK_Role_AuthorizableItems_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "FK_Role_AuthorizableItems_AuthorizableItemId" FOREIGN KEY ("AuthorizableItemId") REFERENCES "InventoryObjects"("Id")
);

-- The primary-key covers the search by "RoleId", but the backend searches by "AuthorizableItemId".
CREATE INDEX "IX_Role_AuthorizableItems_AuthorizableItemId" ON "Role_AuthorizableItems"("AuthorizableItemId");
