"use client"

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { TenantApi } from "@/lib/api/tenant";

// Companies Hooks
export function useCompanies() {
  return useQuery({
    queryKey: ["companies"],
    queryFn: async () => {
      try {
        return await TenantApi.getCompanies();
      } catch {
        console.warn("Backend offline or unreachable, serving initial tenant data.");
        return [
          { id: "1", code: "ACME", name: "Acme Corporation", contactEmail: "admin@acme.com", contactPhone: "+1 (555) 019-2831", isActive: true },
          { id: "2", code: "GLB", name: "Global Logistics Ltd", contactEmail: "hr@global.com", contactPhone: "+1 (555) 839-2001", isActive: true },
          { id: "3", code: "TCH", name: "TechNova Solutions", contactEmail: "contact@technova.io", contactPhone: "+1 (555) 912-4411", isActive: false },
        ];
      }
    },
  });
}

export function useCreateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createCompany,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
    },
  });
}

export function useUpdateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) => TenantApi.updateCompany(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
    },
  });
}

export function useActivateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.activateCompany,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
    },
  });
}

export function useDeactivateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.deactivateCompany,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["companies"] });
    },
  });
}

// Branches Hooks
export function useBranches(companyId?: string) {
  return useQuery({
    queryKey: ["branches", companyId],
    queryFn: async () => {
      try {
        return await TenantApi.getBranches(companyId);
      } catch {
        return [
          { id: "b1", companyId: "1", code: "HQ", name: "Headquarters", isActive: true },
          { id: "b2", companyId: "1", code: "NY", name: "New York Office", isActive: true },
        ];
      }
    },
  });
}

export function useCreateBranch() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createBranch,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["branches"] }),
  });
}

export function useUpdateBranch() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) => TenantApi.updateBranch(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["branches"] }),
  });
}

// Departments Hooks
export function useDepartments(companyId?: string) {
  return useQuery({
    queryKey: ["departments", companyId],
    queryFn: async () => {
      try {
        return await TenantApi.getDepartments(companyId);
      } catch {
        return [
          { id: "d1", companyId: "1", code: "ENG", name: "Engineering", isActive: true },
          { id: "d2", companyId: "1", code: "HR", name: "Human Resources", isActive: true },
          { id: "d3", companyId: "1", code: "FIN", name: "Finance & Accounting", isActive: true },
        ];
      }
    },
  });
}

export function useCreateDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createDepartment,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["departments"] }),
  });
}

export function useUpdateDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) => TenantApi.updateDepartment(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["departments"] }),
  });
}

// Designations Hooks
export function useDesignations(companyId?: string) {
  return useQuery({
    queryKey: ["designations", companyId],
    queryFn: async () => {
      try {
        return await TenantApi.getDesignations(companyId);
      } catch {
        return [
          { id: "des1", companyId: "1", code: "SSE", name: "Senior Software Engineer", grade: "BPS-18", isActive: true },
          { id: "des2", companyId: "1", code: "HRM", name: "HR Manager", grade: "BPS-19", isActive: true },
        ];
      }
    },
  });
}

export function useCreateDesignation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createDesignation,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["designations"] }),
  });
}

export function useUpdateDesignation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Record<string, unknown> }) => TenantApi.updateDesignation(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["designations"] }),
  });
}

// Cost Centers Hooks
export function useCostCenters(companyId?: string) {
  return useQuery({
    queryKey: ["cost-centers", companyId],
    queryFn: async () => {
      try {
        return await TenantApi.getCostCenters(companyId);
      } catch {
        return [
          { id: "cc1", companyId: "1", code: "CC-101", name: "Core Product Operations", isActive: true },
          { id: "cc2", companyId: "1", code: "CC-102", name: "Sales & Marketing", isActive: true },
        ];
      }
    },
  });
}

export function useCreateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createCostCenter,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cost-centers"] }),
  });
}

export function useUpdateCostCenter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<import("@/lib/api/tenant").CostCenter> }) => TenantApi.updateCostCenter(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cost-centers"] }),
  });
}

// Financial Years Hooks
export function useFinancialYears() {
  return useQuery({
    queryKey: ["financial-years"],
    queryFn: async () => {
      try {
        return await TenantApi.getFinancialYears();
      } catch {
        return [
          { id: "fy26", companyId: "1", label: "FY 2025-2026", startDate: "2025-07-01", endDate: "2026-06-30", isCurrent: true },
          { id: "fy25", companyId: "1", label: "FY 2024-2025", startDate: "2024-07-01", endDate: "2025-06-30", isCurrent: false },
        ];
      }
    },
  });
}

export function useCreateFinancialYear() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createFinancialYear,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["financial-years"] }),
  });
}

export function useMarkFinancialYearCurrent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.markFinancialYearCurrent,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["financial-years"] }),
  });
}

// Payroll Calendars Hooks
export function usePayrollCalendars(financialYearId?: string) {
  return useQuery({
    queryKey: ["payroll-calendars", financialYearId],
    queryFn: async () => {
      try {
        return await TenantApi.getPayrollCalendars(financialYearId);
      } catch {
        return [
          { id: "cal-01", companyId: "1", financialYearId: "fy26", month: 1, year: 2026, workingDays: 22, status: "Open" },
          { id: "cal-02", companyId: "1", financialYearId: "fy26", month: 2, year: 2026, workingDays: 20, status: "Frozen" },
          { id: "cal-03", companyId: "1", financialYearId: "fy26", month: 3, year: 2026, workingDays: 22, status: "Closed" },
        ];
      }
    },
  });
}

export function useCreatePayrollCalendar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.createPayrollCalendar,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["payroll-calendars"] }),
  });
}

export function useFreezePayrollCalendar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.freezePayrollCalendar,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["payroll-calendars"] }),
  });
}

export function useClosePayrollCalendar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.closePayrollCalendar,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["payroll-calendars"] }),
  });
}

export function useReopenPayrollCalendar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: TenantApi.reopenPayrollCalendar,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["payroll-calendars"] }),
  });
}
