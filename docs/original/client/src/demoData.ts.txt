import { AuditLog, CurrentUser, Department, Employee, JobRole, OrgNode, PtoRequest, SecurityRole, Snapshot } from "./types";

const ids = {
  ops: "dept-ops",
  people: "dept-people",
  engineering: "dept-engineering",
  finance: "dept-finance",
  ceo: "role-ceo",
  vpPeople: "role-vp-people",
  engManager: "role-eng-manager",
  seniorEngineer: "role-senior-engineer",
  analyst: "role-analyst",
  accountant: "role-accountant",
  riley: "emp-riley",
  jordan: "emp-jordan",
  maya: "emp-maya",
  alex: "emp-alex",
  sam: "emp-sam",
  priya: "emp-priya"
};

export const demoUsers: Record<string, CurrentUser & { password: string }> = {
  "admin@nethris.local": {
    userId: "user-admin",
    employeeId: ids.riley,
    email: "admin@nethris.local",
    name: "Riley Chen",
    role: "Admin",
    password: "Password123!"
  },
  "hr@nethris.local": {
    userId: "user-hr",
    employeeId: ids.jordan,
    email: "hr@nethris.local",
    name: "Jordan Patel",
    role: "Hr",
    password: "Password123!"
  },
  "maya@nethris.local": {
    userId: "user-maya",
    employeeId: ids.maya,
    email: "maya@nethris.local",
    name: "Maya Singh",
    role: "Manager",
    password: "Password123!"
  },
  "alex@nethris.local": {
    userId: "user-alex",
    employeeId: ids.alex,
    email: "alex@nethris.local",
    name: "Alex Rivera",
    role: "Employee",
    password: "Password123!"
  }
};

export const departments: Department[] = [
  { id: ids.ops, name: "Operations", costCenter: "OPS-400", leadName: "Riley Chen", headcount: 1 },
  { id: ids.people, name: "People", costCenter: "PPL-200", leadName: "Jordan Patel", headcount: 2 },
  { id: ids.engineering, name: "Engineering", costCenter: "ENG-100", leadName: "Maya Singh", headcount: 2 },
  { id: ids.finance, name: "Finance", costCenter: "FIN-300", leadName: "Priya Iyer", headcount: 1 }
];

export const jobRoles: JobRole[] = [
  { id: ids.ceo, title: "Chief Executive Officer", level: "Executive", salaryBandMin: 220000, salaryBandMax: 320000 },
  { id: ids.vpPeople, title: "VP People", level: "Executive", salaryBandMin: 170000, salaryBandMax: 240000 },
  { id: ids.engManager, title: "Engineering Manager", level: "M4", salaryBandMin: 150000, salaryBandMax: 210000 },
  { id: ids.seniorEngineer, title: "Senior Software Engineer", level: "IC4", salaryBandMin: 130000, salaryBandMax: 180000 },
  { id: ids.analyst, title: "People Ops Analyst", level: "IC2", salaryBandMin: 70000, salaryBandMax: 95000 },
  { id: ids.accountant, title: "Staff Accountant", level: "IC2", salaryBandMin: 72000, salaryBandMax: 98000 }
];

export const employees: Employee[] = [
  employee(ids.riley, "NET-1001", "Riley", "Chen", "admin@nethris.local", "Operations", ids.ops, "Chief Executive Officer", ids.ceo, "Executive", undefined, undefined, 1, 184, "New York", "2019-01-14"),
  employee(ids.jordan, "NET-1002", "Jordan", "Patel", "hr@nethris.local", "People", ids.people, "VP People", ids.vpPeople, "Executive", ids.riley, "Riley Chen", 1, 168, "Remote", "2020-03-02"),
  employee(ids.maya, "NET-1003", "Maya", "Singh", "maya@nethris.local", "Engineering", ids.engineering, "Engineering Manager", ids.engManager, "M4", ids.riley, "Riley Chen", 1, 156, "San Francisco", "2021-08-09"),
  employee(ids.alex, "NET-1004", "Alex", "Rivera", "alex@nethris.local", "Engineering", ids.engineering, "Senior Software Engineer", ids.seniorEngineer, "IC4", ids.maya, "Maya Singh", 0, 132, "San Francisco", "2022-06-21"),
  employee(ids.sam, "NET-1005", "Sam", "Okafor", "sam@nethris.local", "People", ids.people, "People Ops Analyst", ids.analyst, "IC2", ids.jordan, "Jordan Patel", 0, 96, "Austin", "2023-04-10"),
  employee(ids.priya, "NET-1006", "Priya", "Iyer", "priya@nethris.local", "Finance", ids.finance, "Staff Accountant", ids.accountant, "IC2", ids.riley, "Riley Chen", 0, 148, "Chicago", "2021-11-15")
];

