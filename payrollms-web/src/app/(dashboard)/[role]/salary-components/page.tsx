"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { SalaryApi, SalaryComponentResponse, CreateSalaryComponentCommand, UpdateSalaryComponentCommand } from "@/lib/api/salary";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { SectionHeader } from "@/components/ui/section-header";
import { Plus, Trash2, Edit, Settings2 } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function SalaryComponentsPage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;
  
  const [components, setComponents] = useState<SalaryComponentResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSeeding, setIsSeeding] = useState(false);

  // Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    name: "",
    code: "",
    type: "Allowance",
    isTaxable: true
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchComponents();
  }, []);

  const fetchComponents = async () => {
    try {
      setIsLoading(true);
      const data = await SalaryApi.getComponents();
      setComponents(data);
    } catch (error) {
      console.error("Failed to load salary components:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSeed = async () => {
    try {
      setIsSeeding(true);
      await SalaryApi.seedComponents();
      await fetchComponents();
    } catch (error) {
      console.error("Failed to seed components:", error);
      alert("Seeding failed or components already exist.");
    } finally {
      setIsSeeding(false);
    }
  };

  const openCreateDialog = () => {
    setEditingId(null);
    setFormData({ name: "", code: "", type: "Allowance", isTaxable: true });
    setIsDialogOpen(true);
  };

  const openEditDialog = (comp: SalaryComponentResponse) => {
    setEditingId(comp.id);
    setFormData({ name: comp.name, code: comp.code, type: comp.type, isTaxable: comp.isTaxable });
    setIsDialogOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      if (editingId) {
        await SalaryApi.updateComponent(editingId, { 
          id: editingId, 
          name: formData.name, 
          isTaxable: formData.isTaxable 
        });
      } else {
        await SalaryApi.createComponent({
          name: formData.name,
          code: formData.code,
          type: formData.type,
          isTaxable: formData.isTaxable
        });
      }
      setIsDialogOpen(false);
      await fetchComponents();
      toast.success("Component saved successfully!");
    } catch (error) {
      console.error("Failed to save component:", error);
      if (isAxiosError(error) && error.response?.data?.errors) {
        const errs = error.response.data.errors as Record<string, string[]>;
        const msg = Object.values(errs).flat().join("\n");
        toast.error(`Validation Failed:\n${msg}`);
      } else {
        toast.error("Failed to save component. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this component?")) return;
    try {
      await SalaryApi.deactivateComponent(id);
      await fetchComponents();
      toast.success("Component deactivated!");
    } catch (error) {
      console.error("Failed to deactivate component:", error);
      toast.error("Failed to deactivate. It might be in use.");
    }
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <SectionHeader 
          title="Salary Components" 
          subhead="Manage the building blocks of your salary structures."
        />
        <div className="flex gap-2">
          <Button variant="outline" onClick={handleSeed} disabled={isSeeding || isLoading} className="gap-2 shrink-0">
            <Settings2 className="w-4 h-4" />
            {isSeeding ? "Seeding..." : "Seed Defaults"}
          </Button>
          <Button onClick={openCreateDialog} className="gap-2 shrink-0">
            <Plus className="w-4 h-4" />
            New Component
          </Button>
        </div>
      </div>

      <Card className="border-slate-200 shadow-sm">
        <CardHeader>
          <CardTitle className="text-lg">Configured Elements</CardTitle>
          <CardDescription>Allowances and deductions available for formula calculation.</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex justify-center p-8 text-slate-500">Loading components...</div>
          ) : components.length === 0 ? (
            <div className="text-center p-8 text-slate-500 bg-slate-50 rounded-lg border border-dashed border-slate-200">
              No components configured yet. Click "Seed Defaults" to load standard elements.
            </div>
          ) : (
            <div className="rounded-md border border-slate-200 overflow-hidden">
              <table className="w-full text-sm text-left">
                <thead className="bg-slate-50 text-slate-600 font-medium border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-3">Component Name</th>
                    <th className="px-4 py-3">Code</th>
                    <th className="px-4 py-3">Type</th>
                    <th className="px-4 py-3">Taxable</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 bg-white">
                  {components.map((comp) => (
                    <tr key={comp.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-4 py-3 font-medium text-slate-900">
                        {comp.name}
                        {comp.isCustom && <Badge className="bg-transparent border border-slate-200 text-slate-600 ml-2 text-[10px]">Custom</Badge>}
                      </td>
                      <td className="px-4 py-3">
                        <code className="text-xs font-mono bg-slate-100 px-1 py-0.5 rounded">{comp.code}</code>
                      </td>
                      <td className="px-4 py-3">
                        <Badge variant={comp.type === "Allowance" ? "success" : "warning"}>
                          {comp.type}
                        </Badge>
                      </td>
                      <td className="px-4 py-3">
                        {comp.isTaxable ? "Yes" : "No"}
                      </td>
                      <td className="px-4 py-3">
                        {comp.isActive ? (
                           <Badge variant="default">Active</Badge>
                        ) : (
                           <Badge variant="warning">Inactive</Badge>
                        )}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <div className="flex justify-end gap-2">
                          <Button 
                            variant="ghost" 
                            size="icon"
                            onClick={() => openEditDialog(comp)}
                          >
                            <Edit className="w-4 h-4 text-slate-500 hover:text-slate-900" />
                          </Button>
                          {comp.isActive && (
                            <Button 
                              variant="ghost" 
                              size="icon"
                              onClick={() => handleDelete(comp.id)}
                            >
                              <Trash2 className="w-4 h-4 text-red-500 hover:text-red-700" />
                            </Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editingId ? "Edit Salary Component" : "Create Salary Component"}</DialogTitle>
            <DialogDescription>
              {editingId ? "Update details for this component." : "Define a new allowance or deduction type."}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSave}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name">Component Name</Label>
                <Input 
                  id="name" 
                  required 
                  value={formData.name} 
                  onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="code">Code (Used in formulas)</Label>
                <Input 
                  id="code" 
                  required 
                  disabled={!!editingId} // Code is read-only when editing
                  value={formData.code} 
                  onChange={(e) => setFormData(prev => ({ ...prev, code: e.target.value.toUpperCase() }))}
                  placeholder="e.g. BASIC_SALARY"
                />
              </div>
              {!editingId && (
                <div className="space-y-2">
                  <Label htmlFor="type">Type</Label>
                  <select 
                    id="type"
                    className="flex h-10 w-full rounded-md border border-slate-200 bg-white px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-950"
                    value={formData.type}
                    onChange={(e) => setFormData(prev => ({ ...prev, type: e.target.value }))}
                  >
                    <option value="Allowance">Allowance (Earning)</option>
                    <option value="Deduction">Deduction</option>
                  </select>
                </div>
              )}
              <div className="flex items-center space-x-2 pt-2">
                <input
                  type="checkbox"
                  id="isTaxable"
                  className="w-4 h-4 rounded border-slate-300 text-primary focus:ring-primary"
                  checked={formData.isTaxable}
                  onChange={(e) => setFormData(prev => ({ ...prev, isTaxable: e.target.checked }))}
                />
                <Label htmlFor="isTaxable" className="font-normal">
                  This component is taxable
                </Label>
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Saving..." : "Save Component"}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
