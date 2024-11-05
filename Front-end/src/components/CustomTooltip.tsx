import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip"

interface CustomedTooltipProps {
	label: string | undefined
	children: React.ReactNode
	align?: "start" | "center" | "end"
	side?: "top" | "bottom" | "left" | "right"
}

const CustomTooltip = ({ label, side, align, children }: CustomedTooltipProps) => {
	return (
		<TooltipProvider>
			<Tooltip delayDuration={50}>
				<TooltipTrigger asChild>{children}</TooltipTrigger>
				<TooltipContent side={side} align={align}>
					<p className="font-medium text-xs">{label}</p>
				</TooltipContent>
			</Tooltip>
		</TooltipProvider>
	)
}

export default CustomTooltip