export const orgChart: OrgNode[] = [
  {
    id: ids.riley,
    name: "Riley Chen",
    title: "Chief Executive Officer",
    department: "Operations",
    location: "New York",
    reports: [
      {
        id: ids.jordan,
        name: "Jordan Patel",
        title: "VP People",
        department: "People",
        location: "Remote",
        reports: [
          { id: ids.sam, name: "Sam Okafor", title: "People Ops Analyst", department: "People", location: "Austin", reports: [] }
        ]
      },
      {
        id: ids.maya,
        name: "Maya Singh",
        title: "Engineering Manager",
        department: "Engineering",
        location: "San Francisco",
        reports: [
          { id: ids.alex, name: "Alex Rivera", title: "Senior Software Engineer", department: "Engineering", location: "San Francisco", reports: [] }
        ]
      },
      { id: ids.priya, name: "Priya Iyer", title: "Staff Accountant", department: "Finance", location: "Chicago", reports: [] }
    ]
  }
];

export const ptoRequests: PtoRequest[] = [
  {
    id: "pto-1",
    employeeId: ids.alex,
    employeeName: "Alex Rivera",
    department: "Engineering",
    startDate: futureDate(14),
    endDate: futureDate(16),
    hours: 24,
    reason: "Family trip",
    status: "PendingManager",
    createdAt: new Date().toISOString(),
    approvals: []
  },
  {
    id: "pto-2",
    employeeId: ids.sam,
    employeeName: "Sam Okafor",
    department: "People",
    startDate: futureDate(3),
    endDate: futureDate(4),
    hours: 16,
    reason: "Long weekend",
    status: "Approved",
    createdAt: new Date(Date.now() - 86400000 * 3).toISOString(),
    approvals: [
      { id: "approval-1", approverName: "Jordan Patel", decision: "Approved", comment: "Enjoy.", createdAt: new Date(Date.now() - 86400000 * 2).toISOString() }
    ]
  }
];

export const auditLogs: AuditLog[] = [
  {
    id: "audit-1",
    createdAt: new Date().toISOString(),
    actorEmail: "system@nethris.local",
    action: "Seeded",
    entityType: "Tenant",
    entityId: "initial",
    details: "Created seed departments, employees, accounts, and PTO requests."
  }
];

export const demoSnapshot: Snapshot = {
  departments,
  jobRoles,
  employees,
  orgChart,
  ptoRequests,
  auditLogs
};

export function roleRank(role: SecurityRole) {
  return ["Employee", "Manager", "Hr", "Admin"].indexOf(role);
}

function employee(
  id: string,
  employeeNumber: string,
  firstName: string,
  lastName: string,
  email: string,
  department: string,
  departmentId: string,
  jobRole: string,
  jobRoleId: string,
  roleLevel: string,
  managerId: string | undefined,
  managerName: string | undefined,
  directReportCount: number,
  ptoBalanceHours: number,
  location: string,
  hireDate: string
): Employee {
  return {
    id,
    employeeNumber,
    firstName,
    lastName,
    fullName: `${firstName} ${lastName}`,
    email,
    phone: "(555) 014-0000",
    location,
    hireDate,
    status: "Active",
    ptoBalanceHours,
    department,
    departmentId,
    jobRole,
    jobRoleId,
    roleLevel,
    managerId,
    managerName,
    directReportCount
  };
}

function futureDate(offsetDays: number) {
  const date = new Date();
  date.setDate(date.getDate() + offsetDays);
  return date.toISOString().slice(0, 10);
}

