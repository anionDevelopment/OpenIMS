# 8. Crosscutting Concepts

## Performance

Currently there is no mechanism implemented for horizontal scaling.
So only one OpenIMS-container with one database-container is the way to go for now.
Running multiple OpenIMS-container using the same database is not supported currently.
Horizontal scaling may be implemented in future.

## Timestamps

Timestamps are stored in UTC in the persistence and are converted to the local timezone of the user by the application which displays them.
