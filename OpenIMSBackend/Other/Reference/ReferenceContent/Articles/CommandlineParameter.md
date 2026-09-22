# Supported commandline-parameter

Currently the following commandline-parameter exist:

- `InitialAdminPassword`
- `InitialDatabaseType`
- `InitialDatabaseConnectionString`
- `InitialDomain`
- `InitialEnableEndpointAvailabilityCheckValue`
- `InitialEnableEndpointInitializationStateValue`
- `InitialEnableEndpointCurrentVersionValue`
- `InitialEnableEndpointShowAllEndpointsValue`
- `InitialEnableEndpointHealthCheckValue`
- `InitialEnableEndpointMetricsValue`

These arguments will be used when there is no persisted configuration-file and the configuration-file is generated.
This is typically the case on the first run when the OpenIMS-backend is started the first time in a new deployment-environment.
So these values can be used as "seed" to fill them directly into the correct fields in the configuration without the requirement to change the configuration later.
After the first run, when the configuration is generated, these commandline-parameter will not be used anymore.

The values can be changed later in the configuration-file.
When doing that then the configuration-change will be applied after the next restart of the OpenIMS-backend.

If `InitialAdminPassword` is not provided, then `admin` will be set as the password of the admin-user.
Because the admin-user is created exactly once (on the first start), changing this value afterwards has no effect.

If `InitialDatabaseType` is not provided, then `Transient` will be used as default.
