import {
  PublicClientApplication,
  type AccountInfo,
  type AuthenticationResult,
} from "@azure/msal-browser";
import { entraConfig, getAuthStorageKey, getEntraRedirectUri, getMissingEntraConfigKeys } from "./config";

const pendingSelectionKey = getAuthStorageKey("entra.pendingSignInSelection");

export type EntraPendingSelection = {
  authenticatableUserId: string;
  entraObjectId?: string | null;
  name: string;
  email: string;
  authorityId: string;
  role: "authority-admin" | "participant" | "stakeholder" | "agent";
  accountContextId: string;
  accountContextType: "authority" | "participant" | "stakeholder" | "agent";
  accountContextEntityId: string;
  accountContextName: string;
  participantId: string | null;
  stakeholderId: string | null;
};

export type EntraAuthenticatedIdentity = {
  objectId: string;
  email: string;
  name: string | null;
};

const msalInstance = new PublicClientApplication({
  auth: {
    clientId: entraConfig.clientId ?? "",
    authority: entraConfig.authority,
    redirectUri: getEntraRedirectUri(),
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "localStorage",
  },
});

let initialized = false;

async function ensureMsalInitialized() {
  if (!initialized) {
    await msalInstance.initialize();
    initialized = true;
  }
}

export function assertEntraConfigured() {
  const missing = getMissingEntraConfigKeys();
  if (missing.length > 0) {
    throw new Error(`Missing Entra configuration: ${missing.join(", ")}`);
  }
}

export async function startEntraLogin(selection: EntraPendingSelection) {
  assertEntraConfigured();
  window.localStorage.setItem(pendingSelectionKey, JSON.stringify(selection));
  await ensureMsalInitialized();
  await msalInstance.loginRedirect({
    scopes: entraConfig.signInScopes,
    loginHint: selection.email,
    redirectUri: getEntraRedirectUri(),
    prompt: "select_account",
  });
}

export async function completeEntraRedirect() {
  assertEntraConfigured();
  await ensureMsalInitialized();
  const result = await msalInstance.handleRedirectPromise({
    navigateToLoginRequestUrl: false,
  });
  const account = result?.account ?? msalInstance.getAllAccounts()[0] ?? null;
  if (!account) {
    throw new Error("Entra did not return an authenticated account.");
  }
  msalInstance.setActiveAccount(account);
  return {
    selection: getPendingSelection(),
    identity: getIdentityFromAccount(account, result),
  };
}

export async function getEntraAccessToken() {
  if (!entraConfig.apiScope) return null;
  await ensureMsalInitialized();
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0] ?? null;
  if (!account) return null;
  const result = await msalInstance.acquireTokenSilent({
    account,
    scopes: [entraConfig.apiScope],
  });
  return result.accessToken;
}

export async function logoutFromEntra() {
  await ensureMsalInitialized();
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0] ?? undefined;
  window.localStorage.removeItem(pendingSelectionKey);
  await msalInstance.logoutRedirect({
    account,
    postLogoutRedirectUri: window.location.origin,
  });
}

export function clearPendingSelection() {
  window.localStorage.removeItem(pendingSelectionKey);
}

function getPendingSelection() {
  const raw = window.localStorage.getItem(pendingSelectionKey);
  if (!raw) {
    throw new Error("No selected application user was found for this Entra login.");
  }
  return JSON.parse(raw) as EntraPendingSelection;
}

function getIdentityFromAccount(
  account: AccountInfo,
  result: AuthenticationResult | null,
): EntraAuthenticatedIdentity {
  const claims = (result?.idTokenClaims ?? account.idTokenClaims ?? {}) as Record<string, unknown>;
  const emails = Array.isArray(claims.emails) ? claims.emails : [];
  const email =
    stringClaim(claims.email) ??
    stringClaim(claims.preferred_username) ??
    stringClaim(emails[0]) ??
    account.username;
  const objectId = stringClaim(claims.oid) ?? stringClaim(claims.sub) ?? account.localAccountId;
  if (!email) {
    throw new Error("The Entra token did not include an email address.");
  }
  if (!objectId) {
    throw new Error("The Entra token did not include a user object id.");
  }
  return {
    objectId,
    email: email.toLowerCase(),
    name: stringClaim(claims.name) ?? account.name ?? null,
  };
}

function stringClaim(value: unknown) {
  return typeof value === "string" && value.trim() ? value.trim() : null;
}
