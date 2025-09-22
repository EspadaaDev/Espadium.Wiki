const baseURL = process.env.NEXT_PUBLIC_API_URL || '';
let getAccessToken: () => string | null = () => null;
let clearAuth: () => void = () => {};
export const wireAuthAccessors = (get: typeof getAccessToken, clear: typeof clearAuth) => { getAccessToken = get; clearAuth = clear; };

async function tryRefresh() {
  const res = await fetch(baseURL + '/auth/refresh', { method: 'POST', credentials: 'include' });
  if (!res.ok) return false;
  const data = await res.json().catch(() => null);
  if (data && data.accessToken) {
    // let the app update token via store (page should call setAccessToken from response)
    return true;
  }
  return false;
}

export async function apiFetch(input: RequestInfo | URL, init?: RequestInit, retry = true): Promise<Response> {
  const token = getAccessToken();
  const headers = new Headers(init?.headers || {});
  if (token) headers.set('Authorization', 'Bearer ' + token);
  const url = typeof input === 'string' || input instanceof URL ? input.toString() : (input as Request).url;
  const res = await fetch(url.startsWith('http') ? url : baseURL + url, { ...init, headers, credentials: 'include' });
  if (res.status === 401 && retry) {
    const ok = await tryRefresh();
    if (ok) return apiFetch(input, init, false);
    clearAuth();
    if (typeof window !== 'undefined') window.location.href = '/login';
  }
  return res;
}

