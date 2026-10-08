
-- The inventory: locations, buildings, containers and items, one row per object.
-- The hierarchy is stored by the reference to the parent ("ParentId"): the location of a building, the building or container of a container or an item. Only a location has no parent.
-- Because every row has exactly one parent, an object can never be contained in two places at once.
-- The remaining rules of the hierarchy (what may contain what, no cycles) are business-rules which are checked by the backend.
-- "RealPlaceId" is the place where an item actually is (in contrast to its parent, which is the place where it belongs to). It is only set for items.
CREATE TABLE "InventoryObjects" (
    "Id" varchar(255) not null,
    "Type" varchar(255) not null,
    "Name" varchar(255) not null,
    "Description" text not null,
    "ParentId" varchar(255) null,
    "RealPlaceId" varchar(255) null,
    CONSTRAINT "PK_InventoryObjects" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InventoryObjects_ParentId" FOREIGN KEY ("ParentId") REFERENCES "InventoryObjects"("Id"),
    CONSTRAINT "FK_InventoryObjects_RealPlaceId" FOREIGN KEY ("RealPlaceId") REFERENCES "InventoryObjects"("Id")
);

-- PostgreSQL does not create an index for a foreign-key by itself (MariaDB does), but both columns are searched by the backend.
CREATE INDEX "IX_InventoryObjects_ParentId" ON "InventoryObjects"("ParentId");
CREATE INDEX "IX_InventoryObjects_RealPlaceId" ON "InventoryObjects"("RealPlaceId");
