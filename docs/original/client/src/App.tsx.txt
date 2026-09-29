import {
  Building2,
  CalendarCheck,
  Check,
  ChevronRight,
  ClipboardList,
  Clock3,
  LogOut,
  Search,
  ShieldCheck,
  UsersRound,
  X
} from "lucide-react";
import { FormEvent, useMemo, useState } from "react";
import { createPto, decidePto, loadSnapshot, login, Session } from "./api";
import { demoSnapshot, roleRank } from "./demoData";
import { Employee, OrgNode, PtoRequest, PtoStatus, Snapshot } from "./types";

type Tab = "directory" | "org" | "pto" | "approvals" | "audit";

const statusLabels: Record<PtoStatus, string> = {
  PendingManager: "Manager review",
  PendingHr: "HR review",
  Approved: "Approved",
  Rejected: "Rejected",
  Cancelled: "Cancelled"
};

export default function App() {
  const [session, setSession] = useState<Session>();
  const [snapshot, setSnapshot] = useState<Snapshot>(demoSnapshot);
  const [activeTab, setActiveTab] = useState<Tab>("directory");
  const [query, setQuery] = useState("");
  const [departmentId, setDepartmentId] = useState("all");
  const [selectedEmployeeId, setSelectedEmployeeId] = useState(demoSnapshot.employees[0]?.id);
  const [authError, setAuthError] = useState("");
  const [loading, setLoading] = useState(false);

  const selectedEmployee = snapshot.employees.find((employee) => employee.id === selectedEmployeeId) ?? snapshot.employees[0];
  const isPeopleOps = session ? roleRank(session.user.role) >= roleRank("Hr") : false;
  const canApprove = session ? roleRank(session.user.role) >= roleRank("Manager") : false;

  const filteredEmployees = useMemo(() => {
    return snapshot.employees.filter((employee) => {
      const matchesQuery = `${employee.fullName} ${employee.email} ${employee.employeeNumber} ${employee.jobRole}`
        .toLowerCase()
        .includes(query.toLowerCase());
      const matchesDepartment = departmentId === "all" || employee.departmentId === departmentId;
      return matchesQuery && matchesDepartment;
    });
  }, [snapshot.employees, query, departmentId]);

  const visibleRequests = useMemo(() => {
    if (!session) return snapshot.ptoRequests;
    if (session.user.role === "Employee") {
      return snapshot.ptoRequests.filter((request) => request.employeeId === session.user.employeeId);
    }
    if (session.user.role === "Manager") {
      const reportIds = snapshot.employees.filter((employee) => employee.managerId === session.user.employeeId).map((employee) => employee.id);
      return snapshot.ptoRequests.filter((request) => request.employeeId === session.user.employeeId || reportIds.includes(request.employeeId));
    }
    return snapshot.ptoRequests;
  }, [session, snapshot.employees, snapshot.ptoRequests]);

  const approvalQueue = visibleRequests.filter((request) => {
    if (!session) return false;
    if (session.user.role === "Manager") return request.status === "PendingManager";
    return request.status === "PendingManager" || request.status === "PendingHr";
  });

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setAuthError("");
    setLoading(true);
    const form = new FormData(event.currentTarget);
    try {
      const nextSession = await login(String(form.get("email")), String(form.get("password")));
      const nextSnapshot = await loadSnapshot(nextSession);
      setSession(nextSession);
      setSnapshot(nextSnapshot);
      setSelectedEmployeeId(nextSnapshot.employees[0]?.id);
    } catch (error) {
      setAuthError(error instanceof Error ? error.message : "Unable to sign in.");
    } finally {
      setLoading(false);
    }
  }

  async function handleCreatePto(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!session) return;
    const form = new FormData(event.currentTarget);
    const created = await createPto(
      session,
      {
        startDate: String(form.get("startDate")),
        endDate: String(form.get("endDate")),
        hours: Number(form.get("hours")),
        reason: String(form.get("reason"))
      },
      snapshot
    );

    setSnapshot((current) => ({
      ...current,
      ptoRequests: [created, ...current.ptoRequests],
      auditLogs: [
        {
          id: crypto.randomUUID(),
          createdAt: new Date().toISOString(),
          actorEmail: session.user.email,
          action: "Requested",
          entityType: "PtoRequest",
          entityId: created.id,
          details: `${created.hours} PTO hours`
        },
        ...current.auditLogs
      ]
    }));
    event.currentTarget.reset();
  }

  async function handleDecision(request: PtoRequest, decision: "Approved" | "Rejected") {
    if (!session) return;
    const result = await decidePto(session, request.id, decision, decision === "Approved" ? "Looks good." : "Please adjust dates.");
    const nextStatus = decision === "Rejected" ? "Rejected" : request.status === "PendingManager" && session.user.role === "Manager" ? "PendingHr" : "Approved";

    setSnapshot((current) => ({
      ...current,
      ptoRequests: current.ptoRequests.map((item) =>
        item.id === request.id
          ? {
              ...item,
              ...("status" in result ? result : {}),
              status: "status" in result ? result.status : nextStatus,
              approvals: [
                ...item.approvals,
                {
                  id: crypto.randomUUID(),
                  approverName: session.user.name,
                  decision,
                  comment: decision === "Approved" ? "Looks good." : "Please adjust dates.",
                  createdAt: new Date().toISOString()
                }
              ]
            }
          : item
      ),
      auditLogs: [
        {
          id: crypto.randomUUID(),
          createdAt: new Date().toISOString(),
          actorEmail: session.user.email,
          action: decision,
          entityType: "PtoRequest",
          entityId: request.id,
          details: nextStatus
        },
        ...current.auditLogs
      ]
    }));
  }

  if (!session) {
    return <LoginScreen loading={loading} error={authError} onLogin={handleLogin} />;
  }

  return (
    <div className="appShell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brandMark">N</div>
          <div>
            <strong>Nethris</strong>
            <span>HRIS Workspace</span>
          </div>
        </div>

        <nav className="navList" aria-label="Main navigation">
          <NavButton icon={<UsersRound size={18} />} label="Directory" active={activeTab === "directory"} onClick={() => setActiveTab("directory")} />
          <NavButton icon={<Building2 size={18} />} label="Org chart" active={activeTab === "org"} onClick={() => setActiveTab("org")} />
          <NavButton icon={<CalendarCheck size={18} />} label="PTO" active={activeTab === "pto"} onClick={() => setActiveTab("pto")} />
          {canApprove && <NavButton icon={<Check size={18} />} label="Approvals" active={activeTab === "approvals"} onClick={() => setActiveTab("approvals")} />}
          {isPeopleOps && <NavButton icon={<ClipboardList size={18} />} label="Audit logs" active={activeTab === "audit"} onClick={() => setActiveTab("audit")} />}
        </nav>

        <div className="sessionCard">
          <span className="eyebrow">{session.mode === "api" ? "API session" : "Demo session"}</span>
          <strong>{session.user.name}</strong>
          <span>{session.user.role}</span>
          <button className="ghostButton" onClick={() => setSession(undefined)}>
            <LogOut size={16} /> Sign out
          </button>
        </div>
      </aside>

      <main className="main">
        <header className="topbar">
          <div>
            <span className="eyebrow">People operations</span>
            <h1>{activeTitle(activeTab)}</h1>
          </div>
          <div className="metrics">
            <Metric label="Employees" value={snapshot.employees.length.toString()} />
            <Metric label="Departments" value={snapshot.departments.length.toString()} />
            <Metric label="Open PTO" value={approvalQueue.length.toString()} />
          </div>
        </header>

        {activeTab === "directory" && (
          <section className="directoryLayout">
            <div className="panel directoryPanel">
              <div className="toolbar">
                <label className="searchBox">
                  <Search size={17} />
                  <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search people, roles, email" />
                </label>
                <select value={departmentId} onChange={(event) => setDepartmentId(event.target.value)}>
                  <option value="all">All departments</option>
                  {snapshot.departments.map((department) => (
                    <option key={department.id} value={department.id}>
                      {department.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="employeeList">
                {filteredEmployees.map((employee) => (
                  <button
                    className={`employeeRow ${employee.id === selectedEmployee?.id ? "selected" : ""}`}
                    key={employee.id}
                    onClick={() => setSelectedEmployeeId(employee.id)}
                  >
                    <Avatar name={employee.fullName} />
                    <span>
                      <strong>{employee.fullName}</strong>
                      <small>{employee.jobRole}</small>
                    </span>
                    <em>{employee.department}</em>
                  </button>
                ))}
              </div>
            </div>

            {selectedEmployee && <ProfilePanel employee={selectedEmployee} employees={snapshot.employees} />}
          </section>
        )}

        {activeTab === "org" && (
          <section className="panel">
            <div className="sectionHeader">
              <div>
                <h2>Org chart</h2>
                <p>Manager relationships and direct reporting lines.</p>
              </div>
            </div>
            <div className="orgForest">
              {snapshot.orgChart.map((node) => (
                <OrgNodeView key={node.id} node={node} />
              ))}
            </div>
          </section>
        )}

        {activeTab === "pto" && (
          <section className="ptoGrid">
            <div className="panel">
              <div className="sectionHeader">
                <div>
                  <h2>Request PTO</h2>
                  <p>Submits to manager review, then HR approval.</p>
                </div>
              </div>
              <form className="formGrid" onSubmit={handleCreatePto}>
                <label>
                  Start date
                  <input name="startDate" type="date" required />
                </label>
                <label>
                  End date
                  <input name="endDate" type="date" required />
                </label>
                <label>
                  Hours
                  <input name="hours" type="number" min="1" step="1" defaultValue="8" required />
                </label>
                <label className="wide">
                  Reason
                  <textarea name="reason" rows={4} placeholder="Vacation, appointment, personal time..." required />
                </label>
                <button className="primaryButton">Submit request</button>
              </form>
            </div>

            <PtoList title="My visible requests" requests={visibleRequests} />
          </section>
        )}

        {activeTab === "approvals" && (
          <section className="panel">
            <div className="sectionHeader">
              <div>
                <h2>Approval queue</h2>
                <p>Managers review direct reports first; HR or admins can finalize.</p>
              </div>
            </div>
            <div className="approvalList">
              {approvalQueue.length === 0 && <EmptyState title="No approvals waiting" detail="The queue is clear." />}
              {approvalQueue.map((request) => (
                <div className="approvalItem" key={request.id}>
                  <PtoSummary request={request} />
                  <div className="approvalActions">
                    <button className="approveButton" onClick={() => handleDecision(request, "Approved")}>
                      <Check size={16} /> Approve
                    </button>
                    <button className="rejectButton" onClick={() => handleDecision(request, "Rejected")}>
                      <X size={16} /> Reject
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </section>
        )}

        {activeTab === "audit" && (
          <section className="panel">
            <div className="sectionHeader">
              <div>
                <h2>Audit logs</h2>
                <p>People data changes and approval workflow decisions.</p>
              </div>
            </div>
            <div className="auditList">
              {snapshot.auditLogs.map((log) => (
                <div className="auditItem" key={log.id}>
                  <Clock3 size={17} />
                  <span>
                    <strong>
                      {log.action} {log.entityType}
                    </strong>
                    <small>
                      {log.actorEmail} · {new Date(log.createdAt).toLocaleString()}
                    </small>
                  </span>
                  <em>{log.details}</em>
                </div>
              ))}
            </div>
          </section>
        )}
      </main>
    </div>
  );
}

function LoginScreen({ loading, error, onLogin }: { loading: boolean; error: string; onLogin: (event: FormEvent<HTMLFormElement>) => void }) {
  return (
    <main className="loginPage">
      <section className="loginPanel">
        <div className="brand loginBrand">
          <div className="brandMark">N</div>
          <div>
            <strong>Nethris</strong>
            <span>HRIS Workspace</span>
          </div>
        </div>
        <h1>Employee directory and HR workflows</h1>
        <form className="loginForm" onSubmit={onLogin}>
          <label>
            Email
            <input name="email" type="email" defaultValue="hr@nethris.local" required />
          </label>
          <label>
            Password
            <input name="password" type="password" defaultValue="Password123!" required />
          </label>
          {error && <p className="formError">{error}</p>}
          <button className="primaryButton" disabled={loading}>
            {loading ? "Signing in..." : "Sign in"}
          </button>
        </form>
        <div className="loginHints">
          <span>Try admin@nethris.local</span>
          <span>hr@nethris.local</span>
          <span>maya@nethris.local</span>
          <span>alex@nethris.local</span>
        </div>
      </section>
    </main>
  );
}

function NavButton({ icon, label, active, onClick }: { icon: React.ReactNode; label: string; active: boolean; onClick: () => void }) {
  return (
    <button className={`navButton ${active ? "active" : ""}`} onClick={onClick}>
      {icon}
      {label}
    </button>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="metric">
      <strong>{value}</strong>
      <span>{label}</span>
    </div>
  );
}

function ProfilePanel({ employee, employees }: { employee: Employee; employees: Employee[] }) {
  const reports = employees.filter((item) => item.managerId === employee.id);
  return (
    <aside className="panel profilePanel">
      <Avatar name={employee.fullName} large />
      <h2>{employee.fullName}</h2>
      <p>{employee.jobRole}</p>
      <div className="profileFacts">
        <Fact label="Employee ID" value={employee.employeeNumber} />
        <Fact label="Department" value={employee.department} />
        <Fact label="Level" value={employee.roleLevel} />
        <Fact label="Manager" value={employee.managerName ?? "None"} />
        <Fact label="Location" value={employee.location ?? "Remote"} />
        <Fact label="PTO balance" value={`${employee.ptoBalanceHours} hours`} />
        <Fact label="Hire date" value={formatDate(employee.hireDate)} />
        <Fact label="Status" value={employee.status} />
      </div>
      <div className="reportList">
        <strong>Direct reports</strong>
        {reports.length === 0 ? <small>No direct reports</small> : reports.map((report) => <span key={report.id}>{report.fullName}</span>)}
      </div>
    </aside>
  );
}

function OrgNodeView({ node }: { node: OrgNode }) {
  return (
    <div className="orgNode">
      <div className="orgPerson">
        <Avatar name={node.name} />
        <span>
          <strong>{node.name}</strong>
          <small>
            {node.title} · {node.department}
          </small>
        </span>
      </div>
      {node.reports.length > 0 && (
        <div className="orgReports">
          {node.reports.map((report) => (
            <OrgNodeView key={report.id} node={report} />
          ))}
        </div>
      )}
    </div>
  );
}

function PtoList({ title, requests }: { title: string; requests: PtoRequest[] }) {
  return (
    <div className="panel">
      <div className="sectionHeader">
        <div>
          <h2>{title}</h2>
          <p>Recent time off activity.</p>
        </div>
      </div>
      <div className="requestList">
        {requests.length === 0 && <EmptyState title="No PTO requests" detail="Submitted requests will appear here." />}
        {requests.map((request) => (
          <PtoSummary key={request.id} request={request} />
        ))}
      </div>
    </div>
  );
}

function PtoSummary({ request }: { request: PtoRequest }) {
  return (
    <div className="requestItem">
      <div className="requestTopline">
        <span>
          <strong>{request.employeeName}</strong>
          <small>{request.department}</small>
        </span>
        <StatusPill status={request.status} />
      </div>
      <p>
        {formatDate(request.startDate)} <ChevronRight size={14} /> {formatDate(request.endDate)} · {request.hours} hours
      </p>
      <small>{request.reason}</small>
    </div>
  );
}

function StatusPill({ status }: { status: PtoStatus }) {
  return <span className={`statusPill ${status}`}>{statusLabels[status]}</span>;
}

function EmptyState({ title, detail }: { title: string; detail: string }) {
  return (
    <div className="emptyState">
      <ShieldCheck size={28} />
      <strong>{title}</strong>
      <span>{detail}</span>
    </div>
  );
}

function Avatar({ name, large = false }: { name: string; large?: boolean }) {
  const initials = name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
  return <span className={`avatar ${large ? "large" : ""}`}>{initials}</span>;
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="fact">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function activeTitle(tab: Tab) {
  return {
    directory: "Employee directory",
    org: "Org chart",
    pto: "PTO requests",
    approvals: "Approvals",
    audit: "Audit logs"
  }[tab];
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: "short", day: "numeric", year: "numeric" }).format(new Date(`${value}T00:00:00`));
}
