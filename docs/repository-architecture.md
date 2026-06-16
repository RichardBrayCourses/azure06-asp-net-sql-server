# Repository Architecture

This repository hosts a cloud-native Azure application built as a monorepo.

## Objectives

- Independently deployable React applications.
- Independently deployable ASP.NET Core services.
- Azure SQL for relational storage.
- Bicep for infrastructure.
- GitHub Actions for CI/CD.
- pnpm workspaces for frontend packages.
- No Module Federation.

## Repository Structure

```text
apps/
  shell/
  cases-web/          # future
  people-web/         # future
  admin-web/          # future
services/
  cases-api/
  people-api/         # future
  identity-api/       # future
  notifications-api/  # future
packages/
  ui/
  config/
  contracts/
infra/
  bicep/
scripts/
docs/
```

## Folder Responsibilities

`apps/` contains independently deployable React applications. Each application should build independently, deploy independently, own its routes, and consume backend APIs through HTTP.

`services/` contains independently deployable ASP.NET Core services. Each service should build independently, deploy independently, own its data, own its EF Core DbContext, and own its migrations.

`packages/` contains genuinely shared frontend code only. Allowed package areas are `ui`, `config`, and `contracts`. Avoid shared backend business logic.

## Database Rules

Each service owns its own schema.

```text
cases-api       -> cases schema
people-api      -> people schema
identity-api    -> identity schema
```

Do not create `SharedDbContext`, `ApplicationDbContext`, or `CommonDbContext`. Do not allow services to access each other's tables directly.

## EF Core Layout

EF Core belongs inside the service that owns the data.

```text
services/cases-api/
  Program.cs
  Data/
    AllChecksOutDbContext.cs
    Migrations/
  Entities/
```

The current `cases-api` service owns the Stage 1 EF Core scaffold, including entities, seed data, DbContext, and migrations.

## Azure SQL Strategy

Initially use one Azure SQL Server per environment and one Azure SQL Database per environment, with separate schemas per service.

```text
cases.*
people.*
identity.*
```

This keeps costs low while preserving service boundaries.

## Routing

Frontend target:

```text
/           -> shell
/cases/*    -> cases-web
/people/*   -> people-web
/admin/*    -> admin-web
```

Backend target:

```text
/api/cases/*          -> cases-api
/api/people/*         -> people-api
/api/identity/*       -> identity-api
/api/notifications/*  -> notifications-api
```

## Principles

1. Deployment boundaries are visible from the folder structure.
2. Each service owns its own data.
3. Each service owns its own EF Core migrations.
4. Frontends communicate through APIs, not shared state.
5. Shared code should be minimised.
6. Prefer simplicity over enterprise patterns.
7. Apply Clean Architecture inside a service only if needed.
8. Do not structure the entire repository as a single Clean Architecture application.
