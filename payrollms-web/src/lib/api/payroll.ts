import { apiClient } from "./client";

export interface PayrollRunResponse {
  id: string;
  companyId: string;
  financialYearId: string;
  periodYear: number;
  periodMonth: number;
  status: string;
  runType: string;
  filterBranchId: string | null;
  filterDepartmentId: string | null;
  totalEmployees: number;
  totalGross: number;
  totalDeductions: number;
  totalNet: number;
  generatedAt: string | null;
  generatedBy: string | null;
  remarks: string | null;
  runVersion: number;
  parentRunId: string | null;
  createdAt: string;
}

export interface PagedPayrollRunsResponse {
  items: PayrollRunResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PayrollEntryComponentResponse {
  id: string;
  payrollEntryId: string;
  salaryComponentId: string;
  componentCode: string;
  componentName: string;
  componentType: string;
  formulaUsed: string;
  calculatedAmount: number;
  isManualOverride: boolean;
}

export interface PayrollEntryResponse {
  id: string;
  payrollRunId: string;
  companyId: string;
  externalEmployeeId: string;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  designationName: string;
  bankName: string | null;
  iban: string | null;
  baseSalary: number;
  workingDays: number;
  absentDays: number;
  lateDays: number;
  grossSalary: number;
  totalDeductions: number;
  netSalary: number;
  status: string;
  components: PayrollEntryComponentResponse[];
}

export interface PagedPayrollEntriesResponse {
  items: PayrollEntryResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreatePayrollRunCommand {
  companyId?: string;
  financialYearId: string;
  periodYear: number;
  periodMonth: number;
  runType?: string;
  filterBranchId?: string;
  filterDepartmentId?: string;
  remarks?: string;
  createdBy?: string;
}

export interface OverrideComponentRequest {
  overrideAmount: number;
  overriddenBy: string;
  reason: string;
}

export interface ApproveRequest {
  actorUserId: string;
  actorName: string;
  actorRole: string;
  comments?: string;
}

export interface RejectRequest {
  actorUserId: string;
  actorName: string;
  actorRole: string;
  comments: string;
}

export interface PendingApprovalItemResponse {
  payrollRunId: string;
  companyId: string;
  periodYear: number;
  periodMonth: number;
  runType: string;
  totalEmployees: number;
  totalNet: number;
  pendingStepOrder: number;
  pendingStepName: string;
  requiredRole: string;
  createdAt: string;
}

export const payrollApi = {
  getPayrollRuns: async (
    page = 1,
    pageSize = 20,
    status?: string,
    periodYear?: number,
    periodMonth?: number
  ): Promise<PagedPayrollRunsResponse> => {
    const params = new URLSearchParams({ page: page.toString(), pageSize: pageSize.toString() });
    if (status) params.append("status", status);
    if (periodYear) params.append("periodYear", periodYear.toString());
    if (periodMonth) params.append("periodMonth", periodMonth.toString());

    const res = await apiClient.get<PagedPayrollRunsResponse>(`/v1/payroll/runs?${params}`);
    return res.data;
  },

  getPayrollRunById: async (id: string): Promise<PayrollRunResponse> => {
    const res = await apiClient.get<PayrollRunResponse>(`/v1/payroll/runs/${id}`);
    return res.data;
  },

  createPayrollRun: async (command: CreatePayrollRunCommand): Promise<{ payrollRunId: string; status: string; message: string }> => {
    const res = await apiClient.post<{ payrollRunId: string; status: string; message: string }>("/v1/payroll/runs", command);
    return res.data;
  },

  getPayrollEntries: async (
    runId: string,
    page = 1,
    pageSize = 20,
    status?: string,
    search?: string
  ): Promise<PagedPayrollEntriesResponse> => {
    const params = new URLSearchParams({ page: page.toString(), pageSize: pageSize.toString() });
    if (status) params.append("status", status);
    if (search) params.append("search", search);

    const res = await apiClient.get<PagedPayrollEntriesResponse>(`/v1/payroll/runs/${runId}/entries?${params}`);
    return res.data;
  },

  overrideComponent: async (
    runId: string,
    entryId: string,
    componentId: string,
    request: OverrideComponentRequest
  ): Promise<PayrollEntryComponentResponse> => {
    const res = await apiClient.patch<PayrollEntryComponentResponse>(
      `/v1/payroll/runs/${runId}/entries/${entryId}/components/${componentId}`,
      request
    );
    return res.data;
  },

  approvePayrollRun: async (runId: string, request: ApproveRequest): Promise<PayrollRunResponse> => {
    const res = await apiClient.post<PayrollRunResponse>(`/v1/payroll/runs/${runId}/approve`, request);
    return res.data;
  },

  rejectPayrollRun: async (runId: string, request: RejectRequest): Promise<PayrollRunResponse> => {
    const res = await apiClient.post<PayrollRunResponse>(`/v1/payroll/runs/${runId}/reject`, request);
    return res.data;
  },

  getPendingApprovals: async (userRole = "HRManager"): Promise<PendingApprovalItemResponse[]> => {
    const res = await apiClient.get<PendingApprovalItemResponse[]>(`/v1/approvals/pending?userRole=${encodeURIComponent(userRole)}`);
    return res.data;
  },
};
