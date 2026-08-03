// Base is empty on purpose: in dev, Vite's proxy (see vite.config.js) forwards
// /api/* to the backend same-origin. In prod, the client is expected to be served
// from the same origin as the API (or this gets swapped for an env var later).
const BASE_URL = '/api';

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export async function apiRequest(path, { method = 'GET', body } = {}) {
  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: body ? { 'Content-Type': 'application/json' } : undefined,
    body: body ? JSON.stringify(body) : undefined,
    // Required so the aquascale_session cookie is sent/stored — the API is cookie-auth only.
    credentials: 'include',
  });

  // 204 No Content (e.g. logout) — nothing to parse.
  if (res.status === 204) return null;

  let data = null;
  try {
    data = await res.json();
  } catch {
    // Non-JSON response body; leave data as null.
  }

  if (!res.ok) {
    const message = data?.message || `Request failed (${res.status})`;
    throw new ApiError(message, res.status);
  }

  return data;
}