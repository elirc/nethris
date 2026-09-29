import { demoSnapshot, demoUsers, roleRank } from "./demoData";
import { CurrentUser, PtoDecision, PtoRequest, Snapshot } from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5187";

export interface Session {
  token?: string;
  user: CurrentUser;
  mode: "api" | "demo";
}

export async function login(email: string, password: string): Promise<Session> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password })
    });

    if (response.ok) {
      const data = await response.json();
      return { token: data.token, user: data.user, mode: "api" };
    }
  } catch {
    // The app remains fully explorable with local demo data when the API is not running.
  }

  const demo = demoUsers[email.toLowerCase()];
  if (!demo || demo.password !== password) {
    throw new Error("Invalid email or password.");
  }

  const { password: _password, ...user } = demo;
  return { user, mode: "demo" };
}

export async function loadSnapshot(session: Session): Promise<Snapshot> {
  if (session.mode === "demo") {
    return structuredClone(demoSnapshot);
  }

  const [departments, jobRoles, employees, orgChart, ptoRequests, auditLogs] = await Promise.all([
    request(session, "/api/departments"),
    request(session, "/api/job-roles"),
    request(session, "/api/employees"),
    request(session, "/api/org-chart"),
    request(session, "/api/pto"),
    roleRank(session.user.role) >= roleRank("Hr") ? request(session, "/api/audit-logs") : Promise.resolve([])
  ]);

  return { departments, jobRoles, employees, orgChart, ptoRequests, auditLogs };
}

export async function createPto(session: Session, payload: { startDate: string; endDate: string; hours: number; reason: string }, snapshot: Snapshot) {
  if (session.mode === "api") {
    return request(session, "/api/pto", {
      method: "POST",
      body: JSON.stringify(payload)
    });
  }

  const employee = snapshot.employees.find((item) => item.id === session.user.employeeId);
  const requestItem: PtoRequest = {
    id: crypto.randomUUID(),
    employeeId: session.user.employeeId,
    employeeName: session.user.name,
    department: employee?.department ?? "Unknown",
    startDate: payload.startDate,
    endDate: payload.endDate,
    hours: payload.hours,
    reason: payload.reason,
    status: "PendingManager",
    createdAt: new Date().toISOString(),
    approvals: []
  };
  return requestItem;
}

export async function decidePto(session: Session, id: string, decision: PtoDecision, comment?: string) {
  if (session.mode === "api") {
    return request(session, `/api/pto/${id}/decision`, {
      method: "POST",
      body: JSON.stringify({ decision, comment })
    });
  }

  return { id, decision, comment };
}

async function request(session: Session, path: string, init: RequestInit = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(session.token ? { Authorization: `Bearer ${session.token}` } : {}),
      ...init.headers
    }
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  return response.json();
}

