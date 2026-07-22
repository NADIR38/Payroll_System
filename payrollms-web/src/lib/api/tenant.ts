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
  getCompanies: (): Promise<CompanyDto[]> => apiClient.get("/companies") as any,
  getCompanyById: (id: string): Promise<CompanyDto> => apiClient.get(`/companies/${id}`) as any,
  createCompany: async (data: any): Promise<{ id: string }> => { const r = (await apiClient.post("/companies", data)) as any; return typeof r === "string" ? { id: r } : r; },
  updateCompany: (id: string, data: any): Promise<void> => apiClient.put(`/companies/${id}`, data) as any,
  activateCompany: (id: string): Promise<void> => apiClient.post(`/companies/${id}/activate`) as any,
  deactivateCompany: (id: string): Promise<void> => apiClient.post(`/companies/${id}/deactivate`) as any,

  // Branches (Endpoints 7-12)
  getBranches: (companyId?: string): Promise<BranchDto[]> => apiClient.get("/branches", { params: { companyId } }) as any,
  getBranchById: (id: string): Promise<BranchDto> => apiClient.get(`/branches/${id}`) as any,
  createBranch: async (data: any): Promise<{ id: string }> => { const r = (await apiClient.post("/branches", data)) as any; return typeof r === "string" ? { id: r } : r; },
  updateBranch: (id: string, data: any): Promise<void> => apiClient.put(`/branches/${id}`, data) as any,
  activateBranch: (id: string): Promise<void> => apiClient.post(`/branches/${id}/activate`) as any,
  deactivateBranch: (id: string): Promise<void> => apiClient.post(`/branches/${id}/deactivate`) as any,

  // Departments (Endpoints 13-18)
  getDepartments: (companyId?: string): Promise<DepartmentDto[]> => apiClient.get("/departments", { params: { companyId } }) as any,
  getDepartmentById: (id: string): Promise<DepartmentDto> => apiClient.get(`/departments/${id}`) as any,
  createDepartment: async (data: any): Promise<{ id: string }> => { const r = (await apiClient.post("/departments", data)) as any; return typeof r === "string" ? { id: r } : r; },
  updateDepartment: (id: string, data: any): Promise<void> => apiClient.put(`/departments/${id}`, data) as any,
  activateDepartment: (id: string): Promise<void> => apiClient.post(`/departments/${id}/activate`) as any,
  deactivateDepartment: (id: string): Promise<void> => apiClient.post(`/departments/${id}/deactivate`) as any,

  // Designations (Endpoints 19-24)
  getDesignations: (companyId?: string): Promise<DesignationDto[]> => apiClient.get("/designations", { params: { companyId } }) as any,
  getDesignationById: (id: string): Promise<DesignationDto> => apiClient.get(`/designations/${id}`) as any,
  createDesignation: async (data: any): Promise<{ id: string }> => { const r = (await apiClient.post("/designations", data)) as any; return typeof r === "string" ? { id: r } : r; },
  updateDesignation: (id: string, data: any): Promise<void> => apiClient.put(`/designations/${id}`, data) as any,
  activateDesignation: (id: string): Promise<void> => apiClient.post(`/designations/${id}/activate`) as any,
  deactivateDesignation: (id: string): Promise<void> => apiClient.post(`/designations/${id}/deactivate`) as any,

  // Cost Centers
  getCostCenters: async (companyId?: string): Promise<CostCenter[]> => {
    return apiClient.get('/cost-centers', { params: { companyId } }) as any;
  },
  createCostCenter: async (data: Partial<CostCenter>): Promise<{id: string}> => {
    const r = (await apiClient.post('/cost-centers', data)) as any;
    return typeof r === "string" ? { id: r } : r;
  },
  updateCostCenter: async (id: string, data: Partial<CostCenter>): Promise<void> => {
    return apiClient.put(`/cost-centers/${id}`, data) as any;
  },
  activateCostCenter: (id: string): Promise<void> => apiClient.post(`/cost-centers/${id}/activate`) as any,
  deactivateCostCenter: (id: string): Promise<void> => apiClient.post(`/cost-centers/${id}/deactivate`) as any,

  // Financial Years
  getFinancialYears: async (): Promise<FinancialYear[]> => {
    return apiClient.get('/financial-years') as any;
  },
  createFinancialYear: async (data: Partial<FinancialYear>): Promise<{id: string}> => {
    const r = (await apiClient.post('/financial-years', data)) as any;
    return typeof r === "string" ? { id: r } : r;
  },
  getFinancialYearById: (id: string): Promise<FinancialYearDto> => apiClient.get(`/financial-years/${id}`) as any,
  markFinancialYearCurrent: (id: string): Promise<void> => apiClient.post(`/financial-years/${id}/mark-current`) as any,

  // Payroll Calendars (Endpoints 35-40)
  getPayrollCalendars: (financialYearId?: string): Promise<PayrollCalendarDto[]> => apiClient.get("/payroll-calendars", { params: { financialYearId } }) as any,
  getPayrollCalendarById: (id: string): Promise<PayrollCalendarDto> => apiClient.get(`/payroll-calendars/${id}`) as any,
  createPayrollCalendar: async (data: any): Promise<{ id: string }> => { const r = (await apiClient.post("/payroll-calendars", data)) as any; return typeof r === "string" ? { id: r } : r; },
  freezePayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/freeze`) as any,
  closePayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/close`) as any,
  reopenPayrollCalendar: (id: string): Promise<void> => apiClient.post(`/payroll-calendars/${id}/reopen`) as any,
};
