"use client";

import { useEffect, useState } from "react";
import { useRouter, useParams } from "next/navigation";
import { SalaryApi, SalaryStructureResponse } from "@/lib/api/salary";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { SectionHeader } from "@/components/ui/section-header";
import { Plus, Trash2, Edit, Calculator } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function SalaryStructuresPage() {
  const router = useRouter();
  const params = useParams();
  const role = params.role as string;
  
  const [structures, setStructures] = useState<SalaryStructureResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Dialog State
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [formData, setFormData] = useState({ name: "", description: "", code: "", effectiveFrom: new Date().toISOString().split('T')[0] });
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchStructures();
  }, []);

  const fetchStructures = async () => {
    try {
      setIsLoading(true);
      const data = await SalaryApi.getStructures();
      setStructures(data);
    } catch (error) {
      console.error("Failed to load salary structures:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const openCreateDialog = () => {
    setFormData({ name: "", description: "", code: "", effectiveFrom: new Date().toISOString().split('T')[0] });
    setIsDialogOpen(true);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSubmitting(true);
      const res = await SalaryApi.createStructure({
        name: formData.name,
        code: formData.code,
        effectiveFrom: formData.effectiveFrom,
        description: formData.description
      });
      setIsDialogOpen(false);
      // Navigate straight to builder
      router.push(`/${role}/salary-structures/${res.id}`);
    } catch (error) {
      console.error("Failed to create structure:", error);
      if (isAxiosError(error) && error.response?.data?.errors) {
        const errs = error.response.data.errors as Record<string, string[]>;
        const msg = Object.values(errs).flat().join("\n");
        toast.error(`Validation Failed:\n${msg}`);
      } else {
        toast.error("Failed to create structure. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to deactivate this structure? Existing employees will remain on it until updated.")) return;
    try {
      await SalaryApi.deactivateStructure(id);
      await fetchStructures();
    } catch (error) {
      console.error("Failed to deactivate structure:", error);
      toast.error("Failed to deactivate structure.");
    }
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <SectionHeader 
          title="Salary Structures" 
          subhead="Define complete remuneration packages for employee assignment."
        />
        <Button onClick={openCreateDialog} className="gap-2 shrink-0">
          <Plus className="w-4 h-4" />
          Create Structure
        </Button>
      </div>

      <Card className="border-slate-200 shadow-sm">
        <CardHeader>
          <CardTitle className="text-lg">Configured Packages</CardTitle>
          <CardDescription>All active salary templates available in the system.</CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex justify-center p-8 text-slate-500">Loading structures...</div>
          ) : structures.length === 0 ? (
            <div className="text-center p-8 text-slate-500 bg-slate-50 rounded-lg border border-dashed border-slate-200">
              No salary structures configured yet. Click "Create Structure" to start.
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {structures.map((struct) => (
                <Card key={struct.id} className="border-slate-200 hover:border-primary/50 transition-colors shadow-sm relative overflow-hidden group">
                  {!struct.isActive && (
                    <div className="absolute top-0 right-0 p-4">
                      <Badge variant="warning">Inactive</Badge>
                    </div>
                  )}
                  <CardHeader className="pb-2">
                    <CardTitle className="text-lg text-slate-900 pr-16 leading-tight">
                      {struct.name}
                    </CardTitle>
                    <CardDescription className="line-clamp-2 mt-2">
                      {struct.description || "No description provided."}
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="pt-4">
                    <div className="flex items-center text-sm text-slate-500 mb-6">
                      <Calculator className="w-4 h-4 mr-2" />
                      {struct.components.length} components attached
                    </div>
                    
                    <div className="flex justify-between items-center border-t border-slate-100 pt-4 mt-auto">
                      <Button 
                        variant="outline" 
                        size="sm"
                        className="text-primary hover:bg-primary/5"
                        onClick={() => router.push(`/${role}/salary-structures/${struct.id}`)}
                      >
                        Open Builder
                      </Button>
                      
                      {struct.isActive && (
                        <Button 
                          variant="ghost" 
                          size="icon"
                          onClick={() => handleDelete(struct.id)}
                          className="opacity-0 group-hover:opacity-100 transition-opacity"
                        >
                          <Trash2 className="w-4 h-4 text-red-500 hover:text-red-700" />
                        </Button>
                      )}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create New Salary Structure</DialogTitle>
            <DialogDescription>
              Name your new remuneration package before adding components to it.
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleCreate}>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name">Structure Name</Label>
                <Input 
                  id="name" 
                  required 
                  value={formData.name} 
                  onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                  placeholder="e.g. C-Level Executive Package"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description (Optional)</Label>
                <Input 
                  id="description" 
                  value={formData.description} 
                  onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                  placeholder="Brief description of who this is for."
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="code">Structure Code</Label>
                  <Input 
                    id="code" 
                    required 
                    value={formData.code} 
                    onChange={(e) => setFormData(prev => ({ ...prev, code: e.target.value.toUpperCase() }))}
                    placeholder="e.g. EXEC_2026"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="effectiveFrom">Effective From</Label>
                  <Input 
                    id="effectiveFrom" 
                    type="date"
                    required 
                    value={formData.effectiveFrom} 
                    onChange={(e) => setFormData(prev => ({ ...prev, effectiveFrom: e.target.value }))}
                  />
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsDialogOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Creating..." : "Create & Open Builder"}</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
