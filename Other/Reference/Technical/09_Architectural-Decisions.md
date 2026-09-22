# 9. Architectural Decisions

## Decision-board

| Decision-identifier | Date | Decision | Reason and notes |
| ------------------- | ---- | -------- | ---------------- |
| D001 | 2026-09-21 | OpenIMS should be hostable on premises. | Base requirement for many user. |
| D002 | 2026-09-21 | C# for backend | Appropriate for usecase. |
| D003 | 2026-09-21 | Angular for web-frontend | Appropriate for usecase. |
| D004 | 2026-09-21 | Support for MariaDB as database | Reasons:<br>- Open source.<br>- Free for private user. |
| D005 | 2026-09-21 | Support for PostgreSQL as database | Reasons:<br>- MariaDB is not stable enough on all systems.<br>- MariaDB does still not support transactional DDL-operations, which makes a failed migration hard to recover from. |
| D006 | 2026-09-21 | No dedicated codeunit for an app-client | The web-frontend-client is supposed to be usable also on mobile devices. If there are more requirements which justify the effort then an app-client can still be developed. |
| D007 | 2026-09-21 | Timestamps will be stored in UTC in the persistence | Storing timezones in the database is tricky. To have a consistent way of storing timestamp-values correctly, timestamps are converted to UTC and the application which displays the value converts it back to the local timezone of the user. |
| D008 | 2026-09-21 | Authentication by username/password with an access-token | The simplest mechanism which covers the requirement. An external identity-provider can be added later without changing the endpoints which the frontend uses, because the frontend only ever sees the access-token. |
| D009 | 2026-09-21 | The database-schema is defined by explicit migration-scripts and not by an object-relational mapper | The schema stays readable and reviewable, and the difference between two supported database-types stays a difference between two folders of scripts instead of a difference in the business-logic. |
