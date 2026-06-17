# All Checks Out Development Roadmap

## Current Repository

Azure06 deploys:

- React shell in `apps/shell`.
- Azure Functions API in `services/functions-api`.
- Azure SQL with EF Core migrations owned by the Functions API.
- Testing, staging, and production Azure environments.
- Microsoft Entra sign-in and bearer-token API authorization.

## Completed Foundations

- Static website hosting.
- Registered environment domains.
- Branch promotion through testing, staging, and production.
- GitHub Actions deployment.
- Entra app registration and MSAL shell integration.
- Azure SQL schema, seed data, migrations, and deployment scripts.
- Functions API deployment.

## Near-Term Work

- Add document storage in Azure Blob Storage.
- Add document metadata tables and API endpoints.
- Add upload, download, versioning, and delete workflows.
- Expand Functions tests around request routing and token validation.
- Add operational dashboards, alerts, and backup checks.

## Later Work

- Use Azure AI services for document verification.
- Review bounded contexts and service ownership boundaries.
- Split larger domains into independently deployable Functions APIs if scale or ownership requires it.
- Split the frontend into focused applications when independent delivery becomes valuable.
- Harden production operations with runbooks, restore drills, and security reviews.
