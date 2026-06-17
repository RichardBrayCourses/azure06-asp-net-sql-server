# Authentication in Azure06: Beginner's Guide

This guide explains how authentication works in the Azure06 project today, what parts are already implemented, and what we plan to tighten up next.

The short version is:

- The user signs in with Microsoft Entra using OAuth/OpenID Connect.
- The browser uses MSAL to complete the redirect-based sign-in flow.
- MSAL stores its authentication cache in browser `localStorage`.
- The frontend sends an access token to the API as an `Authorization: Bearer ...` header.
- The API validates that bearer token with ASP.NET Core JWT bearer authentication.
- After the token is trusted, the API maps the Entra user object id to an application user row in SQL.
- Application permissions come mainly from database memberships, not just token claims.

## The Main Pieces

The project has two authentication halves:

- Frontend: `apps/shell`
- Backend API: `services/cases-api`

The frontend is a React/Vite app. It uses `@azure/msal-browser` to talk to Microsoft Entra.

The API is an ASP.NET Core application. It uses JWT bearer authentication to validate tokens sent by the frontend.

## OAuth, OpenID Connect, and Tokens

Microsoft Entra sign-in is based on OAuth 2.0 and OpenID Connect.

OAuth answers:

> Is this caller allowed to call this API?

OpenID Connect answers:

> Who is this signed-in user?

In practice, the browser receives token information from Entra after the user signs in. MSAL manages most of the details for us.

There are two important token types:

- ID token: describes the signed-in user to the frontend.
- Access token: authorizes calls to an API.

The frontend should use the ID token to understand the signed-in identity. The frontend should use the access token when calling the backend API.

## What Happens When Login Starts

The Azure06 frontend login flow starts here:

```text
apps/shell/src/lib/entra/auth.ts
```

The important function is:

```ts
startEntraLogin(...)
```

It calls:

```ts
msalInstance.loginRedirect(...)
```

That sends the browser to Microsoft Entra. The app does not collect a password. Microsoft Entra does.

The app currently passes:

```ts
loginHint: selection.email
```

This does not prove identity. It only hints to Microsoft which email address should be shown first on the login screen.

## Redirect Back to the App

After the user signs in, Microsoft Entra redirects the browser back to:

```text
/auth/callback
```

The callback page calls:

```ts
completeEntraRedirect()
```

That function uses MSAL:

```ts
msalInstance.handleRedirectPromise(...)
```

MSAL handles the OAuth redirect response. Under the covers, this is the browser-based authorization code flow. MSAL exchanges the returned authorization code for tokens.

Azure06 does not manually perform that token exchange. MSAL does it.

## Browser Storage

Azure06 configures MSAL like this:

```ts
cache: {
  cacheLocation: "localStorage",
}
```

That means MSAL stores its authentication cache in browser `localStorage`.

The app also stores its own selected application user context in `localStorage` under the key:

```text
user
```

And it stores the pending sign-in selection under:

```text
entra.pendingSignInSelection
```

So there are two kinds of browser storage:

- MSAL token/account cache.
- Azure06 application context, such as selected role, selected organisation, and selected user.

For deployed environments, browser storage is separated by origin. These are different origins:

```text
https://testing.example.com
https://staging.example.com
https://production.example.com
```

For local development, all environments can share:

```text
http://localhost:5173
```

That means local testing/staging/production runs can accidentally share the same `localStorage`. We should namespace these keys by environment or Entra client id.

## What the Frontend Sends to the API

The frontend API client is here:

```text
apps/shell/src/lib/api/client.ts
```

For authenticated API requests, it calls:

```ts
getEntraAccessToken()
```

That function asks MSAL for an access token:

```ts
msalInstance.acquireTokenSilent({
  account,
  scopes: [entraConfig.apiScope],
})
```

Then the frontend sends this header:

```http
Authorization: Bearer <access-token>
```

Important correction:

The frontend does **not** send both the ID token and the access token to the API on every request.

The intended pattern is:

- ID token: used by the frontend to understand who signed in.
- Access token: sent to the API as the bearer token.

## What the API Checks First

The API authentication setup is in:

```text
services/cases-api/Program.cs
```

