
CREATE TABLE "Users" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    "PasswordHash" varchar(255) null,-- when this property is null then this means that the user uses an external authenticationprovider to login.
    "EMailAddress" varchar(255) null,
    "UserIsActivated" boolean not null,
    "UserIsLocked" boolean not null,
    "RegistrationMoment" timestamp not null,
    "TOTPActivated" boolean null,
    "TOTPSecretKey" varchar(255) null,
    unique("Name"),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Roles" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
    unique("Name")
);

CREATE TABLE "User_Roles" (
    "UserId" varchar(255) not null,
    "RoleId" varchar(255) not null,
    CONSTRAINT "FK_User_Roles_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "FK_User_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_User_Roles" PRIMARY KEY ("UserId", "RoleId")
);

CREATE TABLE "Role_InheritedRoles" (
    "RoleId" varchar(255) not null,
    "InheritedRoleId" varchar(255) not null,
    CONSTRAINT "FK_Role_InheritedRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "FK_Role_InheritedRoles_InheritedRoleId" FOREIGN KEY ("InheritedRoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_Role_InheritedRoles" PRIMARY KEY ("RoleId", "InheritedRoleId")
);

CREATE TABLE "AccessToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_AccessToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_AccessToken_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

CREATE TABLE "RefreshToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_RefreshToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_RefreshToken_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

-- Settings which apply to the whole installation.
CREATE TABLE "Settings" (
    "Key" varchar(255) not null,
    "Value" text not null,
    CONSTRAINT "PK_Settings" PRIMARY KEY ("Key")
);

-- Settings which belong to a single user (in contrast to the table "Settings", whose values apply to the whole installation).
-- The table is a generic key-value-store so that a further user-specific setting does not need another migration.
-- Currently used key: "Theme" with the values "system", "light" and "dark".
CREATE TABLE "UserSettings" (
    "UserId" varchar(255) not null,
    "Key" varchar(255) not null,
    "Value" text not null,
    CONSTRAINT "PK_UserSettings" PRIMARY KEY ("UserId", "Key"),
    CONSTRAINT "FK_UserSettings_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);
