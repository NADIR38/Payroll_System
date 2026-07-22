"use client"

import * as React from "react"
import { Sidebar, NavItem } from "./sidebar"
import { Header } from "./header"

export function DashboardLayout({
  children,
  sidebarItems,
  headerTitle,
  basePath
}: {
  children: React.ReactNode
  sidebarItems: NavItem[]
  headerTitle: string
  basePath: string
}) {
  return (
    <div className="flex min-h-screen bg-paper-warmth antialiased text-ink-black selection:bg-sky-tint selection:text-notion-blue">
      <Sidebar items={sidebarItems} basePath={basePath} />
      <div className="flex-1 flex flex-col min-w-0">
        <Header title={headerTitle} />
        <main className="flex-1 p-10 max-w-[1440px] w-full mx-auto space-y-12">
          {children}
        </main>
      </div>
    </div>
  )
}
