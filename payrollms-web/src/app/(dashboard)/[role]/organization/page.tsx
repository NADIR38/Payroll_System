"use client"

import * as React from "react"
import { SectionHeader } from "@/components/ui/section-header"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs"
import { 
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter 
} from "@/components/ui/dialog"
import { Plus, Building2, Briefcase, Award, Landmark, MapPin, Pencil } from "lucide-react"
import { 
  useBranches, useCreateBranch, useUpdateBranch,
  useDepartments, useCreateDepartment, useUpdateDepartment,
  useDesignations, useCreateDesignation, useUpdateDesignation,
  useCostCenters, useCreateCostCenter, useUpdateCostCenter,
  useFinancialYears, useCreateFinancialYear,
  usePayrollCalendars, useCreatePayrollCalendar
} from "@/hooks/useTenant"

export default function OrganizationPage() {
  const { data: branches } = useBranches()
  const { data: departments } = useDepartments()
  const { data: designations } = useDesignations()
  const { data: costCenters } = useCostCenters()
  const { data: financialYears } = useFinancialYears()
  const { data: calendars } = usePayrollCalendars()

  const createBranch = useCreateBranch()
  const updateBranch = useUpdateBranch()
  
  const createDept = useCreateDepartment()
  const updateDept = useUpdateDepartment()
  
  const createDesig = useCreateDesignation()
  const updateDesig = useUpdateDesignation()
  
  const createCC = useCreateCostCenter()
  const updateCC = useUpdateCostCenter()

  const createFY = useCreateFinancialYear()
  const createCal = useCreatePayrollCalendar()

  const [activeTab, setActiveTab] = React.useState("branches")
  const [isModalOpen, setIsModalOpen] = React.useState(false)
  const [editingId, setEditingId] = React.useState<string | null>(null)
  
  const [formData, setFormData] = React.useState({ 
    code: "", name: "", address: "", branchId: "", grade: "",
    startDate: "", endDate: "", month: new Date().getMonth() + 1, year: new Date().getFullYear(),
    workingDays: 20, freezeDate: "", paymentDate: "", financialYearId: ""
  })

  const openEditModal = (item: any) => {
    setEditingId(item.id)
    setFormData({
      code: item.code || "",
      name: item.name || "",
      address: item.address || "",
      branchId: item.branchId || "",
      grade: item.grade || "",
      startDate: item.startDate || "",
      endDate: item.endDate || "",
      month: item.month || new Date().getMonth() + 1,
      year: item.year || new Date().getFullYear(),
      workingDays: item.workingDays || 20,
      freezeDate: item.payrollFreezeDate || "",
      paymentDate: item.paymentDate || "",
      financialYearId: item.financialYearId || ""
    })
    setIsModalOpen(true)
  }

  const resetForm = () => {
    setEditingId(null)
    setFormData({ 
        code: "", name: "", address: "", branchId: "", grade: "",
        startDate: "", endDate: "", month: new Date().getMonth() + 1, year: new Date().getFullYear(),
        workingDays: 20, freezeDate: "", paymentDate: "", financialYearId: ""
    })
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    
    if (editingId) {
      if (activeTab === "branches") {
        updateBranch.mutate(
          { id: editingId, data: { id: editingId, code: formData.code, name: formData.name, address: formData.address } }, 
          { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
        )
      } else if (activeTab === "departments") {
        updateDept.mutate(
          { id: editingId, data: { id: editingId, code: formData.code, name: formData.name, branchId: formData.branchId || undefined } }, 
          { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
        )
      } else if (activeTab === "designations") {
        updateDesig.mutate(
          { id: editingId, data: { id: editingId, code: formData.code, name: formData.name, grade: formData.grade } }, 
          { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
        )
      } else if (activeTab === "cost-centers") {
        updateCC.mutate(
          { id: editingId, data: { id: editingId, code: formData.code, name: formData.name } }, 
          { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
        )
      }
      return;
    }

    if (activeTab === "branches") {
      createBranch.mutate(
        { code: formData.code, name: formData.name, address: formData.address }, 
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    } else if (activeTab === "departments") {
      createDept.mutate(
        { code: formData.code, name: formData.name, branchId: formData.branchId || undefined }, 
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    } else if (activeTab === "designations") {
      createDesig.mutate(
        { code: formData.code, name: formData.name, grade: formData.grade }, 
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    } else if (activeTab === "cost-centers") {
      createCC.mutate(
        { code: formData.code, name: formData.name }, 
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    } else if (activeTab === "financial-years") {
      createFY.mutate(
        { label: formData.name, startDate: formData.startDate, endDate: formData.endDate, isCurrent: true },
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    } else if (activeTab === "calendars") {
      createCal.mutate(
        { 
          financialYearId: formData.financialYearId, 
          month: Number(formData.month), 
          year: Number(formData.year), 
          workingDays: Number(formData.workingDays),
          payrollFreezeDate: formData.freezeDate,
          paymentDate: formData.paymentDate
        },
        { onSuccess: () => { setIsModalOpen(false); resetForm(); } }
      )
    }
  }

  const activeMutation = 
    editingId ? (
      activeTab === "branches" ? updateBranch :
      activeTab === "departments" ? updateDept :
      activeTab === "designations" ? updateDesig : updateCC
    ) : (
      activeTab === "branches" ? createBranch :
      activeTab === "departments" ? createDept :
      activeTab === "designations" ? createDesig : 
      activeTab === "cost-centers" ? createCC :
      activeTab === "financial-years" ? createFY : createCal
    )

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center border-b border-ink-black/10 pb-5">
        <SectionHeader 
          title="Organization Setup" 
          subhead="Configure organizational structure, branches, departments, designations, and financial cost centers."
        />
        <Button variant="primary" onClick={() => { resetForm(); setIsModalOpen(true); }}>
          <Plus className="w-4 h-4 mr-2" />
          Add {activeTab === "cost-centers" ? "Cost Center" : activeTab.slice(0, -1)}
        </Button>
      </div>

      <Tabs defaultValue="branches" onValueChange={setActiveTab}>
        <TabsList className="bg-transparent border-b border-ink-black/10 rounded-none w-full justify-start h-auto p-0 space-x-6">
          <TabsTrigger value="branches" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Branches</TabsTrigger>
          <TabsTrigger value="departments" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Departments</TabsTrigger>
          <TabsTrigger value="designations" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Designations</TabsTrigger>
          <TabsTrigger value="cost-centers" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Cost Centers</TabsTrigger>
          <TabsTrigger value="financial-years" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Financial Years</TabsTrigger>
          <TabsTrigger value="calendars" className="data-[state=active]:bg-transparent data-[state=active]:shadow-none data-[state=active]:border-b-2 data-[state=active]:border-primary rounded-none px-0 py-3 text-ink-black/60 data-[state=active]:text-primary">Calendars</TabsTrigger>
        </TabsList>

        <Card className="mt-4">
          <TabsContent value="branches" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Branch Code</th>
                  <th className="px-6 py-3">Branch Name</th>
                  <th className="px-6 py-3">Physical Address</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {branches?.map((b) => (
                  <tr key={b.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-mono font-bold text-ink-black">{b.code}</td>
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{b.name}</td>
                    <td className="px-6 py-3.5 text-stone flex items-center space-x-1">
                      <MapPin className="w-3 h-3 text-stone" />
                      <span>{b.address || "Main Location"}</span>
                    </td>
                    <td className="px-6 py-3.5"><Badge variant="success">Active</Badge></td>
                    <td className="px-6 py-3.5 text-right">
                      <Button variant="ghost" size="sm" onClick={() => openEditModal(b)} className="text-stone hover:text-notion-blue">
                        <Pencil className="w-3 h-3" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="departments" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Department Code</th>
                  <th className="px-6 py-3">Department Name</th>
                  <th className="px-6 py-3">Assigned Branch</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {departments?.map((d) => (
                  <tr key={d.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-mono font-bold text-ink-black">{d.code}</td>
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{d.name}</td>
                    <td className="px-6 py-3.5 text-stone">
                      {branches?.find(b => b.id === d.branchId)?.name || "Company Wide"}
                    </td>
                    <td className="px-6 py-3.5"><Badge variant="success">Active</Badge></td>
                    <td className="px-6 py-3.5 text-right">
                      <Button variant="ghost" size="sm" onClick={() => openEditModal(d)} className="text-stone hover:text-notion-blue">
                        <Pencil className="w-3 h-3" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="designations" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Code</th>
                  <th className="px-6 py-3">Job Title / Designation</th>
                  <th className="px-6 py-3">Pay Grade Level</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {designations?.map((des) => (
                  <tr key={des.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-mono font-bold text-ink-black">{des.code}</td>
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{des.name}</td>
                    <td className="px-6 py-3.5 font-medium text-stone">{des.grade || "Standard"}</td>
                    <td className="px-6 py-3.5"><Badge variant="success">Active</Badge></td>
                    <td className="px-6 py-3.5 text-right">
                      <Button variant="ghost" size="sm" onClick={() => openEditModal(des)} className="text-stone hover:text-notion-blue">
                        <Pencil className="w-3 h-3" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="cost-centers" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Cost Center Code</th>
                  <th className="px-6 py-3">GL Allocation Name</th>
                  <th className="px-6 py-3">Status</th>
                  <th className="px-6 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {costCenters?.map((cc) => (
                  <tr key={cc.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-mono font-bold text-ink-black">{cc.code}</td>
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{cc.name}</td>
                    <td className="px-6 py-3.5"><Badge variant="success">Active</Badge></td>
                    <td className="px-6 py-3.5 text-right">
                      <Button variant="ghost" size="sm" onClick={() => openEditModal(cc)} className="text-stone hover:text-notion-blue">
                        <Pencil className="w-3 h-3" />
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="financial-years" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Label</th>
                  <th className="px-6 py-3">Period</th>
                  <th className="px-6 py-3">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {financialYears?.map((f) => (
                  <tr key={f.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{f.label}</td>
                    <td className="px-6 py-3.5 text-stone">{f.startDate} to {f.endDate}</td>
                    <td className="px-6 py-3.5"><Badge variant="success">{f.isCurrent ? "Current" : "Active"}</Badge></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="calendars" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Period</th>
                  <th className="px-6 py-3">Working Days</th>
                  <th className="px-6 py-3">Freeze Date</th>
                  <th className="px-6 py-3">Payment Date</th>
                  <th className="px-6 py-3">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {calendars?.map((c) => (
                  <tr key={c.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-bold text-ink-black">{c.month}/{c.year}</td>
                    <td className="px-6 py-3.5 font-semibold text-ink-black">{c.workingDays} days</td>
                    <td className="px-6 py-3.5 text-stone">{c.payrollFreezeDate}</td>
                    <td className="px-6 py-3.5 text-stone">{c.paymentDate}</td>
                    <td className="px-6 py-3.5"><Badge variant="success">{c.status}</Badge></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>
        </Card>
      </Tabs>

      {/* Modal for creating/updating active entity with specific fields */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent>
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle className="capitalize">
                {editingId ? "Edit" : "Add New"} {activeTab === "cost-centers" ? "Cost Center" : activeTab.slice(0, -1)}
              </DialogTitle>
              <DialogDescription>
                {editingId ? "Update the structural record details." : "Register a new structural record into the tenant organization."}
              </DialogDescription>
            </DialogHeader>

            {activeMutation.isError && (
              <div className="mt-2 p-3 bg-coral/10 border border-coral/20 rounded-[8px] text-xs text-coral font-medium">
                {activeMutation.error instanceof Error ? activeMutation.error.message : "Action failed."}
              </div>
            )}

            <div className="space-y-4 py-4">
              {/* Code Field (All entities except calendars) */}
              {activeTab !== "calendars" && (
                <div className="space-y-1.5">
                  <Label htmlFor="code">Code Identifier (2-20 Alphanumeric)</Label>
                  <Input 
                    id="code" 
                    placeholder={
                      activeTab === "branches" ? "e.g. HQ" :
                      activeTab === "departments" ? "e.g. ENG" :
                      activeTab === "designations" ? "e.g. SSE" : "e.g. CC-101"
                    } 
                    value={formData.code} 
                    onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })} 
                    required 
                  />
                </div>
              )}

              {/* Name Field (All entities except calendars) */}
              {activeTab !== "calendars" && (
                <div className="space-y-1.5">
                  <Label htmlFor="name">Name / Title</Label>
                  <Input 
                    id="name" 
                    placeholder={
                      activeTab === "branches" ? "e.g. Headquarters" :
                      activeTab === "departments" ? "e.g. Software Engineering" :
                      activeTab === "designations" ? "e.g. Senior Software Engineer" : "e.g. Core Product Operations"
                    } 
                    value={formData.name} 
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })} 
                    required 
                  />
                </div>
              )}

              {/* Branch Specific Field */}
              {activeTab === "branches" && (
                <div className="space-y-1.5">
                  <Label htmlFor="address">Physical Location Address (Optional)</Label>
                  <Input 
                    id="address" 
                    placeholder="e.g. 100 Financial Way, Floor 4, New York, NY" 
                    value={formData.address} 
                    onChange={(e) => setFormData({ ...formData, address: e.target.value })} 
                  />
                </div>
              )}

              {/* Cost Center / Financial Year specific UI goes below if needed. Currently no extra fields for CC. */}
              {activeTab === "financial-years" && (
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label htmlFor="startDate">Start Date</Label>
                    <Input 
                      id="startDate" 
                      type="date"
                      value={formData.startDate} 
                      onChange={(e) => setFormData({ ...formData, startDate: e.target.value })} 
                      required 
                    />
                  </div>
                  <div className="space-y-1.5">
                    <Label htmlFor="endDate">End Date</Label>
                    <Input 
                      id="endDate" 
                      type="date"
                      value={formData.endDate} 
                      onChange={(e) => setFormData({ ...formData, endDate: e.target.value })} 
                      required 
                    />
                  </div>
                </div>
              )}

              {activeTab === "calendars" && (
                <div className="space-y-4">
                  <div className="space-y-1.5">
                    <Label htmlFor="financialYearId">Financial Year</Label>
                    <select
                      id="financialYearId"
                      className="flex h-9 w-full rounded-md border border-ink-black/20 bg-white px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-primary"
                      value={formData.financialYearId}
                      onChange={(e) => setFormData({ ...formData, financialYearId: e.target.value })}
                      required
                    >
                      <option value="" disabled>Select Financial Year</option>
                      {financialYears?.map(f => (
                        <option key={f.id} value={f.id}>{f.label}</option>
                      ))}
                    </select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <Label htmlFor="month">Month (1-12)</Label>
                      <Input 
                        id="month" 
                        type="number" min="1" max="12"
                        value={formData.month} 
                        onChange={(e) => setFormData({ ...formData, month: Number(e.target.value) })} 
                        required 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <Label htmlFor="year">Year</Label>
                      <Input 
                        id="year" 
                        type="number"
                        value={formData.year} 
                        onChange={(e) => setFormData({ ...formData, year: Number(e.target.value) })} 
                        required 
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-1.5">
                      <Label htmlFor="freezeDate">Freeze Date</Label>
                      <Input 
                        id="freezeDate" 
                        type="date"
                        value={formData.freezeDate} 
                        onChange={(e) => setFormData({ ...formData, freezeDate: e.target.value })} 
                        required 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <Label htmlFor="paymentDate">Payment Date</Label>
                      <Input 
                        id="paymentDate" 
                        type="date"
                        value={formData.paymentDate} 
                        onChange={(e) => setFormData({ ...formData, paymentDate: e.target.value })} 
                        required 
                      />
                    </div>
                    <div className="space-y-1.5">
                      <Label htmlFor="workingDays">Working Days</Label>
                      <Input 
                        id="workingDays" 
                        type="number" min="1" max="31"
                        value={formData.workingDays} 
                        onChange={(e) => setFormData({ ...formData, workingDays: Number(e.target.value) })} 
                        required 
                      />
                    </div>
                  </div>
                </div>
              )}

              {/* Department Specific Field: Optional Branch Assignment */}
              {activeTab === "departments" && (
                <div className="space-y-1.5">
                  <Label htmlFor="branchId">Assign to Branch (Optional)</Label>
                  <select 
                    id="branchId"
                    value={formData.branchId}
                    onChange={(e) => setFormData({ ...formData, branchId: e.target.value })}
                    className="flex h-9 w-full rounded-md border border-ink-black/15 bg-pure-white px-3 py-1 text-xs text-ink-black shadow-xs focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-notion-blue"
                  >
                    <option value="">Company-Wide (All Branches)</option>
                    {branches?.map((b) => (
                      <option key={b.id} value={b.id}>{b.name} ({b.code})</option>
                    ))}
                  </select>
                </div>
              )}

              {/* Designation Specific Field: Pay Grade */}
              {activeTab === "designations" && (
                <div className="space-y-1.5">
                  <Label htmlFor="grade">Pay Grade Scale (Optional)</Label>
                  <Input 
                    id="grade" 
                    placeholder="e.g. BPS-18 or Executive-Level 2" 
                    value={formData.grade} 
                    onChange={(e) => setFormData({ ...formData, grade: e.target.value })} 
                  />
                </div>
              )}
            </div>

            <DialogFooter>
              <Button type="button" variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button>
              <Button type="submit" variant="primary" disabled={activeMutation.isPending}>
                {activeMutation.isPending ? "Saving..." : "Save Record"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
