import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const badgeVariants = cva(
  "inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-notion-blue focus:ring-offset-2",
  {
    variants: {
      variant: {
        default: "bg-paper-warmth text-ink-black",
        blue: "bg-signal-blue text-pure-white",
        success: "bg-sky-tint text-notion-blue",
        warning: "bg-marigold text-ink-black",
        destructive: "bg-coral text-pure-white",
      },
    },
    defaultVariants: {
      variant: "default",
    },
  }
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return (
    <div className={cn(badgeVariants({ variant }), className)} {...props} />
  )
}

export { Badge, badgeVariants }
