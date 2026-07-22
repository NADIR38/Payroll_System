"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { payrollApi, PayrollRunResponse } from "@/lib/api/payroll";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { SectionHeader } from "@/components/ui/section-header";
import { Plus, Play, RefreshCw, Eye, CheckCircle2, AlertCircle } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function PayrollRunsPage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;

  const [runs, setRuns] = useState<PayrollRunResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState({
    financialYearId: "00000000-0000-0000-0000-000000000000", // Will be filled or prompted
    periodYear: new Date().getFullYear(),
    periodMonth: new Date().getMonth() + 1,
    remarks: "Regular monthly payroll run",
  });

  useEffect(() => {
    fetchRuns();
  }, []);

  const fetchRuns = async () => {
    try {
      setIsLoading(true);
      const data = await payrollApi.getPayrollRuns(1, 50);
      setRuns(data.items);
    } catch (error) {
      console.error("Failed to load payroll runs:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateRun = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      const res = await payrollApi.createPayrollRun({
        financialYearId: formData.financialYearId,
        periodYear: formData.periodYear,
        periodMonth: formData.periodMonth,
        remarks: formData.remarks,
        runType: "Regular",
      });

      toast.success(res.message || "Payroll run initiated!");
      setIsDialogOpen(false);
      fetchRuns();
    } catch (error) {
      if (isAxiosError(error)) {
        toast.error(error.response?.data?.detail || error.response?.data?.message || "Failed to create payroll run.");
      } else {
        toast.error("An unexpected error occurred.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status.toUpperCase()) {
      case "DRAFT":
        return <Badge variant="default">Draft</Badge>;
      case "GENERATING":
        return <Badge variant="blue" className="animate-pulse flex items-center gap-1"><RefreshCw className="h-3 w-3 animate-spin" /> Generating</Badge>;
      case "GENERATED":
        return <Badge variant="blue">Generated</Badge>;
      case "UNDERREVIEW":
        return <Badge variant="warning">Under Review</Badge>;
      case "APPROVED":
        return <Badge variant="success">Approved</Badge>;
      case "DISBURSED":
        return <Badge variant="success">Disbursed</Badge>;
      case "FAILED":
        return <Badge variant="destructive">Failed</Badge>;
      default:
        return <Badge variant="default">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Payroll Runs</h1>
          <p className="text-sm text-muted-foreground">Execute, monitor, and approve monthly payroll calculation batches</p>
        </div>
        <Button onClick={() => setIsDialogOpen(true)} className="bg-primary hover:bg-primary/90 text-primary-foreground font-medium flex items-center gap-2">
          <Plus className="h-4 w-4" /> New Payroll Run
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">Total Runs</CardTitle>
            <Play className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{runs.length}</div>
            <p className="text-xs text-muted-foreground mt-1">Batches processed across financial years</p>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">State Machine Guard</CardTitle>
            <CheckCircle2 className="h-4 w-4 text-green-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">11-State Flow</div>
            <p className="text-xs text-muted-foreground mt-1">Strict transition rules per PRD §15.3</p>
          </CardContent>
        </Card>

        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
            <CardTitle className="text-sm font-medium text-muted-foreground">Correction Versioning</CardTitle>
            <AlertCircle className="h-4 w-4 text-amber-500" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">Append-Only</div>
            <p className="text-xs text-muted-foreground mt-1">Corrections create new RunVersions</p>
          </CardContent>
        </Card>
      </div>

      <Card className="border-border">
        <CardHeader>
          <CardTitle>Payroll Run History</CardTitle>
          <CardDescription>All calculated and active payroll runs for your organization</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8 text-muted-foreground">Loading payroll runs...</div>
          ) : runs.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">No payroll runs found. Create your first run to get started.</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-left">
                <thead className="text-xs text-muted-foreground uppercase bg-muted/40 border-b border-border">
                  <tr>
                    <th className="px-4 py-3">Period</th>
                    <th className="px-4 py-3">Run Type</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3">Employees</th>
                    <th className="px-4 py-3">Total Gross</th>
                    <th className="px-4 py-3">Total Net</th>
                    <th className="px-4 py-3 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {runs.map((run) => (
                    <tr key={run.id} className="hover:bg-muted/20 transition-colors">
                      <td className="px-4 py-3 font-medium">
                        {run.periodYear} - {String(run.periodMonth).padStart(2, "0")}
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant="default">{run.runType}</Badge>
                      </td>
                      <td className="px-4 py-3">{getStatusBadge(run.status)}</td>
                      <td className="px-4 py-3">{run.totalEmployees}</td>
                      <td className="px-4 py-3">PKR {run.totalGross.toLocaleString()}</td>
                      <td className="px-4 py-3 font-semibold text-emerald-400">PKR {run.totalNet.toLocaleString()}</td>
                      <td className="px-4 py-3 text-right">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => router.push(`/${role}/payroll-runs/${run.id}`)}
                          className="flex items-center gap-1 text-primary hover:text-primary/80"
                        >
                          <Eye className="h-4 w-4" /> View Detail
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* New Run Modal */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-[450px]">
          <DialogHeader>
            <DialogTitle>Initiate New Payroll Run</DialogTitle>
            <DialogDescription>Create a batch calculation job for a specific period.</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleCreateRun} className="space-y-4 py-2">
            <div className="space-y-2">
              <Label htmlFor="fy">Financial Year ID</Label>
              <Input
                id="fy"
                placeholder="UUID or default FY ID"
                value={formData.financialYearId}
                onChange={(e) => setFormData({ ...formData, financialYearId: e.target.value })}
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
                <Label htmlFor="month">Period Month</Label>
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
            <div className="space-y-2">
              <Label htmlFor="remarks">Remarks</Label>
              <Input
                id="remarks"
                value={formData.remarks}
                onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
              />
            </div>
            <DialogFooter className="pt-4">
              <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Starting..." : "Initiate Run"}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
