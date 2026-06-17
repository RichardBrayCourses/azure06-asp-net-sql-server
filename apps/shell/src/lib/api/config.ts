const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL;
const configuredDemoSignInKey = import.meta.env.VITE_DEMO_SIGN_IN_KEY;

export const apiConfig = {
  baseUrl: normalizeApiBaseUrl(configuredApiBaseUrl ?? "http://localhost:5294"),
  demoSignInKey: configuredDemoSignInKey ?? "dev-demo-sign-in-key",
};

function normalizeApiBaseUrl(value: string) {
  return value.replace(/\/+$/, "");
}