It configures JWT bearer authentication:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...)
```

The API checks that the bearer token is valid:

- It came from the expected Entra authority.
- It is for the expected audience.
- It has not expired.
- Its issuer is valid.

If this token validation fails, the API returns `401 Unauthorized`.

This is the first gate.

## Application User Mapping

Once the token is trusted, Azure06 still needs to map the Entra identity to an application user row.

That happens here:

```text
services/cases-api/CurrentUser/ApplicationUserResolver.cs
```

The resolver reads the Entra object id claim:

```csharp
oid
```

Then it looks for a matching SQL user:

```csharp
UserAccounts.EntraObjectId == identity.ObjectId
```

This is the key idea:

```text
Entra oid -> UserAccounts.EntraObjectId -> UserAccounts.Id
```

The `UserAccounts.Id` value is then used for relational queries inside the application.

That mirrors the AWS project pattern:

```text
Cognito sub -> registered_user.sub -> application user row
```

In Azure06:

```text
Entra oid -> UserAccounts.EntraObjectId -> application user row
```

## Authorization Inside the Application

Authentication answers:

> Is this a real signed-in Entra user?

Authorization answers:

> Is this signed-in application user allowed to do this action?

Azure06 mostly answers authorization questions using database memberships.

For example, after the API maps the Entra `oid` to a `UserAccount`, it loads memberships such as:

- Authority user
- Participant user
- Stakeholder user
- Agent user
- System owner user

Those memberships are used by domain logic to decide what the user can read or change.

So another correction is:

The API does not currently rely mainly on token claims to decide every business permission.

The API uses:

- token validation to trust the caller;
- Entra `oid` to find the application user;
- SQL memberships to authorize application actions.

If a signed-in user has no matching `UserAccounts.EntraObjectId`, they are authenticated by Entra but not recognized by the application. In that case, the app cannot resolve normal memberships and protected domain operations should fail.

## Current Gaps and Planned Improvements

Some pieces are implemented, but the final shape needs tightening.

### 1. API scope configuration

The frontend asks MSAL for an API access token using:

```text
VITE_ENTRA_API_SCOPE
```

If this value is empty, the frontend cannot get an access token for the API, and authenticated API calls will fail.

Planned final shape:

- Each environment has a real API scope.
- The frontend requests that scope.
- The API validates the resulting access token audience.

### 2. Use Entra object id as the proof, not email

Today, the frontend checks that the Entra email matches the selected demo user email.

That is fragile.

Email is useful as a login hint, but it should not be the final proof of identity.

Planned final shape:

```text
selected UserAccounts.Id
  -> database row has expected EntraObjectId
  -> Entra token returns actual oid
  -> expected oid must equal actual oid
```

The rule should be:

```text
Email helps the login screen.
OID proves the identity.
```

### 3. Runtime sign-in options

The sign-in page currently began life with build-time/demo user data in the frontend.

That is the source of the stale email problem.

Planned final shape:

- The sign-in page asks the API for demo sign-in options at runtime.
- The API reads SQL.
- SQL already contains the Entra object ids and current email addresses from the user setup scripts.
- The dropdowns show the live users and memberships.

### 4. Namespace browser storage

Current keys such as `user` and `entra.pendingSignInSelection` are too generic.

Planned final shape:

```text
azure06:{environment-or-client-id}:user
azure06:{environment-or-client-id}:pendingSignInSelection
```

This avoids local development confusion when testing/staging/production are all run from `localhost`.

## The Sign-In Page

Azure06 has a demo-specific sign-in page.

This is different from the AWS project, which simply has a normal login button.

Azure06 needs the dropdowns because demo users want to experience the application as different account types:

- Authority
- Participant
- Stakeholder
- Agent

The dropdowns are not the actual authentication mechanism. Microsoft Entra is still the authentication mechanism.

The dropdowns choose the intended application persona. Entra proves which real user signed in. The API then maps that Entra identity to the SQL user and memberships.

The final intended flow is:

```text
User chooses a demo persona
Frontend uses that email as an Entra login hint
User signs in with Microsoft Entra
Frontend/API validate the Entra oid
API maps oid to UserAccounts.Id
API loads memberships from SQL
Application permissions come from those memberships
```

## Corrected Version of the User's Mental Model

Original idea:

> The authentication code flow swaps a code for a token and the token content is stored in browser local storage.

Corrected:

> MSAL performs the redirect/code flow and stores its token/account cache in browser `localStorage`. Azure06 also stores application context in `localStorage`.

Original idea:

> The ID token and access tokens are both sent to the API server upon every request.

Corrected:

> The frontend sends the access token to the API as `Authorization: Bearer <access-token>`. The ID token is not intended to be sent to the API on every request.

Original idea:

> The ID token is tested and if the user is not an Entra user the API request fails.

Corrected:

> The API validates the bearer access token. If the token is not issued by the trusted Entra authority, has the wrong audience, or is expired, the API request fails.

Original idea:

> The access token is tested by the API server and if the caller does not have sufficient claims the request fails.

Corrected:

> The access token is validated first. Then application authorization is mainly checked against SQL memberships after mapping Entra `oid` to `UserAccounts.EntraObjectId`.

Original idea:

> Otherwise the request passes.

Corrected:

> The request passes only if the token is valid and the mapped application user has the required application membership/permission for that operation.
