import * as React from "react"
import { cn } from "@/lib/utils"

export interface SectionHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  title: string
  highlightWord?: string
  subhead?: string
  align?: "left" | "center"
}

export function SectionHeader({ title, highlightWord, subhead, align = "left", className, ...props }: SectionHeaderProps) {
  // If highlightWord is provided, wrap that word in a Notion Hero Highlight Pill
  let renderTitle: React.ReactNode = title

  if (highlightWord && title.includes(highlightWord)) {
    const parts = title.split(highlightWord)
    renderTitle = (
      <>
        {parts[0]}
        <span className="inline-block bg-[#f6d5b8] text-ink-black rounded-[9999px] px-6 py-2 mx-1 shadow-none">
          {highlightWord}
        </span>
        {parts[1]}
      </>
    )
  }

  return (
    <div className={cn("flex flex-col space-y-3", align === "center" && "text-center items-center", className)} {...props}>
      <h2 className="text-[48px] md:text-[54px] font-bold leading-[1.04] tracking-[-1.89px] text-ink-black">
        {renderTitle}
      </h2>
      {subhead && (
        <p className="font-serif text-[18px] leading-[1.56] text-graphite max-w-3xl">
          {subhead}
        </p>
      )}
    </div>
  )
}
