# Database

This directory is the home for database ownership notes and future external seed fixtures.

EF Core migrations live inside the service that owns the data. The current migrations are in `services/cases-api/Data/Migrations`.

`database/seed/` is intentionally a placeholder for later external seed fixtures. Current Stage 1 seed data lives in `services/cases-api/Data/AllChecksOutSeedData.cs`.
