import React from "react";
import { Slider } from "@/components/ui/slider"
import CustomTooltip from "@/components/CustomTooltip"

import { Maximize2, SquarePlay, Volume1 } from "lucide-react"

interface SongOptionsProps {
	audioRef: React.MutableRefObject<HTMLAudioElement | null>
	volume: number
	setVolume: (value: number) => void
}

const SongOptions = ({ audioRef, volume, setVolume }: SongOptionsProps) => {
	return (
		<div className="flex gap-x-2 items-center justify-end mr-2 min-w-[180px] w-[30%]">
			<div className="flex items-center gap-2">
				<CustomTooltip label="Volume" side="top">
					<Volume1 className="size-4 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
				</CustomTooltip>

				<Slider
					value={[volume]}
					max={100}
					step={1}
					className="w-24 hover:cursor-grab active:cursor-grabbing"
					onValueChange={(value) => {
						setVolume(value[0])
						if (audioRef.current) {
							audioRef.current.volume = value[0] / 100
						}
					}}
				/>
			</div>

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
