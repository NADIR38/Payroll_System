import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
  "inline-flex items-center justify-center whitespace-nowrap text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-notion-blue focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 active:scale-[0.99]",
  {
    variants: {
      variant: {
        // Notion Primary CTA: #0075de fill, white text, 14px weight 500, rounded 8px, 6px 15px padding
        primary: "bg-notion-blue text-pure-white hover:bg-notion-blue/90 rounded-[8px] shadow-none",
        // Notion Ghost CTA: #e6f3fe fill, #0075de text, 14px weight 500, rounded 8px, 6px 15px padding
        ghostCta: "bg-sky-tint text-notion-blue hover:bg-sky-wash/30 rounded-[8px] shadow-none",
        // Notion Ghost Text: transparent bg, 95% black text, rounded 8px, 6px 15px padding
        ghost: "bg-transparent text-ink-black/95 hover:bg-black/5 rounded-[8px]",
        // Notion Outlined Text: transparent bg, 90% black text, 1px border, rounded 4px, 5px 10px padding
        outline: "bg-transparent border border-ink-black/90 text-ink-black/90 hover:bg-black/5 rounded-[4px] shadow-none",
        destructive: "bg-coral text-pure-white hover:bg-coral/90 rounded-[8px] shadow-none",
      },
      size: {
        default: "py-[6px] px-[15px] text-[14px]",
        outline: "py-[5px] px-[10px] text-[14px]",
        sm: "py-[4px] px-[12px] text-[12px]",
        lg: "py-[10px] px-[20px] text-[16px]",
        icon: "h-8 w-8 rounded-[8px]",
      },
    },
    defaultVariants: {
      variant: "primary",
      size: "default",
    },
  }
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button"
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    )
  }
)
Button.displayName = "Button"

export { Button, buttonVariants }
