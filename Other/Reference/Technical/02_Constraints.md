# 2. Constraints

## Technical constraints

| Constraint-identifier | Constraint | Reason |
| --------------------- | ---------- | ------ |
| TC0001 | Deployment on productive only as container | Effort for support and runtime-problems due to different environments should be minimized. Container-usage is state-of-the-art so there is no requirement to support bare-metal-installations. |
| TC0002 | Only relational databases are supported as persistence | The data of an inventory-management-system is relational and the schema is defined explicitly by migration-scripts, so a relational database is the appropriate kind of persistence. |

## Organizational constraints

| Constraint-identifier | Constraint | Reason |
| --------------------- | ---------- | ------ |
| OC0001 | Issues will be managed only via GitHub | There should be one single source of truth for issues. |
