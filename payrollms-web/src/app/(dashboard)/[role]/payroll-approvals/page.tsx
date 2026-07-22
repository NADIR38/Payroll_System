"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { payrollApi, PendingApprovalItemResponse } from "@/lib/api/payroll";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { CheckSquare, ArrowRight } from "lucide-react";

export default function PendingApprovalsPage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;

  const [pendingItems, setPendingItems] = useState<PendingApprovalItemResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    const fetchPendingApprovals = async () => {
      try {
        const userRole = role === "hr" ? "HRManager" : role === "finance" ? "FinanceManager" : "CompanyAdmin";
        const data = await payrollApi.getPendingApprovals(userRole);
        if (isMounted) setPendingItems(data);
      } catch (error) {
        console.error("Failed to fetch pending approvals:", error);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };
    fetchPendingApprovals();
    return () => { isMounted = false; };
  }, [role]);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Pending Payroll Approvals</h1>
        <p className="text-sm text-muted-foreground">Review and sign off on payroll runs awaiting your role&apos;s approval</p>
      </div>

      <Card className="border-border">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckSquare className="h-5 w-5 text-primary" /> Approval Inbox ({pendingItems.length})
          </CardTitle>
          <CardDescription>Multi-step role-gated sign-off queue per PRD §16.4</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="text-center py-8 text-muted-foreground">Loading pending approvals...</div>
          ) : pendingItems.length === 0 ? (
            <div className="text-center py-12 text-muted-foreground">
              <CheckSquare className="h-12 w-12 mx-auto mb-3 opacity-30 text-green-500" />
              <p className="font-medium text-foreground">You&apos;re all caught up!</p>
              <p className="text-xs text-muted-foreground mt-1">No payroll runs are currently pending your approval.</p>
            </div>
          ) : (
            <div className="space-y-4">
              {pendingItems.map((item) => (
                <div
                  key={item.payrollRunId}
                  className="p-4 rounded-lg border border-border bg-card hover:bg-muted/20 transition-colors flex flex-col md:flex-row items-start md:items-center justify-between gap-4"
                >
                  <div className="space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-base">
                        Period: {item.periodYear} - {String(item.periodMonth).padStart(2, "0")}
                      </span>
                      <Badge variant="default">{item.runType}</Badge>
                    </div>
                    <div className="text-sm text-muted-foreground">
                      Pending Step: <span className="font-medium text-amber-400">Step {item.pendingStepOrder}: {item.pendingStepName}</span> (Role: {item.requiredRole})
                    </div>
                    <div className="text-xs text-muted-foreground">
                      Total Employees: {item.totalEmployees} | Net Payable: PKR {item.totalNet.toLocaleString()}
                    </div>
                  </div>

                  <Button
                    onClick={() => router.push(`/${role}/payroll-runs/${item.payrollRunId}`)}
                    className="bg-primary hover:bg-primary/90 text-primary-foreground flex items-center gap-2"
                  >
                    Review & Sign Off <ArrowRight className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
