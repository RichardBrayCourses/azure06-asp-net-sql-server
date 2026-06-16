export const ENTRA_CALLBACK_PATH = "/auth/callback";

const clientId = import.meta.env.VITE_ENTRA_CLIENT_ID;
const authority = import.meta.env.VITE_ENTRA_AUTHORITY;
const apiScope = import.meta.env.VITE_ENTRA_API_SCOPE;

export const entraConfig = {
  clientId,
  authority,
  apiScope,
  redirectPath: ENTRA_CALLBACK_PATH,
  scopes: apiScope ? [apiScope] : ["openid", "profile", "email"],
};

export function getEntraRedirectUri() {
  return `${window.location.origin}${ENTRA_CALLBACK_PATH}`;
}

export function getMissingEntraConfigKeys() {
  return [
    !clientId ? "VITE_ENTRA_CLIENT_ID" : null,
    !authority ? "VITE_ENTRA_AUTHORITY" : null,
  ].filter((key): key is string => Boolean(key));
}
