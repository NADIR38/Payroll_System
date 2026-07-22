import * as React from "react"
import { cn } from "@/lib/utils"

const Card = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement> & { variant?: 'default' | 'accent' | 'dark', accentColor?: string }>(
  ({ className, variant = 'default', accentColor, ...props }, ref) => {
    let baseStyles = "rounded-[12px] border border-black/8 bg-pure-white text-ink-black p-6 shadow-none";

    if (variant === 'accent') {
      baseStyles = "rounded-[12px] border-none text-ink-black p-6 shadow-none";
    } else if (variant === 'dark') {
      baseStyles = "rounded-[12px] border-none bg-midnight-ink text-pure-white p-6 shadow-none";
    }

    return (
      <div
        ref={ref}
        className={cn(baseStyles, className)}
        style={variant === 'accent' && accentColor ? { backgroundColor: accentColor } : undefined}
        {...props}
      />
    )
  }
)
Card.displayName = "Card"

const CardHeader = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("flex flex-col space-y-1.5 pb-4", className)}
      {...props}
    />
  )
)
CardHeader.displayName = "CardHeader"

const CardTitle = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("text-[22px] font-bold leading-[1.27] tracking-[-0.242px] text-ink-black", className)}
      {...props}
    />
  )
)
CardTitle.displayName = "CardTitle"

const CardDescription = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("text-[16px] text-graphite leading-[1.5]", className)}
      {...props}
    />
  )
)
CardDescription.displayName = "CardDescription"

const CardContent = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn("pt-2", className)} {...props} />
  )
)
CardContent.displayName = "CardContent"

const CardFooter = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("flex items-center pt-4 border-t border-black/8 mt-4", className)}
      {...props}
    />
  )
)
CardFooter.displayName = "CardFooter"

export { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter }
