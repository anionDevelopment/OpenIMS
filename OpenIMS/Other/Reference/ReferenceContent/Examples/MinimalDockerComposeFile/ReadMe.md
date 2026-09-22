# Minimal docker-compose-file

## Start

Run `task BaseExampleStart` (or shorter: `task beu`) from the repository-base-folder.

Optionally you can create a `Variables.env` in this folder to add some configuration-values.

Example-content for a valid `Variables.env`-file:

```bash
InitialAdminPassword=Adm1npa55w0rd
InitialDatabaseType=PostgreSQL
InitialDatabaseConnectionString=Host=openims_database;Port=5432;Database=OpenIMSDatabase;Username=root;Password=R00tpa55w0rd;IncludeErrorDetail=true;
```

## Access

### Application

Open `https://localhost:443` and log in with the user `admin` and the password which was set as `InitialAdminPassword`.

### Database

connectionstring: `Host=openims_database;Port=5432;Database=OpenIMSDatabase;Username=root;Password=R00tpa55w0rd;IncludeErrorDetail=true;`

Data for adminer:

System: `PostgreSQL`

Server: `openims_database`

Username: `root`

Password: `R00tpa55w0rd`

Database: `OpenIMSDatabase`
