import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip"

interface CustomedTooltipProps {
	label: string | undefined
	children: React.ReactNode
	align?: "start" | "center" | "end"
	side?: "top" | "bottom" | "left" | "right"
	isHidden?: boolean
}

const CustomTooltip = ({ label, side, align, isHidden, children }: CustomedTooltipProps) => {
	return (
		<TooltipProvider>
			<Tooltip delayDuration={50}>
				<TooltipTrigger asChild>{children}</TooltipTrigger>
				<TooltipContent side={side} align={align} className={`${isHidden ? "hidden" : ""}`}>
					<p className="font-medium text-xs">{label}</p>
				</TooltipContent>
			</Tooltip>
		</TooltipProvider>
	)
}

export default CustomTooltip
