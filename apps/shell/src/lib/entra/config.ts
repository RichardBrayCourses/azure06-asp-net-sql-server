export const ENTRA_CALLBACK_PATH = "/auth/callback";

const clientId = import.meta.env.VITE_ENTRA_CLIENT_ID;
const authority = import.meta.env.VITE_ENTRA_AUTHORITY;
const apiScope = import.meta.env.VITE_ENTRA_API_SCOPE;
const environmentName = import.meta.env.VITE_APP_ENVIRONMENT;

export const entraConfig = {
  clientId,
  authority,
  apiScope,
  environmentName,
  redirectPath: ENTRA_CALLBACK_PATH,
  signInScopes: ["openid", "profile", "email"],
};

export function getAuthStorageKey(name: string) {
  return [
    "azure06",
    environmentName || clientId || window.location.origin,
    name,
  ].join(":");
}

export function getEntraRedirectUri() {
  return `${window.location.origin}${ENTRA_CALLBACK_PATH}`;
}

export function getMissingEntraConfigKeys() {
  return [
    !clientId ? "VITE_ENTRA_CLIENT_ID" : null,
    !authority ? "VITE_ENTRA_AUTHORITY" : null,
  ].filter((key): key is string => Boolean(key));
}
