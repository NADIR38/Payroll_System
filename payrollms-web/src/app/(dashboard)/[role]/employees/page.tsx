"use client";

import { useEffect, useState, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import { EmployeeApi, EmployeeProfileResponse } from "@/lib/api/employee";
import { TenantApi } from "@/lib/api/tenant";
import { useDebounce } from "@/hooks/useDebounce";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Users, Search, Plus, Building2 } from "lucide-react";
import { SectionHeader } from "@/components/ui/section-header";

export default function EmployeesPage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;
  
  const [employees, setEmployees] = useState<EmployeeProfileResponse[]>([]);
  const [departments, setDepartments] = useState<Record<string, string>>({});
  const [designations, setDesignations] = useState<Record<string, string>>({});
  
  const [searchTerm, setSearchTerm] = useState("");
  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const [isLoading, setIsLoading] = useState(true);

  const fetchData = useCallback(async (search?: string) => {
    try {
      const [empData, deptList, desigList] = await Promise.all([
        EmployeeApi.getEmployees(search ? { searchTerm: search } : undefined),
        TenantApi.getDepartments(),
        TenantApi.getDesignations()
      ]);
      
      const deptMap: Record<string, string> = {};
      deptList.forEach(d => deptMap[d.id] = d.name);
      setDepartments(deptMap);

      const desigMap: Record<string, string> = {};
      desigList.forEach(d => desigMap[d.id] = d.name);
      setDesignations(desigMap);

      setEmployees(empData);
    } catch (error) {
      console.error("Failed to load employees:", error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    let isMounted = true;
    const loadEmpData = async () => {
      try {
        const [empData, deptList, desigList] = await Promise.all([
          EmployeeApi.getEmployees(debouncedSearchTerm ? { searchTerm: debouncedSearchTerm } : undefined),
          TenantApi.getDepartments(),
          TenantApi.getDesignations()
        ]);
        if (isMounted) {
          const deptMap: Record<string, string> = {};
          deptList.forEach(d => deptMap[d.id] = d.name);
          setDepartments(deptMap);

          const desigMap: Record<string, string> = {};
          desigList.forEach(d => desigMap[d.id] = d.name);
          setDesignations(desigMap);

          setEmployees(empData);
        }
      } catch (error) {
        console.error("Failed to load employees:", error);
      } finally {
        if (isMounted) setIsLoading(false);
      }
    };
    loadEmpData();
    return () => { isMounted = false; };
  }, [debouncedSearchTerm]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchData(debouncedSearchTerm);
  };

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
        return <Badge className="bg-green-100 text-green-800 hover:bg-green-100 border-green-200">Active</Badge>;
      case "terminated":
        return <Badge className="bg-red-100 text-red-800 hover:bg-red-100 border-red-200">Terminated</Badge>;
      case "suspended":
        return <Badge className="bg-orange-100 text-orange-800 hover:bg-orange-100 border-orange-200">Suspended</Badge>;
      default:
        return <Badge className="bg-slate-100 text-slate-800 hover:bg-slate-100 border-slate-200">{status}</Badge>;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <SectionHeader 
          title="Employee Directory" 
          subhead="Manage your workforce profiles and payroll settings."
        />
        <Button onClick={() => router.push(`/${role}/employees/new`)} className="gap-2 shrink-0">
          <Plus className="w-4 h-4" />
          Add Employee
        </Button>
      </div>

      <Card className="border-slate-200 shadow-sm">
        <CardHeader className="pb-4">
          <div className="flex justify-between items-center">
            <CardTitle className="text-lg font-medium text-slate-800">All Employees</CardTitle>
            <form onSubmit={handleSearch} className="flex gap-2 max-w-sm w-full">
              <div className="relative w-full">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-500" />
                <Input
                  type="search"
                  placeholder="Search by name or ID..."
                  className="pl-9 bg-slate-50 border-slate-200"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
              <Button type="submit" variant="outline">Search</Button>
            </form>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex justify-center p-8 text-slate-500">Loading employees...</div>
          ) : employees.length === 0 ? (
            <div className="flex flex-col items-center justify-center p-12 text-slate-500 bg-slate-50/50 rounded-lg border border-dashed border-slate-200">
              <Users className="w-12 h-12 text-slate-300 mb-4" />
              <h3 className="text-lg font-medium text-slate-900 mb-1">No employees found</h3>
              <p className="text-sm text-slate-500 max-w-sm text-center mb-4">
                We couldn&apos;t find any employees matching your search criteria, or your directory is currently empty.
              </p>
              <Button onClick={() => router.push(`/${role}/employees/new`)} variant="outline">
                Create First Employee
              </Button>
            </div>
          ) : (
            <div className="rounded-md border border-slate-200 overflow-hidden">
              <table className="w-full text-sm text-left">
                <thead className="bg-slate-50 text-slate-600 font-medium border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-3">Employee</th>
                    <th className="px-4 py-3">ID / Code</th>
                    <th className="px-4 py-3">Department</th>
                    <th className="px-4 py-3">Base Salary</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 bg-white">
                  {employees.map((emp) => (
                    <tr key={emp.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-4 py-3">
                        <div className="font-medium text-slate-900">{emp.fullName}</div>
                        <div className="text-xs text-slate-500">{designations[emp.designationId] || "Unknown Designation"}</div>
                      </td>
                      <td className="px-4 py-3 text-slate-600">
                        <div className="font-mono text-xs">{emp.employeeCode}</div>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-1.5 text-slate-600 text-xs">
                          <Building2 className="w-3.5 h-3.5" />
                          <span>{departments[emp.departmentId] || "Unknown Department"}</span>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-slate-700 font-medium">
                        ${emp.baseSalary.toLocaleString()}
                      </td>
                      <td className="px-4 py-3">
                        {getStatusBadge(emp.status)}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          onClick={() => router.push(`/${role}/employees/${emp.id}`)}
                          className="text-primary hover:text-primary/80 hover:bg-primary/10"
                        >
                          View Profile
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
    </div>
  );
}
