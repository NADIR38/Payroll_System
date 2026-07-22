import { apiClient } from "./client";

export interface SalaryComponentResponse {
  id: string;
  name: string;
  code: string;
  type: string;
  isTaxable: boolean;
  isCustom: boolean;
  isActive: boolean;
}

export interface SalaryStructureResponse {
  id: string;
  companyId: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  components: SalaryStructureComponentResponse[];
}

export interface SalaryStructureComponentResponse {
  id: string;
  salaryStructureId: string;
  salaryComponentId: string;
  componentCode: string;
  componentName: string;
  componentType: string;
  calculationMethod: string;
  formulaExpression: string | null;
  sequence: number;
  fixedAmount: number | null;
  isActive: boolean;
  allowanceRule?: any;
  deductionRule?: any;
}

export interface CreateSalaryComponentCommand {
  name: string;
  code: string;
  type: string;
  isTaxable: boolean;
}

export interface UpdateSalaryComponentCommand {
  id: string;
  name: string;
  isTaxable: boolean;
}

export interface CreateSalaryStructureCommand {
  name: string;
  code: string;
  effectiveFrom: string;
  effectiveTo?: string;
  description?: string;
}

export interface AddComponentToStructureCommand {
  salaryStructureId: string;
  salaryComponentId: string;
  formulaExpression?: string | null;
  fixedAmount?: number | null;
  sequence: number;
}

export interface UpdateComponentInStructureCommand {
  salaryStructureId: string;
  structureComponentId: string;
  formulaExpression?: string | null;
  fixedAmount?: number | null;
  sequence: number;
}

export interface SetAllowanceRuleCommand {
  salaryStructureId: string;
  salaryComponentId: string;
  calculationType: string;
  formula?: string | null;
  fixedAmount?: number | null;
}

export interface SetDeductionRuleCommand {
  salaryStructureId: string;
  salaryComponentId: string;
  calculationType: string;
  formula?: string | null;
  fixedAmount?: number | null;
}

export interface ValidateFormulaCommand {
  formula: string;
}

export interface ValidateFormulaResponse {
  isValid: boolean;
  errorMessage?: string;
  evaluatedResult?: number;
}

export const SalaryApi = {
  // Components
  getComponents: async (): Promise<SalaryComponentResponse[]> => {
    return apiClient.get("/salary-components");
  },
  getComponentById: async (id: string): Promise<SalaryComponentResponse> => {
    return apiClient.get(`/salary-components/${id}`);
  },
  createComponent: async (data: CreateSalaryComponentCommand): Promise<{ id: string }> => {
    const res = (await apiClient.post("/salary-components", data)) as any;
    return typeof res === "string" ? { id: res } : res;
  },
  updateComponent: async (id: string, data: UpdateSalaryComponentCommand): Promise<void> => {
    return apiClient.put(`/salary-components/${id}`, data) as any;
  },
  deactivateComponent: async (id: string): Promise<void> => {
    return apiClient.delete(`/salary-components/${id}`) as any;
  },
  seedComponents: async (): Promise<void> => {
    return apiClient.post("/salary-components/seed") as any;
  },

  // Structures
  getStructures: async (): Promise<SalaryStructureResponse[]> => {
    return apiClient.get("/salary-structures") as any;
  },
  getStructureById: async (id: string): Promise<SalaryStructureResponse> => {
    return apiClient.get(`/salary-structures/${id}`) as any;
  },
  createStructure: async (data: CreateSalaryStructureCommand): Promise<{ id: string }> => {
    const res = (await apiClient.post("/salary-structures", data)) as any;
    return typeof res === "string" ? { id: res } : res;
  },
  addComponent: async (id: string, data: AddComponentToStructureCommand): Promise<void> => {
    return apiClient.post(`/salary-structures/${id}/components`, data);
  },
  updateComponentInStructure: async (id: string, componentId: string, data: UpdateComponentInStructureCommand): Promise<void> => {
    return apiClient.put(`/salary-structures/${id}/components/${componentId}`, data);
  },
  removeComponent: async (id: string, componentId: string): Promise<void> => {
    return apiClient.delete(`/salary-structures/${id}/components/${componentId}`);
  },
  setAllowanceRule: async (id: string, componentId: string, data: SetAllowanceRuleCommand): Promise<void> => {
    return apiClient.put(`/salary-structures/${id}/components/${componentId}/allowance-rule`, data);
  },
  setDeductionRule: async (id: string, componentId: string, data: SetDeductionRuleCommand): Promise<void> => {
    return apiClient.put(`/salary-structures/${id}/components/${componentId}/deduction-rule`, data);
  },
  deactivateStructure: async (id: string): Promise<void> => {
    return apiClient.delete(`/salary-structures/${id}`);
  },
  validateFormula: async (data: ValidateFormulaCommand): Promise<ValidateFormulaResponse> => {
    return apiClient.post("/salary-structures/validate-formula", data);
  }
};
