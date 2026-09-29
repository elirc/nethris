export type Role = 'Employee' | 'Manager' | 'Hr' | 'Admin';
export interface User { userId: string; employeeId: string; email: string; name: string; role: Role; version: number }
export interface Session { token: string; user: User; expiresAt: string }
export interface Row { id: string; version: number; [field: string]: unknown }
export interface Page { items: Row[]; total: number; offset: number; limit: number }
export interface Operation { path: string; method: string; body?: string; key: string; etag?: string }
export class ApiError extends Error { constructor(public status: number, message: string) { super(message); } }
export const etag = (row: Row) => `"${row.id.replace(/-/g, '')}-${row.version}"`;
export async function request<T>(path: string, token?: string, operation?: Operation): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`/api/${path}`, {
      method: operation?.method ?? 'GET', cache: 'no-store',
      headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(operation ? { 'Content-Type': 'application/json', 'Idempotency-Key': operation.key,
          ...(operation.etag ? { 'If-Match': operation.etag } : {}) } : {}) }, body: operation?.body
    });
  } catch { throw new ApiError(0, 'Connection interrupted. A save may have committed. Retry the exact operation to recover its result.'); }
  let value: unknown;
  try { value = await response.json(); }
  catch { throw new ApiError(0, 'The response could not be read. Retry the exact operation before making another change.'); }
  if (!response.ok) throw new ApiError(response.status, (value as { message?: string }).message ?? `Request failed (${response.status}).`);
  return value as T;
}
export function operation(path: string, method: string, body?: unknown, row?: Row): Operation {
  return { path, method, body: body === undefined ? undefined : JSON.stringify(body), key: crypto.randomUUID(), etag: row ? etag(row) : undefined };
}
export async function allRows(kind: string, token: string, maximum: number): Promise<Row[]> {
  const rows: Row[] = [];
  for (let offset = 0; offset < maximum; offset += 100) {
    const page = await request<Page>(`${kind}?offset=${offset}&limit=100`, token); rows.push(...page.items);
    if (rows.length >= page.total) return rows;
  }
  throw new ApiError(409, `The ${kind} selector exceeds this workshop's supported size.`);
}
