import CustomTooltip from "@/components/CustomTooltip"
import { Button } from "@/components/ui/button"
import { Slider } from "@/components/ui/slider"
import { Play, Repeat2, Shuffle, SkipBack, SkipForward } from "lucide-react"

const SongPlay = () => {
	return (
		<div className="flex flex-col items-center justify-center max-w-[722px] w-2/5">
			<div className="w-full mb-2 flex flex-nowrap items-center justify-center gap-x-4">
				<CustomTooltip label="Shuffle">
					<Shuffle className="size-4 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
				</CustomTooltip>

				<CustomTooltip label="Previous">
					<SkipBack className="size-4 text-[#b3b3b3] fill-current hover:text-white cursor-pointer hover:scale-105 transition-all" />
				</CustomTooltip>

				<CustomTooltip label="Play">
					<Button className="group" variant={"play"} size={"iconMd"}>
						<Play className="size-5 text-[#b3b3b3] fill-black stroke-none group-hover:text-white group-hover:scale-105 transition-all" />
					</Button>
					{/* <CirclePlay  /> */}
				</CustomTooltip>

				<CustomTooltip label="Next">
					<SkipForward className="size-4 text-[#b3b3b3] fill-current hover:text-white cursor-pointer hover:scale-105 transition-all" />
				</CustomTooltip>

				<CustomTooltip label="Enable repeat">
					<Repeat2 className="size-4 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
				</CustomTooltip>
			</div>

			{/* ==== SLIDER ====  */}
			<div className="w-full flex justify-between items-center gap-x-2">
				<span>0:00</span>
				<Slider className="w-full h-1 slider-root" />
				<span>1:11</span>
			</div>
		</div>
	)
}

export default SongPlay
