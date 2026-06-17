import { getEntraAccessToken } from "@/lib/entra/auth";
import { apiConfig } from "./config";

type ApiRequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly problem?: unknown,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export async function apiRequest<T>(path: string, options: ApiRequestOptions = {}) {
  const accessToken = await getEntraAccessToken();
  if (!accessToken) {
    throw new ApiError("Sign in again to call the API.", 401);
  }

  const headers = new Headers(options.headers);
  headers.set("Authorization", `Bearer ${accessToken}`);
  if (options.body !== undefined && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    ...options,
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (response.status === 401) {
    throw new ApiError("Your API session has expired. Sign in again.", 401, await readProblem(response));
  }

  if (response.status === 403) {
    throw new ApiError("You do not have access to that API resource.", 403, await readProblem(response));
  }

  if (!response.ok) {
    const problem = await readProblem(response);
    throw new ApiError(problemMessage(problem) ?? `API request failed with status ${response.status}.`, response.status, problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export async function publicApiRequest<T>(path: string, options: ApiRequestOptions = {}) {
  const headers = new Headers(options.headers);
  if (options.body !== undefined && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const response = await fetch(`${apiConfig.baseUrl}${path}`, {
    ...options,
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (!response.ok) {
    const problem = await readProblem(response);
    throw new ApiError(problemMessage(problem) ?? `API request failed with status ${response.status}.`, response.status, problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

async function readProblem(response: Response) {
  const text = await response.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

function problemMessage(problem: unknown) {
  if (!problem || typeof problem !== "object") {
    return typeof problem === "string" ? problem : null;
  }

  const candidate = problem as Record<string, unknown>;
  return typeof candidate.detail === "string"
    ? candidate.detail
    : typeof candidate.title === "string"
      ? candidate.title
      : null;
}
