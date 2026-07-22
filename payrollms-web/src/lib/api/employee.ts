import { apiClient } from "./client";

export interface EmployeeBankAccountResponse {
  id: string;
  employeePayrollProfileId: string;
  bankName: string;
  accountTitle: string;
  accountNumber: string;
  iban: string;
  branchCode: string | null;
  isPrimary: boolean;
  isActive: boolean;
}

export interface EmployeeProfileHistoryResponse {
  id: string;
  employeePayrollProfileId: string;
  externalEmployeeId: string;
  employeeCode: string;
  fullName: string;
  salaryStructureId: string;
  baseSalary: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  changedBy: string | null;
  changeReason: string | null;
}

export interface EmployeeProfileResponse {
  id: string;
  companyId: string;
  externalEmployeeId: string;
  employeeCode: string;
  fullName: string;
  branchId: string;
  departmentId: string;
  designationId: string;
  costCenterId: string | null;
  salaryStructureId: string;
  baseSalary: number;
  attendanceDeductionOptIn: boolean;
  joiningDate: string;
  leavingDate: string | null;
  status: string;
  effectiveFrom: string;
  createdAt: string;
  bankAccounts: EmployeeBankAccountResponse[];
}

export interface SyncEmployeeCommand {
  companyId?: string; // Optional since backend pulls from tenant header if empty
  externalEmployeeId: string;
  employeeCode: string;
  fullName: string;
  branchId: string;
  departmentId: string;
  designationId: string;
  costCenterId?: string | null;
  salaryStructureId: string;
  baseSalary: number;
  joiningDate: string; // YYYY-MM-DD
  attendanceDeductionOptIn: boolean;
}

export interface UpdateEmployeeProfileCommand {
  id: string;
  fullName: string;
  branchId: string;
  departmentId: string;
  designationId: string;
  costCenterId?: string | null;
  salaryStructureId: string;
  baseSalary: number;
  joiningDate: string;
  leavingDate?: string | null;
  attendanceDeductionOptIn: boolean;
  status: string;
  changedBy: string;
  changeReason: string;
}

export interface AddEmployeeBankAccountCommand {
  employeePayrollProfileId: string;
  bankName: string;
  accountTitle: string;
  accountNumber: string;
  iban: string;
  branchCode?: string | null;
  isPrimary: boolean;
}

export interface TerminateEmployeeCommand {
  id: string;
  changedBy: string;
}

export const EmployeeApi = {
  getEmployees: async (params?: {
    searchTerm?: string;
    statusFilter?: string;
    branchId?: string;
    departmentId?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<EmployeeProfileResponse[]> => {
    return apiClient.get("/employees", { params });
  },

  getEmployeeById: async (id: string): Promise<EmployeeProfileResponse> => {
    return apiClient.get(`/employees/${id}`);
  },

  getEmployeeHistory: async (id: string): Promise<EmployeeProfileHistoryResponse[]> => {
    return apiClient.get(`/employees/${id}/history`);
  },

  syncEmployee: async (command: SyncEmployeeCommand): Promise<{ profileId: string, action: string }> => {
    const res = (await apiClient.post("/employees", command)) as { profileId: string; action: string } | string;
    return typeof res === "string" ? { profileId: res, action: "Created" } : res;
  },

  updateProfile: async (id: string, command: UpdateEmployeeProfileCommand): Promise<void> => {
    return apiClient.put(`/employees/${id}/profile`, command);
  },

  terminateEmployee: async (id: string, command: TerminateEmployeeCommand): Promise<void> => {
    return apiClient.post(`/employees/${id}/terminate`, command);
  },

  addBankAccount: async (id: string, command: AddEmployeeBankAccountCommand): Promise<string> => {
    return apiClient.post(`/employees/${id}/bank-accounts`, command);
  },

  setPrimaryBankAccount: async (id: string, accountId: string): Promise<void> => {
    return apiClient.put(`/employees/${id}/bank-accounts/${accountId}/primary`);
  }
};
