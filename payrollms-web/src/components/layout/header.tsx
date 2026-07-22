import { Button } from "@/components/ui/button"
import { Bell, Search } from "lucide-react"

export function Header({ title }: { title: string }) {
  return (
    <header 
      className="h-[64px] bg-paper-warmth flex items-center justify-between px-8 sticky top-0 z-30 border-b border-black/8"
      style={{
        boxShadow: "0px 0.7px 1.462px 0px rgba(0, 0, 0, 0.015), 0px 3px 9px 0px rgba(0, 0, 0, 0.03)"
      }}
    >
      <div className="flex items-center">
        <h1 className="text-[18px] font-semibold text-ink-black">{title}</h1>
      </div>
      
      <div className="flex items-center space-x-4">
        <div className="relative hidden md:block">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-stone" />
          <input 
            type="text" 
            placeholder="Search workspace..." 
            className="h-8 w-64 bg-black/5 rounded-[8px] pl-9 pr-4 text-xs focus:outline-none focus:ring-1 focus:ring-notion-blue text-ink-black placeholder:text-stone transition-colors"
          />
        </div>

        <Button variant="ghost" size="icon" className="text-ink-black/60 hover:text-ink-black">
          <Bell className="w-4 h-4" />
        </Button>

        <div className="flex items-center space-x-2 text-xs font-medium text-stone bg-sky-tint text-notion-blue px-3 py-1 rounded-[9999px]">
          <span className="w-2 h-2 rounded-full bg-notion-blue animate-pulse" />
          <span>Acme Corp Tenant</span>
        </div>
      </div>
    </header>
  )
}
