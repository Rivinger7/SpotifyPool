import * as React from "react"

import { cn } from "@/lib/utils"

const Textarea = React.forwardRef<HTMLTextAreaElement, React.ComponentProps<"textarea">>(
	({ className, ...props }, ref) => {
		return (
			<textarea
				className={cn(
					"flex min-h-[80px] w-full rounded-md border border-transparent bg-[#ffffff1a] px-3 py-2 text-base placeholder:text-muted-foreground focus:bg-[#333] focus:border focus:border-[#535353] focus:outline-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm transition-colors duration-200",
					className
				)}
				ref={ref}
				{...props}
			/>
		)
	}
)
Textarea.displayName = "Textarea"

export { Textarea }
