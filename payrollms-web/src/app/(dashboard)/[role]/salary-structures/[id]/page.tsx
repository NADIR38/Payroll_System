"use client";

import { useEffect, useState, use } from "react";
import { useRouter } from "next/navigation";
import { 
  SalaryApi, 
  SalaryStructureResponse, 
  SalaryComponentResponse, 
  SalaryStructureComponentResponse 
} from "@/lib/api/salary";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from "@/components/ui/dialog";
import { SectionHeader } from "@/components/ui/section-header";
import { ArrowLeft, Settings2, Trash2, PlusCircle, CheckCircle2, AlertCircle, PlayCircle } from "lucide-react";
import { toast } from "sonner";
import { isAxiosError } from "axios";

export default function SalaryStructureBuilderPage({ params }: { params: Promise<{ role: string; id: string }> }) {
  const router = useRouter();
  const { role, id } = use(params);
  
  const [structure, setStructure] = useState<SalaryStructureResponse | null>(null);
  const [allComponents, setAllComponents] = useState<SalaryComponentResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Modal State
  const [isConfigOpen, setIsConfigOpen] = useState(false);
  const [selectedAssignedComponent, setSelectedAssignedComponent] = useState<SalaryStructureComponentResponse | null>(null);
  
  // Rule Form State
  const [calcType, setCalcType] = useState("Fixed");
  const [fixedAmount, setFixedAmount] = useState<number>(0);
  const [formula, setFormula] = useState("");
  
  // Validation State
  const [validationResult, setValidationResult] = useState<{isValid: boolean, message?: string, evaluatedResult?: number} | null>(null);
  const [isValidating, setIsValidating] = useState(false);

  useEffect(() => {
    fetchData();
  }, [id]);

  const fetchData = async () => {
    try {
      const [structData, compsData] = await Promise.all([
        SalaryApi.getStructureById(id),
        SalaryApi.getComponents()
      ]);
      setStructure(structData);
      setAllComponents(compsData);
    } catch (error) {
      console.error("Failed to load builder data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddComponent = async (componentId: string) => {
    try {
      await SalaryApi.addComponent(id, {
        salaryStructureId: id,
        salaryComponentId: componentId,
        formulaExpression: "0",
        fixedAmount: 0,
        sequence: (structure?.components.length || 0) + 1
      });
      await fetchData();
    } catch (error) {
      console.error("Failed to add component:", error);
      if (isAxiosError(error) && error.response?.data?.errors) {
        const errs = error.response.data.errors as Record<string, string[]>;
        const msg = Object.values(errs).flat().join("\n");
        toast.error(`Validation Failed:\n${msg}`);
      } else {
        toast.error("Failed to add component. Check console.");
      }
    }
  };

  const handleRemoveComponent = async (componentId: string) => {
    try {
      await SalaryApi.removeComponent(id, componentId);
      toast.success("Component removed from structure.");
      await fetchData();
    } catch (error) {
      console.error("Failed to remove component:", error);
      if (isAxiosError(error) && error.response?.data?.title) {
        toast.error(error.response.data.title);
      } else {
        toast.error("Failed to remove component from structure.");
      }
    }
  };

  const openConfig = (structComp: SalaryStructureComponentResponse) => {
    setSelectedAssignedComponent(structComp);
    setCalcType(structComp.calculationMethod || "Fixed");
    setFormula(structComp.formulaExpression || "");
    setFixedAmount(structComp.fixedAmount || 0);
    setValidationResult(null);
    setIsConfigOpen(true);
  };

  const handleTestFormula = async () => {
    if (!formula.trim()) return;
    try {
      setIsValidating(true);
      const res = await SalaryApi.validateFormula({ formula });
      setValidationResult({
        isValid: res.isValid,
        message: res.errorMessage,
        evaluatedResult: res.evaluatedResult
      });
    } catch (error: any) {
      setValidationResult({
        isValid: false,
        message: error.message || "Failed to parse formula"
      });
    } finally {
      setIsValidating(false);
    }
  };

  const handleSaveConfig = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedAssignedComponent) return;

    try {
      const payload = {
        salaryStructureId: id,
        structureComponentId: selectedAssignedComponent.id,
        formulaExpression: calcType === "Formula" ? formula : "0",
        fixedAmount: calcType === "Fixed" ? (Number(fixedAmount) || 0) : null,
        sequence: selectedAssignedComponent.sequence
      };

      await SalaryApi.updateComponentInStructure(id, selectedAssignedComponent.id, payload);

      setIsConfigOpen(false);
      await fetchData();
      toast.success("Rule saved successfully!");
    } catch (error) {
      if (isAxiosError(error) && error.response?.data?.errors) {
        const errs = error.response.data.errors as Record<string, string[]>;
        const msg = Object.values(errs).flat().join("\n");
        toast.error(`Validation Failed:\n${msg}`);
      } else {
        toast.error("Failed to save rule. Ensure formula syntax is valid.");
      }
    }
  };

  if (isLoading) return <div className="p-8 text-center text-slate-500">Loading builder...</div>;
  if (!structure) return <div className="p-8 text-center text-red-500">Structure not found.</div>;

  // Filter available components (those not already in the structure)
  const assignedIds = new Set(structure.components.map(c => c.salaryComponentId));
  const availableComponents = allComponents.filter(c => !assignedIds.has(c.id) && c.isActive);

  return (
    <div className="space-y-6 max-w-7xl mx-auto pb-12">
      <div className="flex items-center gap-4">
        <Button 
          variant="outline" 
          size="icon" 
          onClick={() => router.push(`/${role}/salary-structures`)}
          className="shrink-0"
        >
          <ArrowLeft className="w-4 h-4" />
        </Button>
        <SectionHeader 
          title={structure.name}
          subhead="Formula Builder & Component Orchestration"
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left Pane: Available Components */}
        <div className="lg:col-span-1 space-y-4">
          <Card className="border-slate-200 shadow-sm bg-slate-50/50">
            <CardHeader className="pb-3 border-b border-slate-200">
              <CardTitle className="text-base">Available Elements</CardTitle>
              <CardDescription>Click to add to structure.</CardDescription>
            </CardHeader>
            <CardContent className="pt-4 h-[600px] overflow-y-auto space-y-2">
              {availableComponents.length === 0 ? (
                <div className="text-sm text-slate-500 text-center py-8">
                  All active components have been added.
                </div>
              ) : (
                availableComponents.map(comp => (
                  <div 
                    key={comp.id} 
                    className="flex items-center justify-between p-3 bg-white border border-slate-200 rounded-lg shadow-sm hover:border-primary/50 hover:shadow transition-all group cursor-pointer"
                    onClick={() => handleAddComponent(comp.id)}
                  >
                    <div>
                      <div className="font-medium text-slate-900 text-sm">{comp.name}</div>
                      <code className="text-[10px] text-slate-500">{comp.code}</code>
                    </div>
                    <Button variant="ghost" size="icon" className="h-8 w-8 text-primary opacity-0 group-hover:opacity-100 transition-opacity">
                      <PlusCircle className="w-4 h-4" />
                    </Button>
                  </div>
                ))
              )}
            </CardContent>
          </Card>
        </div>

        {/* Right Pane: Assigned Components & Rules */}
        <div className="lg:col-span-2 space-y-4">
          <Card className="border-slate-200 shadow-sm">
            <CardHeader className="pb-3 border-b border-slate-200 bg-slate-50">
              <div className="flex justify-between items-center">
                <div>
                  <CardTitle className="text-base">Assigned Components</CardTitle>
                  <CardDescription>Configure rules and calculation order.</CardDescription>
                </div>
                <Badge className="bg-white text-slate-800 border border-slate-200">
                  {structure.components.length} Items
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="pt-4 min-h-[600px] bg-slate-50/20">
              {structure.components.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full py-20 text-slate-400">
                  <div className="p-4 rounded-full bg-slate-100 mb-4">
                    <Settings2 className="w-8 h-8 text-slate-300" />
                  </div>
                  <p>Structure is empty. Add components from the left panel.</p>
                </div>
              ) : (
                <div className="space-y-3">
                  {structure.components.map((structComp) => (
                    <div 
                      key={structComp.id}
                      className="flex flex-col bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden"
                    >
                      <div className="flex items-center justify-between p-4 border-b border-slate-100">
                        <div className="flex items-center gap-3">
                          <Badge variant={structComp.componentType === "Allowance" ? "success" : "warning"}>
                            {structComp.componentType}
                          </Badge>
                          <div>
                            <div className="font-medium text-slate-900">{structComp.componentName}</div>
                            <div className="text-xs text-slate-500 font-mono">{structComp.componentCode}</div>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Button variant="outline" size="sm" className="gap-2 text-primary" onClick={() => openConfig(structComp)}>
                            <Settings2 className="w-3.5 h-3.5" /> Configure Rule
                          </Button>
                          <Button variant="ghost" size="icon" className="text-slate-400 hover:text-red-500" onClick={() => handleRemoveComponent(structComp.id)}>
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </div>
                      <div className="p-3 bg-slate-50 text-sm flex items-center justify-between">
                        <div className="text-sm">
                          Calculation: <strong className="text-slate-900">{structComp.calculationMethod}</strong>
                        </div>
                        {structComp.calculationMethod === "Formula" ? (
                          <div className="bg-slate-100 text-slate-700 font-mono text-xs p-2 rounded">
                            {structComp.formulaExpression || "No formula set"}
                          </div>
                        ) : (
                          <div className="bg-slate-100 text-slate-700 font-medium text-xs p-2 rounded">
                            ${(structComp.fixedAmount || 0).toLocaleString()}
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

      </div>

      {/* Configuration Dialog */}
      <Dialog open={isConfigOpen} onOpenChange={setIsConfigOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Configure Component</DialogTitle>
            <DialogDescription>
              {selectedAssignedComponent?.componentName} ({selectedAssignedComponent?.componentCode})
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSaveConfig} className="space-y-6 py-4">
            <div className="space-y-3">
              <Label>Calculation Strategy</Label>
              <div className="grid grid-cols-2 gap-3">
                <Button 
                  type="button" 
                  variant={calcType === "Fixed" ? "primary" : "outline"}
                  onClick={() => setCalcType("Fixed")}
                  className="w-full"
                >
                  Fixed Amount
                </Button>
                <Button 
                  type="button" 
                  variant={calcType === "Formula" ? "primary" : "outline"}
                  onClick={() => setCalcType("Formula")}
                  className="w-full"
                >
                  Formula Driven
                </Button>
              </div>
            </div>

            {calcType === "Fixed" ? (
              <div className="space-y-2 animate-in fade-in slide-in-from-top-2">
                <Label htmlFor="fixedAmount">Flat Value</Label>
                <div className="relative">
                  <span className="absolute left-3 top-2.5 text-slate-500">$</span>
                  <Input 
                    id="fixedAmount" 
                    type="number"
                    min="0"
                    step="0.01"
                    required
                    className="pl-7"
                    value={fixedAmount}
                    onChange={(e) => setFixedAmount(Number(e.target.value))}
                  />
                </div>
              </div>
            ) : (
              <div className="space-y-4 animate-in fade-in slide-in-from-top-2">
                <div className="space-y-2">
                  <Label htmlFor="formula">NCalc Formula Expression</Label>
                  <Input 
                    id="formula" 
                    required
                    value={formula}
                    onChange={(e) => {
                      setFormula(e.target.value);
                      setValidationResult(null);
                    }}
                    placeholder="e.g. BaseSalary * 0.15"
                    className="font-mono text-sm"
                  />
                  <div className="text-[11px] text-slate-500 bg-slate-50 p-2 rounded border border-slate-100 mt-2">
                    <p className="font-semibold mb-1">Available Variables & Syntax:</p>
                    <ul className="list-disc pl-4 space-y-1">
                      <li><code className="text-primary">BASE</code>, <code className="text-primary">GROSS</code>, <code className="text-primary">EARNINGS</code>, <code className="text-primary">WORKING_DAYS</code></li>
                      <li>Any prior component code (e.g. <code className="text-primary">BASIC_SAL</code>)</li>
                      <li>Math functions: <code className="text-primary">If(cond, true, false)</code>, <code className="text-primary">Round(val, 2)</code>, <code className="text-primary">Abs(val)</code>, <code className="text-primary">Min(a, b)</code>, <code className="text-primary">Max(a, b)</code></li>
                    </ul>
                  </div>
                </div>

                <div className="p-4 bg-slate-50 rounded-lg border border-slate-200 space-y-3">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-slate-700">Syntax Check</span>
                    <Button 
                      type="button" 
                      variant="outline" 
                      size="sm" 
                      onClick={handleTestFormula}
                      disabled={isValidating || !formula.trim()}
                      className="h-8 gap-1"
                    >
                      <PlayCircle className="w-3.5 h-3.5" /> Test
                    </Button>
                  </div>
                  
                  {validationResult && (
                    <div className={`p-3 text-sm rounded flex items-start gap-2 ${validationResult.isValid ? 'bg-green-50 text-green-800 border border-green-200' : 'bg-red-50 text-red-800 border border-red-200'}`}>
                      {validationResult.isValid ? (
                        <>
                          <CheckCircle2 className="w-4 h-4 shrink-0 mt-0.5" />
                          <div>
                            <div className="font-medium">Valid Expression</div>
                            <div className="text-green-700 mt-1 opacity-80">Test eval: {validationResult.evaluatedResult} (Assuming inputs = 0)</div>
                          </div>
                        </>
                      ) : (
                        <>
                          <AlertCircle className="w-4 h-4 shrink-0 mt-0.5" />
                          <div>
                            <div className="font-medium">Parse Error</div>
                            <div className="text-red-700 mt-1 opacity-80 text-xs font-mono">{validationResult.message}</div>
                          </div>
                        </>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}

            <DialogFooter className="pt-4 border-t border-slate-100">
              <Button type="button" variant="outline" onClick={() => setIsConfigOpen(false)}>Cancel</Button>
              <Button type="submit">Save Rule</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
