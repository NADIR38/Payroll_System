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
import { Plus, Calendar, Snowflake, Lock, RefreshCw, CheckCircle, AlertTriangle } from "lucide-react"
import { 
  useFinancialYears, useCreateFinancialYear, useMarkFinancialYearCurrent,
  usePayrollCalendars, useFreezePayrollCalendar, useClosePayrollCalendar, useReopenPayrollCalendar
} from "@/hooks/useTenant"

export default function FinancialOperationsPage() {
  const { data: financialYears } = useFinancialYears()
  const { data: calendars } = usePayrollCalendars()

  const createFY = useCreateFinancialYear()
  const markCurrent = useMarkFinancialYearCurrent()
  const freezeCalendar = useFreezePayrollCalendar()
  const closeCalendar = useClosePayrollCalendar()
  const reopenCalendar = useReopenPayrollCalendar()

  const [activeTab, setActiveTab] = React.useState("financial-years")
  const [isModalOpen, setIsModalOpen] = React.useState(false)
  const [formData, setFormData] = React.useState({ label: "", startDate: "", endDate: "" })

  const handleCreateFY = (e: React.FormEvent) => {
    e.preventDefault()
    createFY.mutate(formData, { onSuccess: () => setIsModalOpen(false) })
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center border-b border-ink-black/10 pb-5">
        <SectionHeader 
          title="Financial Operations" 
          subhead="Manage fiscal year anchors and monthly payroll operational freeze cycles."
        />
        <Button variant="primary" onClick={() => setIsModalOpen(true)}>
          <Plus className="w-4 h-4 mr-2" />
          Create Fiscal Year
        </Button>
      </div>

      <Tabs defaultValue="financial-years" onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="financial-years" className="flex items-center space-x-2">
            <Calendar className="w-3.5 h-3.5" />
            <span>Financial Years</span>
          </TabsTrigger>
          <TabsTrigger value="calendars" className="flex items-center space-x-2">
            <Snowflake className="w-3.5 h-3.5" />
            <span>Payroll Calendars</span>
          </TabsTrigger>
        </TabsList>

        <Card className="mt-4">
          <TabsContent value="financial-years" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Label</th>
                  <th className="px-6 py-3">Start Date</th>
                  <th className="px-6 py-3">End Date</th>
                  <th className="px-6 py-3">Active Baseline</th>
                  <th className="px-6 py-3 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {financialYears?.map((fy) => (
                  <tr key={fy.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-bold text-ink-black">{fy.label}</td>
                    <td className="px-6 py-3.5 font-mono text-stone">{fy.startDate}</td>
                    <td className="px-6 py-3.5 font-mono text-stone">{fy.endDate}</td>
                    <td className="px-6 py-3.5">
                      {fy.isCurrent ? (
                        <Badge variant="success">Current Active FY</Badge>
                      ) : (
                        <Badge variant="default">Inactive</Badge>
                      )}
                    </td>
                    <td className="px-6 py-3.5 text-right">
                      {!fy.isCurrent && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => markCurrent.mutate(fy.id)}
                        >
                          Mark As Current
                        </Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>

          <TabsContent value="calendars" className="m-0">
            <table className="w-full text-xs text-left">
              <thead className="text-[11px] font-semibold text-stone uppercase bg-black/5 border-b border-ink-black/10">
                <tr>
                  <th className="px-6 py-3">Month / Year</th>
                  <th className="px-6 py-3">Working Days</th>
                  <th className="px-6 py-3">Payroll Status</th>
                  <th className="px-6 py-3 text-right">State Machine Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-black/5">
                {calendars?.map((cal) => (
                  <tr key={cal.id} className="hover:bg-black/5">
                    <td className="px-6 py-3.5 font-semibold text-ink-black">
                      Month {cal.month}, {cal.year}
                    </td>
                    <td className="px-6 py-3.5 font-mono text-stone">{cal.workingDays} Days</td>
                    <td className="px-6 py-3.5">
                      <Badge 
                        variant={
                          cal.status === "Frozen" 
                            ? "destructive" 
                            : cal.status === "Closed" 
                            ? "default" 
                            : "success"
                        }
                      >
                        {cal.status}
                      </Badge>
                    </td>
                    <td className="px-6 py-3.5 text-right space-x-2">
                      {cal.status === "Open" && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => freezeCalendar.mutate(cal.id)}
                          className="text-amber-700 hover:bg-amber-50"
                        >
                          <Snowflake className="w-3.5 h-3.5 mr-1" />
                          Freeze Calendar
                        </Button>
                      )}
                      {cal.status === "Frozen" && (
                        <>
                          <Button 
                            variant="outline" 
                            size="sm"
                            onClick={() => closeCalendar.mutate(cal.id)}
                            className="text-stone"
                          >
                            <Lock className="w-3.5 h-3.5 mr-1" />
                            Close Period
                          </Button>
                          <Button 
                            variant="outline" 
                            size="sm"
                            onClick={() => reopenCalendar.mutate(cal.id)}
                            className="text-notion-blue"
                          >
                            <RefreshCw className="w-3.5 h-3.5 mr-1" />
                            Reopen
                          </Button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabsContent>
        </Card>
      </Tabs>

      {/* Modal for creating Financial Year */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent>
          <form onSubmit={handleCreateFY}>
            <DialogHeader>
              <DialogTitle>Create Financial Year</DialogTitle>
              <DialogDescription>
                Define a new fiscal cycle anchor for the company.
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-4">
              <div className="space-y-1.5">
                <Label htmlFor="label">Fiscal Label</Label>
                <Input 
                  id="label" 
                  placeholder="e.g. FY 2026-2027" 
                  value={formData.label} 
                  onChange={(e) => setFormData({ ...formData, label: e.target.value })} 
                  required 
                />
              </div>
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
            </div>

            <DialogFooter>
              <Button type="button" variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button>
              <Button type="submit" variant="primary">Create Fiscal Year</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
