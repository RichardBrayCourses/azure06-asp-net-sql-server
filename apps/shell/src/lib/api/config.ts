const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL;

export const apiConfig = {
  baseUrl: normalizeApiBaseUrl(configuredApiBaseUrl ?? "http://localhost:5294"),
};

function normalizeApiBaseUrl(value: string) {
  return value.replace(/\/+$/, "");
}
