# Services

Independently deployable ASP.NET Core services live here.

Current:

- `cases-api` - the current Azure06 API and EF Core scaffold. It owns its current DbContext, entities, seed data, and migrations inside the service folder.

Future services such as `people-api`, `identity-api`, and `notifications-api` should be created only when their data ownership and deployment boundary are real.
