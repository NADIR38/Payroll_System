"use client";

import { useEffect, useState, useCallback } from "react";
import { useParams } from "next/navigation";
import { payrollApi, PayrollRunResponse, PayrollEntryResponse, PayrollEntryComponentResponse } from "@/lib/api/payroll";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { CheckCircle, XCircle, Edit, Users } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function PayrollRunDetailPage() {
  const params = useParams();
  const runId = params.id as string;

  const [run, setRun] = useState<PayrollRunResponse | null>(null);
  const [entries, setEntries] = useState<PayrollEntryResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Selected Entry for Side Panel Sheet
  const [selectedEntry, setSelectedEntry] = useState<PayrollEntryResponse | null>(null);

  // Override Component Modal
  const [editingComponent, setEditingComponent] = useState<PayrollEntryComponentResponse | null>(null);
  const [overrideAmount, setOverrideAmount] = useState<number>(0);
  const [overrideReason, setOverrideReason] = useState("");
  const [isSubmittingOverride, setIsSubmittingOverride] = useState(false);

  // Approval Modal
  const [isApproveOpen, setIsApproveOpen] = useState(false);
  const [approveComments, setApproveComments] = useState("");
  const [isRejectOpen, setIsRejectOpen] = useState(false);
  const [rejectComments, setRejectComments] = useState("");
  const [isSubmittingApproval, setIsSubmittingApproval] = useState(false);

  const fetchRunDetail = useCallback(async () => {
    try {
      const runData = await payrollApi.getPayrollRunById(runId);
      setRun(runData);

      const entriesData = await payrollApi.getPayrollEntries(runId, 1, 100);
      setEntries(entriesData.items);
    } catch (error) {
      console.error("Failed to load run details:", error);
    } finally {
      setIsLoading(false);
    }
  }, [runId]);

  useEffect(() => {
    if (!runId) return;
    let isMounted = true;
    const loadData = async () => {
      try {
        const runData = await payrollApi.getPayrollRunById(runId);
        const entriesData = await payrollApi.getPayrollEntries(runId, 1, 100);
        if (isMounted) {
          setRun(runData);
          setEntries(entriesData.items);
        }
      } catch (error) {
        console.error("Failed to load run details:", error);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };
    loadData();
    return () => { isMounted = false; };
  }, [runId]);

  const handleOverrideSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedEntry || !editingComponent) return;

    try {
      setIsSubmittingOverride(true);
      await payrollApi.overrideComponent(runId, selectedEntry.id, editingComponent.id, {
        overrideAmount,
        overriddenBy: "HR Officer",
        reason: overrideReason,
      });

      toast.success("Component amount manually overridden and audited!");
      setEditingComponent(null);
      fetchRunDetail();
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.detail || error.response?.data?.message || "Failed to override component.");
      } else {
        toast.error("An unexpected error occurred.");
      }
    } finally {
      setIsSubmittingOverride(false);
    }
  };

  const handleApprove = async () => {
    try {
      setIsSubmittingApproval(true);
      await payrollApi.approvePayrollRun(runId, {
        actorUserId: "USER-1",
        actorName: "HR Manager",
        actorRole: "HRManager",
        comments: approveComments,
      });

      toast.success("Payroll run approved successfully!");
      setIsApproveOpen(false);
      fetchRunDetail();
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.detail || error.response?.data?.message || "Failed to approve payroll run.");
      } else {
        toast.error("An unexpected error occurred.");
      }
    } finally {
      setIsSubmittingApproval(false);
    }
  };

  const handleReject = async () => {
    try {
      setIsSubmittingApproval(true);
      await payrollApi.rejectPayrollRun(runId, {
        actorUserId: "USER-1",
        actorName: "HR Manager",
        actorRole: "HRManager",
        comments: rejectComments,
      });

      toast.success("Payroll run rejected and returned to GENERATED status.");
      setIsRejectOpen(false);
      fetchRunDetail();
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.detail || error.response?.data?.message || "Failed to reject payroll run.");
      } else {
        toast.error("An unexpected error occurred.");
      }
    } finally {
      setIsSubmittingApproval(false);
    }
  };

  if (isLoading || !run) {
    return <div className="text-center py-12 text-muted-foreground">Loading payroll run detail...</div>;
  }

  const canModify = run.status === "Generated" || run.status === "UnderReview";

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{`Payroll Run: ${run.periodYear}-${String(run.periodMonth).padStart(2, "0")}`}</h1>
          <p className="text-sm text-muted-foreground">{`Run Type: ${run.runType} | Version: ${run.runVersion}`}</p>
        </div>
        {canModify && (
          <div className="flex items-center gap-2">
            <Button onClick={() => setIsRejectOpen(true)} variant="outline" className="text-destructive border-destructive/20 hover:bg-destructive/10">
              <XCircle className="h-4 w-4 mr-1" /> Reject
            </Button>
            <Button onClick={() => setIsApproveOpen(true)} className="bg-emerald-600 hover:bg-emerald-700 text-white">
              <CheckCircle className="h-4 w-4 mr-1" /> Approve Step
            </Button>
          </div>
        )}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs text-muted-foreground">Status</CardTitle>
          </CardHeader>
          <CardContent>
            <Badge variant="blue" className="text-sm font-semibold">{run.status}</Badge>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs text-muted-foreground">Total Employees</CardTitle>
          </CardHeader>
          <CardContent className="flex items-center gap-2">
            <Users className="h-4 w-4 text-primary" />
            <span className="text-xl font-bold">{run.totalEmployees}</span>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs text-muted-foreground">Total Gross</CardTitle>
          </CardHeader>
          <CardContent>
            <span className="text-xl font-bold">PKR {run.totalGross.toLocaleString()}</span>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-xs text-muted-foreground">Total Net Payable</CardTitle>
          </CardHeader>
          <CardContent>
            <span className="text-xl font-bold text-emerald-400">PKR {run.totalNet.toLocaleString()}</span>
          </CardContent>
        </Card>
      </div>

      {/* Employees Table */}
      <Card className="border-border">
        <CardHeader>
          <CardTitle>Calculated Employee Entries ({entries.length})</CardTitle>
          <CardDescription>Click any entry to inspect components or perform manual overrides</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="text-xs text-muted-foreground uppercase bg-muted/40 border-b border-border">
                <tr>
                  <th className="px-4 py-3">Employee</th>
                  <th className="px-4 py-3">Dept / Desg</th>
                  <th className="px-4 py-3">Working / Absent</th>
                  <th className="px-4 py-3">Gross Salary</th>
                  <th className="px-4 py-3">Deductions</th>
                  <th className="px-4 py-3">Net Salary</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {entries.map((entry) => (
                  <tr key={entry.id} className="hover:bg-muted/20 cursor-pointer" onClick={() => setSelectedEntry(entry)}>
                    <td className="px-4 py-3 font-medium">
                      {entry.employeeName}
                      <div className="text-xs text-muted-foreground">{entry.employeeCode}</div>
                    </td>
                    <td className="px-4 py-3">
                      {entry.departmentName}
                      <div className="text-xs text-muted-foreground">{entry.designationName}</div>
                    </td>
                    <td className="px-4 py-3">
                      {entry.workingDays}d / {entry.absentDays}d
                    </td>
                    <td className="px-4 py-3 font-medium">PKR {entry.grossSalary.toLocaleString()}</td>
                    <td className="px-4 py-3 text-rose-400">PKR {entry.totalDeductions.toLocaleString()}</td>
                    <td className="px-4 py-3 font-semibold text-emerald-400">PKR {entry.netSalary.toLocaleString()}</td>
                    <td className="px-4 py-3">
                      <Badge variant="default">{entry.status}</Badge>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Button variant="ghost" size="sm">Inspect</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Entry Components Modal */}
      <Dialog open={!!selectedEntry} onOpenChange={() => setSelectedEntry(null)}>
        <DialogContent className="sm:max-w-[540px]">
          <DialogHeader>
            <DialogTitle>{selectedEntry?.employeeName} ({selectedEntry?.employeeCode})</DialogTitle>
            <DialogDescription>Component calculation breakdown and formula audit</DialogDescription>
          </DialogHeader>

          {selectedEntry && (
            <div className="space-y-6 mt-2">
              <div className="p-4 rounded-lg bg-muted/40 space-y-2 border border-border">
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Base Salary:</span>
                  <span className="font-semibold">PKR {selectedEntry.baseSalary.toLocaleString()}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Gross Salary:</span>
                  <span className="font-semibold">PKR {selectedEntry.grossSalary.toLocaleString()}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Net Salary:</span>
                  <span className="font-semibold text-emerald-400">PKR {selectedEntry.netSalary.toLocaleString()}</span>
                </div>
              </div>

              <div className="space-y-3">
                <h4 className="font-semibold text-sm text-foreground">Salary Components ({selectedEntry.components.length})</h4>
                {selectedEntry.components.map((comp) => (
                  <div key={comp.id} className="p-3 rounded-lg border border-border flex items-center justify-between">
                    <div>
                      <div className="font-medium text-sm flex items-center gap-2">
                        {comp.componentName} ({comp.componentCode})
                        {comp.isManualOverride && <Badge variant="destructive" className="text-[10px]">Overridden</Badge>}
                      </div>
                      <div className="text-xs text-muted-foreground mt-1 font-mono">Formula: {comp.formulaUsed}</div>
                    </div>

                    <div className="text-right space-y-1">
                      <div className="font-semibold text-sm">PKR {comp.calculatedAmount.toLocaleString()}</div>
                      {canModify && (
                        <Button
                          variant="ghost"
                          size="sm"
                          className="h-6 text-xs text-primary flex items-center gap-1"
                          onClick={() => {
                            setEditingComponent(comp);
                            setOverrideAmount(comp.calculatedAmount);
                            setOverrideReason("");
                          }}
                        >
                          <Edit className="h-3 w-3" /> Override
                        </Button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Override Component Modal */}
      <Dialog open={!!editingComponent} onOpenChange={() => setEditingComponent(null)}>
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Override Component Amount</DialogTitle>
            <DialogDescription>Manually edit {editingComponent?.componentName} for {selectedEntry?.employeeName}.</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleOverrideSubmit} className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="amount">New Calculated Amount (PKR)</Label>
              <Input
                id="amount"
                type="number"
                value={overrideAmount}
                onChange={(e) => setOverrideAmount(parseFloat(e.target.value) || 0)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="reason">Audit Reason (Mandatory per PRD §24.3)</Label>
              <Input
                id="reason"
                placeholder="Reason for manual adjustment..."
                value={overrideReason}
                onChange={(e) => setOverrideReason(e.target.value)}
                required
              />
            </div>
            <DialogFooter className="pt-4">
              <Button type="button" variant="outline" onClick={() => setEditingComponent(null)}>Cancel</Button>
              <Button type="submit" disabled={isSubmittingOverride}>{isSubmittingOverride ? "Saving..." : "Apply Override"}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Approve Modal */}
      <Dialog open={isApproveOpen} onOpenChange={setIsApproveOpen}>
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Approve Payroll Step</DialogTitle>
            <DialogDescription>Advance payroll run workflow step.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="appComments">Approval Comments (Optional)</Label>
              <Input
                id="appComments"
                placeholder="Sign-off notes..."
                value={approveComments}
                onChange={(e) => setApproveComments(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsApproveOpen(false)}>Cancel</Button>
            <Button onClick={handleApprove} disabled={isSubmittingApproval} className="bg-emerald-600 hover:bg-emerald-700 text-white">Confirm Approval</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Reject Modal */}
      <Dialog open={isRejectOpen} onOpenChange={setIsRejectOpen}>
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Reject Payroll Step</DialogTitle>
            <DialogDescription>Return payroll run to GENERATED status for revisions.</DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="rejComments">Rejection Reason (Mandatory per PRD §16.5)</Label>
              <Input
                id="rejComments"
                placeholder="State reason for rejection..."
                value={rejectComments}
                onChange={(e) => setRejectComments(e.target.value)}
                required
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsRejectOpen(false)}>Cancel</Button>
            <Button onClick={handleReject} disabled={isSubmittingApproval || !rejectComments} variant="destructive">Confirm Rejection</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
