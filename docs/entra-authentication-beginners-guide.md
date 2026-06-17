# Authentication Beginner's Guide

All Checks Out uses Microsoft Entra for sign-in and a C# Azure Functions API for authenticated data access.

## The Short Version

1. The browser starts sign-in with MSAL.
2. Microsoft Entra redirects back to `/auth/callback`.
3. MSAL stores its account and token cache in browser storage.
4. The shell requests an API access token for `VITE_ENTRA_API_SCOPE`.
5. API calls include `Authorization: Bearer <access-token>`.
6. The Functions API validates the token.
7. The API maps the Entra `oid` claim to `UserAccounts.EntraObjectId`.
8. Domain permissions come from SQL memberships and grants.

## Frontend Pieces

```text
apps/shell/src/lib/entra/auth.ts
apps/shell/src/lib/entra/config.ts
apps/shell/src/lib/api/client.ts
```

The shell receives these values from Azure App Configuration:

```text
VITE_ENTRA_CLIENT_ID
VITE_ENTRA_AUTHORITY
VITE_ENTRA_API_SCOPE
VITE_API_BASE_URL
```

## API Pieces

```text
services/functions-api/Authentication/BearerTokenValidator.cs
services/functions-api/CurrentUser/ApplicationUserResolver.cs
services/functions-api/Domain/CasesDomainService.cs
```

`BearerTokenValidator` checks issuer, audience, lifetime, and signing keys. `ApplicationUserResolver` turns the trusted Entra identity into the application user profile used by the domain layer.

## Tokens

The frontend uses two token concepts:

- ID token: lets the frontend understand who signed in.
- Access token: authorizes calls to the Functions API.

The API expects only the access token in the bearer header.

## Demo Sign-In Options

The pre-authentication demo endpoint is:

```text
GET /api/demo/sign-in-options
```

It requires the `X-Demo-SignIn-Key` header. This key is configured per environment and is also written to the shell `.env` as `VITE_DEMO_SIGN_IN_KEY`.

## Authorization

Authentication proves the caller is a valid Entra user. Authorization decides what that application user may do.

The important mapping is:

```text
Entra oid -> UserAccounts.EntraObjectId -> UserAccounts.Id
```

`CasesDomainService` then checks authority, participant, stakeholder, agent, system-owner, and access-grant records before returning or changing data.
