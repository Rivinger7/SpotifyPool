import CustomTooltip from "@/components/CustomTooltip"
import { Maximize2, SquarePlay } from "lucide-react"

const SongOptions = () => {
	return (
		<div className="flex gap-x-2 items-center justify-end mr-2 min-w-[180px] w-[30%]">
			<CustomTooltip label="Now playing view">
				<SquarePlay className="size-5 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
			</CustomTooltip>

			<CustomTooltip label="Fullscreen">
				<Maximize2 className="size-5 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
			</CustomTooltip>
		</div>
	)
}

export default SongOptions
