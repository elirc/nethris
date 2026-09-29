export type EmploymentStatus = "Active" | "OnLeave" | "Terminated";
export type SecurityRole = "Employee" | "Manager" | "Hr" | "Admin";
export type PtoStatus = "PendingManager" | "PendingHr" | "Approved" | "Rejected" | "Cancelled";
export type PtoDecision = "Approved" | "Rejected";

export interface CurrentUser {
  userId: string;
  employeeId: string;
  email: string;
  name: string;
  role: SecurityRole;
}

export interface Department {
  id: string;
  name: string;
  costCenter?: string;
  leadName?: string;
  headcount: number;
}

export interface JobRole {
  id: string;
  title: string;
  level: string;
  salaryBandMin: number;
  salaryBandMax: number;
}

export interface Employee {
  id: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  location?: string;
  hireDate: string;
  status: EmploymentStatus;
  ptoBalanceHours: number;
  departmentId: string;
  department: string;
  jobRoleId: string;
  jobRole: string;
  roleLevel: string;
  managerId?: string;
  managerName?: string;
  directReportCount: number;
}

export interface OrgNode {
  id: string;
  name: string;
  title: string;
  department: string;
  location?: string;
  reports: OrgNode[];
}

export interface PtoApproval {
  id: string;
  approverName: string;
  decision: PtoDecision;
  comment?: string;
  createdAt: string;
}

export interface PtoRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  department: string;
  startDate: string;
  endDate: string;
  hours: number;
  reason: string;
  status: PtoStatus;
  createdAt: string;
  approvals: PtoApproval[];
}

export interface AuditLog {
  id: string;
  createdAt: string;
  actorEmail: string;
  action: string;
  entityType: string;
  entityId: string;
  details?: string;
}

export interface Snapshot {
  departments: Department[];
  jobRoles: JobRole[];
  employees: Employee[];
  orgChart: OrgNode[];
  ptoRequests: PtoRequest[];
  auditLogs: AuditLog[];
}

