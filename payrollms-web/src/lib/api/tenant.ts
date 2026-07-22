import { apiClient } from "./client";

// Types matching Backend DTOs
export interface CompanyDto {
  id: string;
  code: string;
  name: string;
  contactEmail?: string;
  contactPhone?: string;
  logoUrl?: string;
  address?: {
    street?: string;
    city?: string;
    state?: string;
    zipCode?: string;
    country?: string;
  };
  isActive: boolean;
}

export interface BranchDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  address?: string;
  isActive: boolean;
}

export interface DepartmentDto {
  id: string;
  companyId: string;
  branchId?: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface DesignationDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  grade?: string;
  isActive: boolean;
}

export interface CostCenterDto {
  id: string;
  companyId: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface CostCenter {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

export interface FinancialYear {
  id: string;
  label: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  isActive: boolean;
}

export interface FinancialYearDto {
  id: string;
  label: string;
  companyId: string;
  startDate: string;
  endDate: string;
  isCurrent: boolean;
  isActive?: boolean;
}

export interface PayrollCalendarDto {
  id: string;
  companyId: string;
  financialYearId: string;
  month: number;
  year: number;
  payrollFreezeDate?: string;
  paymentDate?: string;
  workingDays: number;
  status: string;
}

// API Calls
export const TenantApi = {
  // Companies (Endpoints 1-6)
  getCompanies: (): Promise<CompanyDto[]> => apiClient.get("/companies"),
  getCompanyById: (id: string): Promise<CompanyDto> => apiClient.get(`/companies/${id}`),
  createCompany: async (data: Record<string, unknown>): Promise<{ id: string }> => {
    const r = (await apiClient.post("/companies", data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  updateCompany: (id: string, data: Record<string, unknown>): Promise<void> => apiClient.put(`/companies/${id}`, data),
  activateCompany: (id: string): Promise<void> => apiClient.post(`/companies/${id}/activate`),
  deactivateCompany: (id: string): Promise<void> => apiClient.post(`/companies/${id}/deactivate`),

  // Branches (Endpoints 7-12)
  getBranches: (companyId?: string): Promise<BranchDto[]> => apiClient.get("/branches", { params: { companyId } }),
  getBranchById: (id: string): Promise<BranchDto> => apiClient.get(`/branches/${id}`),
  createBranch: async (data: Record<string, unknown>): Promise<{ id: string }> => {
    const r = (await apiClient.post("/branches", data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  updateBranch: (id: string, data: Record<string, unknown>): Promise<void> => apiClient.put(`/branches/${id}`, data),
  activateBranch: (id: string): Promise<void> => apiClient.post(`/branches/${id}/activate`),
  deactivateBranch: (id: string): Promise<void> => apiClient.post(`/branches/${id}/deactivate`),

  // Departments (Endpoints 13-18)
  getDepartments: (companyId?: string): Promise<DepartmentDto[]> => apiClient.get("/departments", { params: { companyId } }),
  getDepartmentById: (id: string): Promise<DepartmentDto> => apiClient.get(`/departments/${id}`),
  createDepartment: async (data: Record<string, unknown>): Promise<{ id: string }> => {
    const r = (await apiClient.post("/departments", data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  updateDepartment: (id: string, data: Record<string, unknown>): Promise<void> => apiClient.put(`/departments/${id}`, data),
  activateDepartment: (id: string): Promise<void> => apiClient.post(`/departments/${id}/activate`),
  deactivateDepartment: (id: string): Promise<void> => apiClient.post(`/departments/${id}/deactivate`),

  // Designations (Endpoints 19-24)
  getDesignations: (companyId?: string): Promise<DesignationDto[]> => apiClient.get("/designations", { params: { companyId } }),
  getDesignationById: (id: string): Promise<DesignationDto> => apiClient.get(`/designations/${id}`),
  createDesignation: async (data: Record<string, unknown>): Promise<{ id: string }> => {
    const r = (await apiClient.post("/designations", data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  updateDesignation: (id: string, data: Record<string, unknown>): Promise<void> => apiClient.put(`/designations/${id}`, data),
  activateDesignation: (id: string): Promise<void> => apiClient.post(`/designations/${id}/activate`),
  deactivateDesignation: (id: string): Promise<void> => apiClient.post(`/designations/${id}/deactivate`),

  // Cost Centers
  getCostCenters: async (companyId?: string): Promise<CostCenter[]> => {
    return apiClient.get('/cost-centers', { params: { companyId } });
  },
  createCostCenter: async (data: Partial<CostCenter>): Promise<{id: string}> => {
    const r = (await apiClient.post('/cost-centers', data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  updateCostCenter: async (id: string, data: Partial<CostCenter>): Promise<void> => {
    return apiClient.put(`/cost-centers/${id}`, data);
  },
  activateCostCenter: (id: string): Promise<void> => apiClient.post(`/cost-centers/${id}/activate`),
  deactivateCostCenter: (id: string): Promise<void> => apiClient.post(`/cost-centers/${id}/deactivate`),

  // Financial Years
  getFinancialYears: async (): Promise<FinancialYear[]> => {
    return apiClient.get('/financial-years');
  },
  createFinancialYear: async (data: Partial<FinancialYear>): Promise<{id: string}> => {
    const r = (await apiClient.post('/financial-years', data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  getFinancialYearById: (id: string): Promise<FinancialYearDto> => apiClient.get(`/financial-years/${id}`),
  markFinancialYearCurrent: (id: string): Promise<void> => apiClient.post(`/financial-years/${id}/mark-current`),

  // Payroll Calendars (Endpoints 35-40)
  getPayrollCalendars: (financialYearId?: string): Promise<PayrollCalendarDto[]> => apiClient.get("/payroll-calendars", { params: { financialYearId } }),
  getPayrollCalendarById: (id: string): Promise<PayrollCalendarDto> => apiClient.get(`/payroll-calendars/${id}`),
  createPayrollCalendar: async (data: Record<string, unknown>): Promise<{ id: string }> => {
    const r = (await apiClient.post("/payroll-calendars", data)) as { id: string } | string;
    return typeof r === "string" ? { id: r } : r;
  },
  freezePayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/freeze`),
  closePayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/close`),
  reopenPayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/reopen`),
};
