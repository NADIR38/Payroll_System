"use client";

import { useEffect, useState, useCallback, use } from "react";
import { useRouter } from "next/navigation";
import { EmployeeApi, EmployeeProfileResponse, EmployeeProfileHistoryResponse } from "@/lib/api/employee";
import { TenantApi } from "@/lib/api/tenant";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, UserCircle, Briefcase, Building2, Calendar, CreditCard, History, MoreVertical, Building } from "lucide-react";
import { SectionHeader } from "@/components/ui/section-header";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";

export default function EmployeeDetailsPage({ params }: { params: Promise<{ role: string; id: string }> }) {
  const router = useRouter();
  const { role, id } = use(params);
  
  const [employee, setEmployee] = useState<EmployeeProfileResponse | null>(null);
  const [history, setHistory] = useState<EmployeeProfileHistoryResponse[]>([]);
  
  const [departmentName, setDepartmentName] = useState<string>("Loading...");
  const [branchName, setBranchName] = useState<string>("Loading...");

  const [isLoading, setIsLoading] = useState(true);
  
  const [isBankModalOpen, setIsBankModalOpen] = useState(false);
  const [isSubmittingBank, setIsSubmittingBank] = useState(false);
  const [newBank, setNewBank] = useState({
    bankName: "",
    accountTitle: "",
    accountNumber: "",
    iban: "",
    branchCode: "",
    isPrimary: false
  });

  const fetchData = useCallback(async () => {
    try {
      const [profileData, historyData] = await Promise.all([
        EmployeeApi.getEmployeeById(id),
        EmployeeApi.getEmployeeHistory(id)
      ]);
      setEmployee(profileData);
      setHistory(historyData);

      Promise.all([
        TenantApi.getDepartmentById(profileData.departmentId).catch(() => null),
        TenantApi.getBranchById(profileData.branchId).catch(() => null)
      ]).then(([dept, branch]) => {
        setDepartmentName(dept ? dept.name : "Unknown Department");
        setBranchName(branch ? branch.name : "Unknown Branch");
      });
    } catch (error) {
      console.error("Failed to load employee details:", error);
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    let isMounted = true;
    const loadData = async () => {
      try {
        const [profileData, historyData] = await Promise.all([
          EmployeeApi.getEmployeeById(id),
          EmployeeApi.getEmployeeHistory(id)
        ]);
        if (isMounted) {
          setEmployee(profileData);
          setHistory(historyData);
        }

        const [dept, branch] = await Promise.all([
          TenantApi.getDepartmentById(profileData.departmentId).catch(() => null),
          TenantApi.getBranchById(profileData.branchId).catch(() => null)
        ]);
        if (isMounted) {
          setDepartmentName(dept ? dept.name : "Unknown Department");
          setBranchName(branch ? branch.name : "Unknown Branch");
        }
      } catch (error) {
        console.error("Failed to load employee details:", error);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };
    loadData();
    return () => { isMounted = false; };
  }, [id]);

  const handleSetPrimaryBank = async (accountId: string) => {
    try {
      await EmployeeApi.setPrimaryBankAccount(id, accountId);
      await fetchData(); // Refresh data
    } catch (error) {
      console.error("Failed to set primary bank:", error);
    }
  };

  const handleAddBank = async () => {
    if (!newBank.bankName || !newBank.accountNumber || !newBank.accountTitle || !newBank.iban) {
      alert("Please fill all required fields");
      return;
    }
    
    try {
      setIsSubmittingBank(true);
      await EmployeeApi.addBankAccount(id, {
        employeePayrollProfileId: employee!.id,
        bankName: newBank.bankName,
        accountTitle: newBank.accountTitle,
        accountNumber: newBank.accountNumber,
        iban: newBank.iban,
        branchCode: newBank.branchCode || null,
        isPrimary: newBank.isPrimary
      });
      await fetchData(); // Refresh data
      setIsBankModalOpen(false);
      setNewBank({
        bankName: "",
        accountTitle: "",
        accountNumber: "",
        iban: "",
        branchCode: "",
        isPrimary: false
      });
    } catch (error) {
      console.error("Failed to add bank:", error);
      alert("Failed to add bank account. Ensure IBAN is valid if required.");
    } finally {
      setIsSubmittingBank(false);
    }
  };

  if (isLoading) {
    return <div className="p-8 text-center text-slate-500">Loading profile...</div>;
  }

  if (!employee) {
    return <div className="p-8 text-center text-red-500">Employee not found.</div>;
  }

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      <div className="flex items-center gap-4">
        <Button 
          variant="outline" 
          size="icon" 
          onClick={() => router.push(`/${role}/employees`)}
          className="shrink-0"
        >
          <ArrowLeft className="w-4 h-4" />
        </Button>
        <div className="flex-1 flex items-center justify-between">
          <SectionHeader 
            title={employee.fullName}
            subhead={`Employee Code: ${employee.employeeCode} | HRIS: ${employee.externalEmployeeId}`}
          />
          <div className="flex items-center gap-2">
            <Badge variant={employee.status === "Active" ? "success" : "warning"}>
              {employee.status}
            </Badge>
            <Button variant="outline" size="icon">
              <MoreVertical className="w-4 h-4 text-slate-600" />
            </Button>
          </div>
        </div>
      </div>

      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="grid w-full grid-cols-3 max-w-md bg-slate-100/50">
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="banks">Bank Accounts</TabsTrigger>
          <TabsTrigger value="history">History</TabsTrigger>
        </TabsList>
        
        <TabsContent value="overview" className="mt-6 space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Card className="md:col-span-2 border-slate-200 shadow-sm">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2">
                  <UserCircle className="w-5 h-5 text-primary" />
                  Profile Details
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid grid-cols-2 gap-y-6 gap-x-4">
                  <div>
                    <div className="text-sm text-slate-500 mb-1">Full Name</div>
                    <div className="font-medium text-slate-900">{employee.fullName}</div>
                  </div>
                  <div>
                    <div className="text-sm text-slate-500 mb-1">Employee Code</div>
                    <div className="font-medium text-slate-900">{employee.employeeCode}</div>
                  </div>
                  <div>
                    <div className="text-sm text-slate-500 mb-1 flex items-center gap-1.5">
                      <Calendar className="w-3.5 h-3.5" /> Joining Date
                    </div>
                    <div className="font-medium text-slate-900">
                      {new Date(employee.joiningDate).toLocaleDateString()}
                    </div>
                  </div>
                  <div>
                    <div className="text-sm text-slate-500 mb-1 flex items-center gap-1.5">
                      <Calendar className="w-3.5 h-3.5" /> Effective From
                    </div>
                    <div className="font-medium text-slate-900">
                      {new Date(employee.effectiveFrom).toLocaleDateString()}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="border-slate-200 shadow-sm">
              <CardHeader className="pb-4">
                <CardTitle className="text-lg flex items-center gap-2">
                  <Briefcase className="w-5 h-5 text-primary" />
                  Employment Data
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <div className="text-sm text-slate-500 mb-1 flex items-center gap-1.5">
                    <Building2 className="w-3.5 h-3.5" /> Department
                  </div>
                  <div className="font-medium text-slate-900 text-sm truncate">
                    {departmentName}
                  </div>
                </div>
                <div>
                  <div className="text-sm text-slate-500 mb-1 flex items-center gap-1.5">
                    <Building className="w-3.5 h-3.5" /> Branch
                  </div>
                  <div className="font-medium text-slate-900 text-sm truncate">
                    {branchName}
                  </div>
                </div>
                <div className="pt-4 border-t border-slate-100">
                  <div className="text-sm text-slate-500 mb-1">Base Salary</div>
                  <div className="text-2xl font-bold text-slate-900">
                    ${employee.baseSalary.toLocaleString()}
                  </div>
                  <div className="text-xs text-slate-500 mt-1">
                    {employee.attendanceDeductionOptIn ? "Subject to Attendance Deductions" : "Fixed Output"}
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="banks" className="mt-6">
          <Card className="border-slate-200 shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-4">
              <div>
                <CardTitle className="text-lg flex items-center gap-2">
                  <CreditCard className="w-5 h-5 text-primary" />
                  Bank Accounts
                </CardTitle>
                <CardDescription>Manage disbursement destinations.</CardDescription>
              </div>
              <Button size="sm" onClick={() => setIsBankModalOpen(true)}>Add Bank Account</Button>
            </CardHeader>
            <CardContent>
              {employee.bankAccounts.length === 0 ? (
                <div className="text-center py-8 text-slate-500">No bank accounts configured.</div>
              ) : (
                <div className="space-y-4">
                  {employee.bankAccounts.map((bank) => (
                    <div key={bank.id} className={`flex items-center justify-between p-4 rounded-lg border ${bank.isPrimary ? 'border-primary/50 bg-primary/5' : 'border-slate-200 bg-white'}`}>
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          <span className="font-semibold text-slate-900">{bank.bankName}</span>
                          {bank.isPrimary && <Badge variant="success" className="text-xs h-5">Primary</Badge>}
                          {!bank.isActive && <Badge variant="warning" className="text-xs h-5">Inactive</Badge>}
                        </div>
                        <div className="text-sm text-slate-600">
                          Account: {bank.accountNumber} <span className="text-slate-300 mx-1">|</span> {bank.accountTitle}
                        </div>
                        <div className="text-xs text-slate-500 mt-1 font-mono">{bank.iban}</div>
                      </div>
                      {!bank.isPrimary && bank.isActive && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => handleSetPrimaryBank(bank.id)}
                        >
                          Set Primary
                        </Button>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="history" className="mt-6">
          <Card className="border-slate-200 shadow-sm">
            <CardHeader className="pb-4">
              <CardTitle className="text-lg flex items-center gap-2">
                <History className="w-5 h-5 text-primary" />
                Audit & Version History
              </CardTitle>
              <CardDescription>Temporal view of profile changes (ERP style).</CardDescription>
            </CardHeader>
            <CardContent>
              {history.length === 0 ? (
                <div className="text-center py-8 text-slate-500">No historical records found.</div>
              ) : (
                <div className="relative border-l-2 border-slate-200 ml-4 pl-6 space-y-8 py-2">
                  {history.map((record, idx) => (
                    <div key={record.id} className="relative">
                      <div className={`absolute -left-[31px] w-4 h-4 rounded-full border-2 border-white ${idx === 0 ? 'bg-primary' : 'bg-slate-300'}`}></div>
                      <div className="bg-slate-50 rounded-lg p-4 border border-slate-200">
                        <div className="flex justify-between items-start mb-2">
                          <div className="font-medium text-slate-900 flex items-center gap-2">
                            Base Salary: ${record.baseSalary.toLocaleString()}
                          </div>
                          <div className="text-xs text-slate-500 bg-white px-2 py-1 rounded border border-slate-200 shadow-sm">
                            {new Date(record.effectiveFrom).toLocaleDateString()} — {record.effectiveTo ? new Date(record.effectiveTo).toLocaleDateString() : 'Present'}
                          </div>
                        </div>
                        <div className="text-sm text-slate-600">
                          <span className="font-medium text-slate-700">Reason:</span> {record.changeReason || "Initial Setup"}
                        </div>
                        {record.changedBy && (
                          <div className="text-xs text-slate-500 mt-2">
                            Changed By: {record.changedBy}
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <Dialog open={isBankModalOpen} onOpenChange={setIsBankModalOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Add Bank Account</DialogTitle>
            <DialogDescription>
              Add a new disbursement destination for {employee.fullName}.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="space-y-2">
              <Label>Bank Name <span className="text-red-500">*</span></Label>
              <Input 
                value={newBank.bankName} 
                onChange={e => setNewBank({...newBank, bankName: e.target.value})} 
                placeholder="e.g. Chase Bank"
              />
            </div>
            <div className="space-y-2">
              <Label>Account Title <span className="text-red-500">*</span></Label>
              <Input 
                value={newBank.accountTitle} 
                onChange={e => setNewBank({...newBank, accountTitle: e.target.value})} 
                placeholder="e.g. John Doe"
              />
            </div>
            <div className="space-y-2">
              <Label>Account Number <span className="text-red-500">*</span></Label>
              <Input 
                value={newBank.accountNumber} 
                onChange={e => setNewBank({...newBank, accountNumber: e.target.value})} 
                placeholder="e.g. 123456789"
              />
            </div>
            <div className="space-y-2">
              <Label>IBAN <span className="text-red-500">*</span></Label>
              <Input 
                value={newBank.iban} 
                onChange={e => setNewBank({...newBank, iban: e.target.value})} 
                placeholder="e.g. US12 CHAS 0000 1234 5678 90"
              />
            </div>
            <div className="space-y-2">
              <Label>Branch Code</Label>
              <Input 
                value={newBank.branchCode} 
                onChange={e => setNewBank({...newBank, branchCode: e.target.value})} 
                placeholder="Optional"
              />
            </div>
            <div className="flex items-center gap-2 mt-2">
              <input 
                type="checkbox" 
                id="isPrimary"
                checked={newBank.isPrimary}
                onChange={e => setNewBank({...newBank, isPrimary: e.target.checked})}
                className="w-4 h-4 text-primary rounded border-slate-300"
              />
              <Label htmlFor="isPrimary" className="font-normal">Set as primary account</Label>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsBankModalOpen(false)}>Cancel</Button>
            <Button onClick={handleAddBank} disabled={isSubmittingBank}>
              {isSubmittingBank ? "Saving..." : "Save Account"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
