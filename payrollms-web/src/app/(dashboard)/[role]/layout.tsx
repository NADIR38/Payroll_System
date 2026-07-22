import { notFound } from "next/navigation"
import { DashboardLayout } from "@/components/layout/dashboard-layout"

// Define the role configurations
const ROLE_CONFIGS = {
  superadmin: {
    title: "Super Admin",
    nav: [
      { title: "Overview", href: "/superadmin/dashboard", icon: "LayoutDashboard" },
      { title: "Companies", href: "/superadmin/companies", icon: "Building2" },
      { title: "Global Settings", href: "/superadmin/settings", icon: "Wallet" },
    ]
  },
  companyadmin: {
    title: "Company Admin",
    nav: [
      { title: "Overview", href: "/companyadmin/dashboard", icon: "LayoutDashboard" },
      { title: "Organization", href: "/companyadmin/organization", icon: "Building2" },
      { title: "Users", href: "/companyadmin/users", icon: "Users" },
      { title: "Salary Components", href: "/companyadmin/salary-components", icon: "Wallet" },
      { title: "Salary Structures", href: "/companyadmin/salary-structures", icon: "Briefcase" },
      { title: "Payroll Runs", href: "/companyadmin/payroll-runs", icon: "Receipt" },
      { title: "Approvals", href: "/companyadmin/payroll-approvals", icon: "CheckSquare" },
      { title: "Attendance & Leave", href: "/companyadmin/attendance", icon: "CalendarDays" },
    ]
  },
  hr: {
    title: "HR Manager",
    nav: [
      { title: "Overview", href: "/hr/dashboard", icon: "LayoutDashboard" },
      { title: "Organization", href: "/hr/organization", icon: "Briefcase" },
      { title: "Employees", href: "/hr/employees", icon: "Users" },
      { title: "Salary Components", href: "/hr/salary-components", icon: "Wallet" },
      { title: "Salary Structures", href: "/hr/salary-structures", icon: "Briefcase" },
      { title: "Payroll Runs", href: "/hr/payroll-runs", icon: "Receipt" },
      { title: "Approvals", href: "/hr/payroll-approvals", icon: "CheckSquare" },
      { title: "Attendance & Leave", href: "/hr/attendance", icon: "CalendarDays" },
    ]
  },
  finance: {
    title: "Finance Manager",
    nav: [
      { title: "Overview", href: "/finance/dashboard", icon: "LayoutDashboard" },
      { title: "Financial Ops", href: "/finance/operations", icon: "DollarSign" },
      { title: "Cost Centers", href: "/finance/cost-centers", icon: "Building2" },
    ]
  },
  payroll: {
    title: "Payroll Officer",
    nav: [
      { title: "Overview", href: "/payroll/dashboard", icon: "LayoutDashboard" },
      { title: "Runs", href: "/payroll/payroll-runs", icon: "Receipt" },
      { title: "Approvals", href: "/payroll/payroll-approvals", icon: "CheckSquare" },
      { title: "Attendance & Leave", href: "/payroll/attendance", icon: "CalendarDays" },
      { title: "Calendars", href: "/payroll/calendars", icon: "CalendarDays" },
      { title: "Salary Components", href: "/payroll/salary-components", icon: "Wallet" },
      { title: "Salary Structures", href: "/payroll/salary-structures", icon: "Briefcase" },
    ]
  },
  employee: {
    title: "Employee Portal",
    nav: [
      { title: "Dashboard", href: "/employee/dashboard", icon: "LayoutDashboard" },
      { title: "Payslips", href: "/employee/payslips", icon: "Receipt" },
      { title: "Leaves", href: "/employee/leaves", icon: "CalendarDays" },
    ]
  }
}

export default async function RoleDashboardLayout({
  children,
  params,
}: {
  children: React.ReactNode
  params: Promise<{ role: string }>
}) {
  const { role } = await params;
  
  if (!ROLE_CONFIGS[role as keyof typeof ROLE_CONFIGS]) {
    notFound()
  }

  const config = ROLE_CONFIGS[role as keyof typeof ROLE_CONFIGS]

  return (
    <DashboardLayout 
      sidebarItems={config.nav} 
      headerTitle={config.title}
      basePath={`/${role}`}
    >
      {children}
    </DashboardLayout>
  )
}
