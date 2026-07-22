"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import { 
  Building2, Users, LayoutDashboard, Wallet, CalendarDays, Receipt, Briefcase, DollarSign,
  type LucideIcon 
} from "lucide-react"

const iconMap: Record<string, LucideIcon> = {
  Building2,
  Users,
  LayoutDashboard,
  Wallet,
  CalendarDays,
  Receipt,
  Briefcase,
  DollarSign
}

export interface NavItem {
  title: string
  href: string
  icon: string
}

export function Sidebar({ items, basePath }: { items: NavItem[], basePath: string }) {
  const pathname = usePathname()

  return (
    <aside className="w-64 bg-paper-warmth border-r border-black/8 flex flex-col shrink-0 min-h-screen py-6 px-4">
      {/* Brand Header */}
      <div className="flex items-center space-x-3 px-4 mb-8">
        <div className="w-7 h-7 rounded-[8px] bg-notion-blue flex items-center justify-center text-pure-white font-bold text-xs">
          P
        </div>
        <div className="flex flex-col">
          <span className="font-semibold text-sm text-ink-black tracking-tight">PayrollMS</span>
          <span className="text-[11px] text-stone capitalize">{basePath.replace('/', '')} Workspace</span>
        </div>
      </div>
      
      {/* Nav List - Notion Muted Nav Link design */}
      <nav className="flex-1 space-y-1">
        <div className="px-4 pb-2 text-[11px] font-semibold text-stone uppercase tracking-wider">
          Navigation
        </div>
        {items.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(item.href + '/')
          const IconComponent = iconMap[item.icon] || LayoutDashboard

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center space-x-3 px-4 py-3 rounded-[8px] text-[14px] font-medium transition-colors",
                isActive 
                  ? "bg-black/5 text-ink-black font-semibold" 
                  : "text-ink-black/54 hover:bg-black/5 hover:text-ink-black"
              )}
            >
              <IconComponent className={cn("w-4 h-4", isActive ? "text-notion-blue" : "text-ink-black/54")} />
              <span>{item.title}</span>
            </Link>
          )
        })}
      </nav>

      {/* User Footer */}
      <div className="pt-4 border-t border-black/8 px-4">
        <div className="flex items-center space-x-3">
          <div className="w-8 h-8 rounded-full bg-sky-tint text-notion-blue flex items-center justify-center text-xs font-bold">
            AD
          </div>
          <div className="flex flex-col truncate">
            <span className="text-xs font-medium text-ink-black truncate">Admin Session</span>
            <span className="text-[11px] text-stone truncate">Tenant: Acme Corp</span>
          </div>
        </div>
      </div>
    </aside>
  )
}
