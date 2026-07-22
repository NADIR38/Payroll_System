import { apiClient } from "./client";

export interface AttendanceSummaryResponse {
  id: string;
  companyId: string;
  externalEmployeeId: string;
  periodYear: number;
  periodMonth: number;
  workingDays: number;
  absentDays: number;
  lateDays: number;
  lateMinutes: number;
  overtimeHours: number;
  halfDays: number;
  holidays: number;
  weekends: number;
  syncedAt: string;
  sourceSystem: string;
  idempotencyKey: string;
}

export interface LeaveSummaryResponse {
  id: string;
  companyId: string;
  externalEmployeeId: string;
  periodYear: number;
  periodMonth: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  medicalLeaveDays: number;
  casualLeaveDays: number;
  halfDays: number;
  syncedAt: string;
  idempotencyKey: string;
}

export interface SyncAttendanceCommand {
  companyId?: string;
  externalEmployeeId: string;
  periodYear: number;
  periodMonth: number;
  workingDays: number;
  absentDays: number;
  lateDays: number;
  lateMinutes: number;
  overtimeHours: number;
  halfDays: number;
  holidays: number;
  weekends: number;
  idempotencyKey: string;
  sourceSystem?: string;
}

export interface SyncLeaveCommand {
  companyId?: string;
  externalEmployeeId: string;
  periodYear: number;
  periodMonth: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  medicalLeaveDays: number;
  casualLeaveDays: number;
  halfDays: number;
  idempotencyKey: string;
}

export const attendanceApi = {
  syncAttendance: async (command: SyncAttendanceCommand): Promise<AttendanceSummaryResponse> => {
    const res = await apiClient.post<AttendanceSummaryResponse>("/v1/attendance/sync", command);
    return res.data;
  },

  syncLeave: async (command: SyncLeaveCommand): Promise<LeaveSummaryResponse> => {
    const res = await apiClient.post<LeaveSummaryResponse>("/v1/leave/sync", command);
    return res.data;
  },
};
