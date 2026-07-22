"use client";
import { useState, useEffect } from "react";
import { useRouter, useParams } from "next/navigation";
import { EmployeeApi, SyncEmployeeCommand } from "@/lib/api/employee";
import { TenantApi, BranchDto, DepartmentDto, DesignationDto } from "@/lib/api/tenant";
import { SalaryApi, SalaryStructureResponse } from "@/lib/api/salary";
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { SectionHeader } from "@/components/ui/section-header";
import { toast } from "sonner";
import { ArrowLeft, Save } from "lucide-react";

export default function NewEmployeePage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;
  
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingData, setIsLoadingData] = useState(true);

  // Reference Data
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [designations, setDesignations] = useState<DesignationDto[]>([]);
  const [structures, setStructures] = useState<SalaryStructureResponse[]>([]);

  const [formData, setFormData] = useState<SyncEmployeeCommand>({
    externalEmployeeId: "",
    employeeCode: "",
    fullName: "",
    branchId: "",
    departmentId: "",
    designationId: "",
    salaryStructureId: "",
    baseSalary: 0,
    joiningDate: new Date().toISOString().split("T")[0],
    attendanceDeductionOptIn: false,
  });



  useEffect(() => {
    let isMounted = true;
    const loadRefData = async () => {
      try {
        const [bData, dData, desigData, sData] = await Promise.all([
          TenantApi.getBranches(),
          TenantApi.getDepartments(),
          TenantApi.getDesignations(),
          SalaryApi.getStructures(),
        ]);
        if (isMounted) {
          setBranches(bData);
          setDepartments(dData);
          setDesignations(desigData);
          setStructures(sData);

          setFormData(prev => ({
            ...prev,
            branchId: bData[0]?.id || "",
            departmentId: dData[0]?.id || "",
            designationId: desigData[0]?.id || "",
            salaryStructureId: sData[0]?.id || "",
          }));
        }
      } catch (error) {
        console.error("Failed to load reference data:", error);
      } finally {
        if (isMounted) setIsLoadingData(false);
      }
    };
    loadRefData();
    return () => { isMounted = false; };
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    const checked = (e.target as HTMLInputElement).checked;
    
    setFormData(prev => ({
      ...prev,
      [name]: type === "checkbox" ? checked : type === "number" ? Number(value) : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.branchId || !formData.departmentId || !formData.designationId || !formData.salaryStructureId) {
      toast.error("Please ensure all dropdowns are selected.");
      return;
    }

    try {
      setIsSubmitting(true);
      const result = await EmployeeApi.syncEmployee(formData);
      router.push(`/${role}/employees/${result.profileId}`);
    } catch (error) {
      console.error(error);
      toast.error("Failed to create employee. Please check console.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="flex items-center gap-4">
        <Button 
          variant="outline" 
          size="icon" 
          onClick={() => router.push(`/${role}/employees`)}
          className="shrink-0"
        >
          <ArrowLeft className="w-4 h-4" />
        </Button>
        <SectionHeader 
          title="Onboard New Employee" 
          subhead="Register a new employee profile to the payroll system."
        />
      </div>

      <form onSubmit={handleSubmit}>
        <Card className="border-slate-200 shadow-sm">
          <CardHeader>
            <CardTitle className="text-lg">Basic Information</CardTitle>
            <CardDescription>Enter the primary details for the employee.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <Label htmlFor="fullName">Full Name</Label>
                <Input 
                  id="fullName" 
                  name="fullName" 
                  required 
                  value={formData.fullName}
                  onChange={handleChange}
                  placeholder="John Doe" 
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="employeeCode">Employee Code (Optional)</Label>
                <Input 
                  id="employeeCode" 
                  name="employeeCode" 
                  placeholder="Auto-generated if left blank" 
                  value={formData.employeeCode}
                  onChange={handleChange}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="externalEmployeeId">External System ID (HRIS)</Label>
                <Input 
                  id="externalEmployeeId" 
                  name="externalEmployeeId" 
                  required 
                  value={formData.externalEmployeeId}
                  onChange={handleChange}
                  placeholder="HRIS-001" 
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="joiningDate">Joining Date</Label>
                <Input 
                  id="joiningDate" 
                  name="joiningDate" 
                  type="date"
                  required 
                  value={formData.joiningDate}
                  onChange={handleChange}
                />
              </div>
            </div>

            <div className="pt-4 border-t border-slate-100">
              <h3 className="text-sm font-medium text-slate-900 mb-4">Organization & Financial Details</h3>
              
              {isLoadingData ? (
                <div className="text-sm text-slate-500 py-4">Loading organization data...</div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <Label htmlFor="branchId">Branch</Label>
                    <select 
                      id="branchId" 
                      name="branchId"
                      className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      required
                      value={formData.branchId}
                      onChange={handleChange}
                    >
                      <option value="" disabled>Select Branch</option>
                      {branches.map(b => (
                        <option key={b.id} value={b.id}>{b.name} ({b.code})</option>
                      ))}
                    </select>
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="departmentId">Department</Label>
                    <select 
                      id="departmentId" 
                      name="departmentId"
                      className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      required
                      value={formData.departmentId}
                      onChange={handleChange}
                    >
                      <option value="" disabled>Select Department</option>
                      {departments.map(d => (
                        <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                      ))}
                    </select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="designationId">Designation</Label>
                    <select 
                      id="designationId" 
                      name="designationId"
                      className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      required
                      value={formData.designationId}
                      onChange={handleChange}
                    >
                      <option value="" disabled>Select Designation</option>
                      {designations.map(d => (
                        <option key={d.id} value={d.id}>{d.name} ({d.code})</option>
                      ))}
                    </select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="salaryStructureId">Salary Structure</Label>
                    <select 
                      id="salaryStructureId" 
                      name="salaryStructureId"
                      className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      required
                      value={formData.salaryStructureId}
                      onChange={handleChange}
                    >
                      <option value="" disabled>Select Structure</option>
                      {structures.map(s => (
                        <option key={s.id} value={s.id}>{s.name}</option>
                      ))}
                    </select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="baseSalary">Base Salary</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-2.5 text-slate-500">$</span>
                      <Input 
                        id="baseSalary" 
                        name="baseSalary" 
                        type="number"
                        min="0"
                        step="0.01"
                        required 
                        className="pl-7"
                        value={formData.baseSalary}
                        onChange={handleChange}
                      />
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-2 pt-8">
                    <input
                      type="checkbox"
                      id="attendanceDeductionOptIn"
                      name="attendanceDeductionOptIn"
                      className="w-4 h-4 rounded border-slate-300 text-primary focus:ring-primary"
                      checked={formData.attendanceDeductionOptIn}
                      onChange={handleChange}
                    />
                    <Label htmlFor="attendanceDeductionOptIn" className="font-normal">
                      Opt-in for Attendance & Leave Deductions
                    </Label>
                  </div>
                </div>
              )}
            </div>
          </CardContent>
          <CardFooter className="bg-slate-50 border-t border-slate-100 py-4 flex justify-end gap-3 rounded-b-lg">
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => router.push(`/${role}/employees`)}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting || isLoadingData} className="gap-2">
              {isSubmitting ? "Saving..." : (
                <>
                  <Save className="w-4 h-4" />
                  Save Employee
                </>
              )}
            </Button>
          </CardFooter>
        </Card>
      </form>
    </div>
  );
}
