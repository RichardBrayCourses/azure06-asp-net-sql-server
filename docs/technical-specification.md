# All Checks Out - Technical Specification

## Target Azure Stack

| Area                   | Target technology                                                                              |
| ---------------------- | ---------------------------------------------------------------------------------------------- |
| Frontend               | React, TypeScript, Vite, Tailwind CSS, shadcn-style UI primitives                              |
| Backend                | C#, ASP.NET Core Web API                                                                       |
| API endpoints          | ASP.NET Core HTTP endpoints                                                                    |
| Database               | Azure SQL Database Serverless, Microsoft SQL Server                                            |
| ORM                    | Entity Framework Core                                                                          |
| Authentication         | Microsoft Entra External ID                                                                    |
| Authorization          | Application roles plus Azure RBAC where appropriate                                            |
| Object storage         | Azure Blob Storage                                                                             |
| Events                 | Azure Event Grid                                                                               |
| Queues                 | Azure Queue Storage                                                                            |
| Monitoring             | Application Insights, Azure Monitor                                                            |
| Infrastructure as code | Bicep                                                                                          |
| DNS and edge           | Azure DNS, Azure Managed Certificates, future Azure Front Door or Static Web Apps where needed |

## Frontend Tech

- Current app location: `apps/shell`
- Framework:
  - React 19
  - TypeScript 5.9
  - Vite 7
  - React Router 7
- Styling:
  - Tailwind CSS 4
  - shadcn-style component primitives
  - lucide-react icons
  - GOV.UK-influenced typography, spacing, status colors, and table-heavy console layout
  - Azure/AWS-style header with app launcher, global search, account menu, and utility icons
- Runtime state:
  - `AuthContext` stores demo login selection in `localStorage`.
  - `ThemeContext` stores light/dark preference in `localStorage`.
  - `DomainDataContext` exposes the in-memory database and refresh counter.
- Main UI routes:
  - `/`
  - `/admin`
  - `/admin/participants`
  - `/admin/participants/:participantId`
  - `/admin/stakeholders`
  - `/admin/stakeholders/:stakeholderId`
  - `/admin/case-templates`
  - `/admin/case-templates/:templateId`
  - `/admin/task-types`
  - `/admin/users`
  - `/cases`
  - `/cases/suppliers`
  - `/cases/users`
  - `/cases/:caseId`
  - `/cases/:caseId/tasks/:taskId`
  - `/stakeholder`
  - `/stakeholder/participants/:participantId`
  - `/stakeholder/:caseId`
