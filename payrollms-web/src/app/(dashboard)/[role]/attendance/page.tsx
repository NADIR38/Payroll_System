"use client";

import { useState } from "react";
import { attendanceApi } from "@/lib/api/attendance";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { Calendar, Plus, RefreshCw, UserCheck } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function AttendancePage() {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    externalEmployeeId: "",
    periodYear: new Date().getFullYear(),
    periodMonth: new Date().getMonth() + 1,
    workingDays: 22,
    absentDays: 0,
    lateDays: 0,
    lateMinutes: 0,
    overtimeHours: 0,
    halfDays: 0,
    holidays: 1,
    weekends: 8,
  });

  const handleSyncSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      const idempotencyKey = `SYNC-${formData.externalEmployeeId}-${formData.periodYear}-${String(formData.periodMonth).padStart(2, '0')}-${Date.now()}`;

      await attendanceApi.syncAttendance({
        ...formData,
        idempotencyKey,
        sourceSystem: "Manual Entry (Dashboard)",
      });

      toast.success("Attendance summary synced successfully!");
      setIsDialogOpen(false);
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.detail || error.response?.data?.message || "Failed to sync attendance.");
      } else {
        toast.error("An unexpected error occurred.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Attendance & Leave Summaries</h1>
          <p className="text-sm text-muted-foreground">Sync and manage monthly attendance summaries fed from external ERPs</p>
        </div>
        <Button onClick={() => setIsDialogOpen(true)} className="bg-primary hover:bg-primary/90 text-primary-foreground font-medium flex items-center gap-2">
          <Plus className="h-4 w-4" /> Sync Attendance Summary
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">Attendance Sync Engine</CardTitle>
            <Calendar className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">Monthly Rollups</div>
            <p className="text-xs text-muted-foreground mt-1">Idempotent ERP integration engine (PRD §13.1)</p>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">Formula Integration</CardTitle>
            <UserCheck className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">FormulaContext Ready</div>
            <p className="text-xs text-muted-foreground mt-1">WorkingDays, AbsentDays, LateMinutes mapped</p>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">Sync Policy</CardTitle>
            <RefreshCw className="h-4 w-4 text-blue-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">Freeze Protected</div>
            <p className="text-xs text-muted-foreground mt-1">Sync auto-blocked if PayrollCalendar is frozen</p>
          </CardContent>
        </Card>
      </div>

      <Card className="border-border">
        <CardHeader>
          <CardTitle>Attendance Ingestion Guide</CardTitle>
          <CardDescription>How PayrollMS processes biometric and attendance summaries from external ERPs</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4 text-sm">
          <p className="text-muted-foreground">
            PayrollMS strictly enforces the PRD §1.3 separation of concerns — raw biometric punch logs remain in the external ERP. Monthly summaries are ingested via <code>POST /api/v1/attendance/sync</code> and <code>POST /api/v1/leave/sync</code>.
          </p>
          <div className="p-4 rounded-lg bg-muted/40 border border-border">
            <h4 className="font-semibold text-foreground mb-2">Supported FormulaContext Variables:</h4>
            <ul className="list-disc list-inside space-y-1 text-muted-foreground">
              <li><code>WorkingDays</code> — Days present and worked</li>
              <li><code>AbsentDays</code> — Unexcused absence days</li>
              <li><code>LateDays</code> & <code>LateMinutes</code> — Late arrival details</li>
              <li><code>OvertimeHours</code> — Approved overtime hours</li>
              <li><code>PaidLeaveDays</code> & <code>UnpaidLeaveDays</code> — Leave summaries</li>
            </ul>
          </div>
        </CardContent>
      </Card>

      {/* Sync Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>Sync Employee Attendance</DialogTitle>
            <DialogDescription>Manually enter or test attendance summary sync for an employee period.</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSyncSubmit} className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="extId">External Employee ID</Label>
              <Input
                id="extId"
                placeholder="e.g. EXT-1001"
                value={formData.externalEmployeeId}
                onChange={(e) => setFormData({ ...formData, externalEmployeeId: e.target.value })}
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="year">Period Year</Label>
                <Input
                  id="year"
                  type="number"
                  value={formData.periodYear}
                  onChange={(e) => setFormData({ ...formData, periodYear: parseInt(e.target.value) || 2025 })}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="month">Period Month (1-12)</Label>
                <Input
                  id="month"
                  type="number"
                  min={1}
                  max={12}
                  value={formData.periodMonth}
                  onChange={(e) => setFormData({ ...formData, periodMonth: parseInt(e.target.value) || 1 })}
                  required
                />
              </div>
            </div>
            <div className="grid grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="workingDays">Working Days</Label>
                <Input
                  id="workingDays"
                  type="number"
                  value={formData.workingDays}
                  onChange={(e) => setFormData({ ...formData, workingDays: parseInt(e.target.value) || 0 })}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="absentDays">Absent Days</Label>
                <Input
                  id="absentDays"
                  type="number"
                  value={formData.absentDays}
                  onChange={(e) => setFormData({ ...formData, absentDays: parseInt(e.target.value) || 0 })}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lateDays">Late Days</Label>
                <Input
                  id="lateDays"
                  type="number"
                  value={formData.lateDays}
                  onChange={(e) => setFormData({ ...formData, lateDays: parseInt(e.target.value) || 0 })}
                  required
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="lateMinutes">Total Late Minutes</Label>
                <Input
                  id="lateMinutes"
                  type="number"
                  value={formData.lateMinutes}
                  onChange={(e) => setFormData({ ...formData, lateMinutes: parseInt(e.target.value) || 0 })}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="overtimeHours">Overtime Hours</Label>
                <Input
                  id="overtimeHours"
                  type="number"
                  step="0.5"
                  value={formData.overtimeHours}
                  onChange={(e) => setFormData({ ...formData, overtimeHours: parseFloat(e.target.value) || 0 })}
                />
              </div>
            </div>
            <DialogFooter className="pt-4">
              <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Syncing..." : "Sync Summary"}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
