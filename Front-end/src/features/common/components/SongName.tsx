import CustomTooltip from "@/components/CustomTooltip"
import { Button } from "@/components/ui/button"
import { ChevronUp, CirclePlus } from "lucide-react"
import { Link } from "react-router-dom"

const SongName = () => {
	return (
		<div className="ps-2 min-w-[180px] w-[30%]">
			<div className="flex items-center justify-start relative">
				{/* ==== IMAGE ==== */}
				<div className="relative h-[56px] w-[56px] group me-2">
					<div className="w-full h-full">
						<img
							className="rounded-lg w-full h-full object-cover flex shrink-0"
							src="https://i.scdn.co/image/ab67616d000048513719eed165d16597bd930595"
							alt="random song on spotify"
						/>
					</div>

					<div className="absolute top-[5px] right-[5px] opacity-0 group-hover:opacity-100 transition-opacity">
						<CustomTooltip label="Expand" side="top">
							<Button
								className="cursor-default text-[#b3b3b3] hover:text-white hover:bg-[rgba(0,0,0,0.8)]"
								variant={"normal"}
								size={"iconSm"}
							>
								<ChevronUp className="size-4" />
							</Button>
						</CustomTooltip>
					</div>
				</div>

				{/* ==== NAME -- ARTIST ==== */}
				<div className="mx-2 flex flex-col justify-center">
					<div className="text-sm font-bold text-white">
						<Link to={"/"} className="hover:underline hover:text-white">
							Song Name
						</Link>
					</div>
					<div className="text-xs text-[#b3b3b3]">
						<Link to={"/"} className="hover:underline hover:text-white">
							Artist Name
						</Link>
					</div>
				</div>

				{/* ==== ADD TO LIKED SONGS ==== */}
				<div>
					<CustomTooltip label="Add to Liked Songs" side="top">
						<Button variant={"transparent"} className="p-2 group">
							<CirclePlus className="size-4 text-[#b3b3b3] group-hover:text-white" />
						</Button>
					</CustomTooltip>
				</div>
			</div>
		</div>
	)
}

export default SongName
