# Azure06 Authentication Decisions

This note records the authentication and API protection decisions for future Azure06 implementation work.

## Source Comparison

The AWS reference project studied was:

`/Users/richardbray/src/aws10-rekognition-tagging/monorepo`

The relevant AWS pattern is:

- The frontend uses Cognito hosted UI with OAuth authorization code flow and PKCE.
- The callback exchanges the authorization code for tokens.
- The frontend stores both tokens in local storage:
  - `cognito_access_token`
  - `cognito_id_token`
- The frontend API client reads the stored ID token and sends it in the `Authorization` header.
- API Gateway protects `/auth/*` routes with a Cognito user pool authorizer.
- The backend reads validated claims from the API Gateway authorizer context.
- Application authorization then uses token claims such as `sub`, `email`, and `cognito:groups`.

## Current Azure06 State

Azure06 already contains Microsoft Entra sign-in code in:

`apps/ui/src/lib/entra/auth.ts`

The app uses MSAL:

```ts
cache: {
  cacheLocation: "localStorage",
}
```

This means MSAL stores its authentication cache in browser local storage after sign-in.

The app also stores its selected application context separately under the `user` local storage key. That value is not an Entra ID token. It is the application-level context selected by the user, such as authority, participant, stakeholder, or agent.

Azure06 does not currently have an ASP.NET Core backend, API client layer, protected endpoints, or token-bearing fetch calls.

## Decision

Azure06 should mirror the AWS architecture, but not copy the AWS token handling exactly.

Use MSAL-managed tokens rather than manually storing custom `entra_id_token` or `entra_access_token` keys.

For API calls:

- Acquire an API access token with MSAL.
- Send the access token using the standard bearer format:

```ts
headers: {
  Authorization: `Bearer ${accessToken}`,
}
```

For backend protection:

- Add ASP.NET Core JWT bearer authentication.
- Validate Microsoft Entra access tokens.
- Protect API endpoints with `[Authorize]` or endpoint-level authorization policies.
- Read validated claims from `HttpContext.User`.

Use Entra token claims for identity, then use Azure SQL for application authorization.

The API should map the Entra user claim, preferably `oid`, falling back only where appropriate, to `UserAccount.entraObjectId`. From there it should resolve:

- authority memberships
- participant memberships
- stakeholder memberships
- agent memberships
- application roles
- access grants
- tenant/data scope

## ID Token Versus Access Token

Do not use the Entra ID token as the API bearer credential.

The ID token proves sign-in to the frontend client. The access token is intended for the backend API.

This differs slightly from the AWS reference, where the Cognito ID token is sent to API Gateway because the Cognito user pool authorizer accepts that pattern.

The Azure06 implementation should use this flow instead:

```text
React + MSAL
  -> loginRedirect
  -> MSAL stores its cache in localStorage
  -> acquireTokenSilent(api scope)
  -> fetch(... Authorization: Bearer <access token>)

ASP.NET Core API
  -> validates token with JwtBearer / Microsoft Identity Web
  -> exposes claims via HttpContext.User
  -> maps Entra identity to UserAccount in SQL
  -> enforces application authorization from SQL data
```

## Frontend Implementation Guidance

Keep `getEntraAccessToken()` as the single way to obtain API tokens from the UI.

Add a small API client wrapper that:

- gets the access token via `getEntraAccessToken()`
- adds `Authorization: Bearer <token>` for protected calls
- handles 401 and 403 consistently
- centralizes API base URL configuration

Do not read MSAL internal local storage keys directly. Treat MSAL local storage as an implementation detail.

## Backend Implementation Guidance

When the ASP.NET Core API is added:

- Configure JWT bearer authentication for the Entra tenant and API audience.
- Configure CORS for the Vite dev server and deployed frontend domains.
- Add a `/api/me` endpoint that returns the authenticated Entra identity and resolved application user context.
- Protect all domain command/query endpoints by default.
- Allow only explicit public endpoints, such as health checks.
- Resolve application authorization from SQL, not from client-supplied local storage state.

The client-selected account context is useful for UI routing and user experience, but the backend must verify that the authenticated Entra user is allowed to use that context before returning or mutating data.

## Scope Configuration Note

The current Entra config chooses either an API scope or `openid profile email`.

When the API is added, prefer keeping identity scopes explicit during sign-in and using the configured API scope for access-token acquisition. The exact MSAL request shape can be finalized during implementation, but the backend API should expect a proper access token for its audience.
