"use client"

import * as React from "react"
import { SectionHeader } from "@/components/ui/section-header"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { 
  Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter 
} from "@/components/ui/dialog"
import { Search, Plus, CheckCircle2, XCircle, Building2, Loader2, Pencil } from "lucide-react"
import { useCompanies, useCreateCompany, useUpdateCompany, useActivateCompany, useDeactivateCompany } from "@/hooks/useTenant"

export default function CompaniesDirectoryPage() {
  const { data: companies, isLoading } = useCompanies()
  const createCompany = useCreateCompany()
  const updateCompany = useUpdateCompany()
  const activateCompany = useActivateCompany()
  const deactivateCompany = useDeactivateCompany()

  const [isModalOpen, setIsModalOpen] = React.useState(false)
  const [editingId, setEditingId] = React.useState<string | null>(null)
  const [searchTerm, setSearchTerm] = React.useState("")
  const [formData, setFormData] = React.useState({
    code: "",
    name: "",
    contactEmail: "",
    contactPhone: "",
  })

  const filteredCompanies = companies?.filter(
    (c) =>
      c.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.code.toLowerCase().includes(searchTerm.toLowerCase()) ||
      c.contactEmail?.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const openEditModal = (company: any) => {
    setEditingId(company.id)
    setFormData({
      code: company.code || "",
      name: company.name || "",
      contactEmail: company.contactEmail || "",
      contactPhone: company.contactPhone || "",
    })
    setIsModalOpen(true)
  }

  const resetForm = () => {
    setEditingId(null)
    setFormData({ code: "", name: "", contactEmail: "", contactPhone: "" })
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()

    if (editingId) {
      updateCompany.mutate(
        { id: editingId, data: { id: editingId, ...formData } },
        {
          onSuccess: () => {
            setIsModalOpen(false)
            resetForm()
          },
        }
      )
      return
    }

    createCompany.mutate(formData, {
      onSuccess: () => {
        setIsModalOpen(false)
        resetForm()
      },
    })
  }

  const activeMutation = editingId ? updateCompany : createCompany

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center border-b border-ink-black/10 pb-5">
        <SectionHeader 
          title="Companies" 
          subhead="Manage multi-tenant platform organizations and enterprise accounts."
        />
        <Button variant="primary" onClick={() => { resetForm(); setIsModalOpen(true); }}>
          <Plus className="w-4 h-4 mr-2" />
          Register Company
        </Button>
      </div>

      <Card>
        <div className="p-4 border-b border-ink-black/10 flex justify-between items-center bg-paper-warmth/40">
          <div className="relative">
            <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-stone" />
            <Input 
              type="text" 
              placeholder="Search by name, code, or email..." 
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-80 pl-9"
            />
          </div>
          <div className="text-xs text-stone font-medium">
            Showing <span className="text-ink-black font-semibold">{filteredCompanies?.length || 0}</span> Companies
          </div>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full text-xs text-left">
            <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
              <tr>
                <th className="px-6 py-3">Code</th>
                <th className="px-6 py-3">Company Name</th>
                <th className="px-6 py-3">Contact Email</th>
                <th className="px-6 py-3">Contact Phone</th>
                <th className="px-6 py-3">Status</th>
                <th className="px-6 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-black/5">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-stone">
                    <Loader2 className="w-5 h-5 animate-spin mx-auto mb-2 text-notion-blue" />
                    Loading companies...
                  </td>
                </tr>
              ) : filteredCompanies?.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-stone">
                    No matching companies found.
                  </td>
                </tr>
              ) : (
                filteredCompanies?.map((company) => (
                  <tr key={company.id} className="hover:bg-black/5 transition-colors">
                    <td className="px-6 py-4 font-mono font-bold text-ink-black">{company.code}</td>
                    <td className="px-6 py-4 font-semibold text-ink-black">
                      <div className="flex items-center space-x-2">
                        <Building2 className="w-4 h-4 text-notion-blue shrink-0" />
                        <span>{company.name}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 text-stone">{company.contactEmail || "N/A"}</td>
                    <td className="px-6 py-4 text-stone">{company.contactPhone || "N/A"}</td>
                    <td className="px-6 py-4">
                      <Badge variant={company.isActive ? "success" : "default"}>
                        {company.isActive ? "Active" : "Inactive"}
                      </Badge>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <Button 
                        variant="ghost" 
                        size="sm" 
                        onClick={() => openEditModal(company)}
                        className="text-stone hover:text-notion-blue"
                      >
                        <Pencil className="w-4 h-4" />
                      </Button>
                      {company.isActive ? (
                        <Button 
                          variant="outline" 
                          size="sm" 
                          onClick={() => deactivateCompany.mutate(company.id)}
                          className="text-coral hover:bg-coral/10 hover:border-coral/30"
                        >
                          <XCircle className="w-3.5 h-3.5 mr-1" />
                          Deactivate
                        </Button>
                      ) : (
                        <Button 
                          variant="outline" 
                          size="sm" 
                          onClick={() => activateCompany.mutate(company.id)}
                          className="text-emerald-600 hover:bg-emerald-50 hover:border-emerald-200"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5 mr-1" />
                          Activate
                        </Button>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Create / Edit Company Modal */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent>
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>{editingId ? "Edit Company" : "Register New Company"}</DialogTitle>
              <DialogDescription>
                {editingId ? "Update the details of this company." : "Add a new tenant organization to the payroll platform."}
              </DialogDescription>
            </DialogHeader>

            {activeMutation.isError && (
              <div className="mt-2 p-3 bg-coral/10 border border-coral/20 rounded-[8px] text-xs text-coral font-medium">
                {activeMutation.error instanceof Error ? activeMutation.error.message : "Failed to process request."}
              </div>
            )}

            <div className="space-y-4 py-4">
              <div className="space-y-1.5">
                <Label htmlFor="code">Company Code (Short ID)</Label>
                <Input 
                  id="code" 
                  placeholder="e.g. ACME" 
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  required
                />
              </div>

              <div className="space-y-1.5">
                <Label htmlFor="name">Full Legal Name</Label>
                <Input 
                  id="name" 
                  placeholder="e.g. Acme Corporation Inc." 
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1.5">
                  <Label htmlFor="email">Contact Email</Label>
                  <Input 
                    id="email" 
                    type="email" 
                    placeholder="contact@company.com" 
                    value={formData.contactEmail}
                    onChange={(e) => setFormData({ ...formData, contactEmail: e.target.value })}
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="phone">Contact Phone</Label>
                  <Input 
                    id="phone" 
                    placeholder="+1 (555) 000-0000" 
                    value={formData.contactPhone}
                    onChange={(e) => setFormData({ ...formData, contactPhone: e.target.value })}
                  />
                </div>
              </div>
            </div>

            <DialogFooter>
              <Button type="button" variant="ghost" onClick={() => setIsModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" variant="primary" disabled={activeMutation.isPending}>
                {activeMutation.isPending ? "Saving..." : (editingId ? "Save Changes" : "Create Tenant")}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
