"use client"

import { SectionHeader } from "@/components/ui/section-header"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Building2, Users, Snowflake, ArrowUpRight, CheckCircle2 } from "lucide-react"

export default function RoleOverviewDashboard() {
  return (
    <div className="space-y-12">
      <div className="flex justify-between items-end border-b border-black/8 pb-8">
        <SectionHeader 
          title="Workspace Overview" 
          highlightWord="Overview"
          subhead="Welcome back. Here is the high-level operational summary across your multi-tenant payroll workspace."
        />
        <div className="flex items-center space-x-3">
          <Badge variant="success">System Online</Badge>
          <Button variant="primary">Generate Payroll</Button>
        </div>
      </div>

      {/* Notion Accent & Dark Feature Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card variant="accent" accentColor="#ffb110">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-semibold uppercase text-ink-black/70 tracking-wider">Active Employees</CardTitle>
            <Users className="w-4 h-4 text-ink-black/60" />
          </CardHeader>
          <CardContent>
            <p className="text-[40px] font-bold leading-none tracking-tight text-ink-black">1,248</p>
            <p className="text-[14px] text-ink-black/70 mt-3 font-medium flex items-center">
              <ArrowUpRight className="w-4 h-4 mr-0.5 text-emerald-800" /> +12 new this month
            </p>
          </CardContent>
        </Card>

        <Card variant="accent" accentColor="#62aef0">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-semibold uppercase text-ink-black/70 tracking-wider">Tenant Companies</CardTitle>
            <Building2 className="w-4 h-4 text-ink-black/60" />
          </CardHeader>
          <CardContent>
            <p className="text-[40px] font-bold leading-none tracking-tight text-ink-black">3 Active</p>
            <p className="text-[14px] text-ink-black/70 mt-3 font-medium">100% Data Isolation (Postgres RLS)</p>
          </CardContent>
        </Card>

        <Card variant="dark">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-semibold uppercase text-pure-white/70 tracking-wider">Payroll Status</CardTitle>
            <Snowflake className="w-4 h-4 text-sky-wash" />
          </CardHeader>
          <CardContent>
            <p className="text-[40px] font-bold leading-none tracking-tight text-pure-white">Frozen</p>
            <p className="text-[14px] text-pure-white/70 mt-3 font-medium">March 2026 Operational Window</p>
          </CardContent>
        </Card>
      </div>

      {/* White Feature Cards */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <Card variant="default">
          <CardHeader>
            <CardTitle>Recent Event Stream</CardTitle>
            <CardDescription>Immutable domain audit logs from the PostgreSQL cluster.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4 text-sm text-graphite">
              <div className="flex items-start space-x-3 p-3 rounded-[8px] bg-black/5">
                <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
                <div>
                  <p className="font-semibold text-ink-black">Company Soft-Deactivated</p>
                  <p className="text-[13px] text-stone">TechNova Solutions deactivated by SuperAdmin under soft-delete policy.</p>
                </div>
              </div>
              <div className="flex items-start space-x-3 p-3 rounded-[8px] bg-black/5">
                <Snowflake className="w-4 h-4 text-amber-600 shrink-0 mt-0.5" />
                <div>
                  <p className="font-semibold text-ink-black">Payroll Freeze Initiated</p>
                  <p className="text-[13px] text-stone">March 2026 payroll freeze date executed by Rule 6.4.2 constraint.</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card variant="default">
          <CardHeader>
            <CardTitle>Business Rules Matrix</CardTitle>
            <CardDescription>Status of Module 01 domain invariants & architectural rules.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3 text-xs">
              <div className="flex justify-between items-center py-2 border-b border-black/8">
                <span className="font-medium text-ink-black">PRD Rule 6.4.2 (Payroll Freeze Lock)</span>
                <Badge variant="success">Enforced</Badge>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-black/8">
                <span className="font-medium text-ink-black">PRD Rule 6.4.4 (Single Active FY)</span>
                <Badge variant="success">Enforced</Badge>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-black/8">
                <span className="font-medium text-ink-black">Multi-Tenant Isolation (Row Level Security)</span>
                <Badge variant="success">Protected</Badge>
              </div>
              <div className="flex justify-between items-center py-2">
                <span className="font-medium text-ink-black">RFC 7807 Global Problem Details Mapper</span>
                <Badge variant="blue">Active</Badge>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
