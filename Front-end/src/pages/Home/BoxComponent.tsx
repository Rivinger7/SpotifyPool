import { Play } from "lucide-react"
import { Link } from "react-router-dom"

interface BoxComponentProps {
	isAvatar?: boolean
	track?: any
}

const BoxComponent = ({ isAvatar, track }: BoxComponentProps) => {
	return (
		<div className="group inline-flex flex-col gap-x-2 p-3 rounded-sm hover:bg-[#1f1f1f] transition-all animate-in animate-out cursor-pointer">
			<div className="relative">
				<div>
					<img
						className={`w-full h-full object-cover ${isAvatar ? "rounded-full" : "rounded-md"}`} // Chỉ dùng rounded-full cho ảnh avatar còn lại dùng rounded-lg
						src={track.images[1].url}
						alt=""
					/>
				</div>
				<div className="absolute transition-all duration-300 transform translate-y-2 opacity-0 box-play-btn right-2 bottom-2 group-hover:opacity-100 group-hover:translate-y-0">
					<button className="cursor-pointer group/play">
						<span className="bg-[#1ed760] group-hover/play:scale-105 group-hover/play:bg-[#3be477] rounded-full flex items-center justify-center w-12 h-12 text-black">
							<Play className="w-6 fill-current" />
						</span>
					</button>
				</div>
			</div>
			<div>
				<div className="flex flex-col pt-1">
					<Link to={"/"} className="font-medium line-clamp-2">
						{track.name}
					</Link>
					<div className="text-[#b3b3b3] line-clamp-2">
						{track.artists.length > 1
							? track.artists.length > 3
								? `With ${track.artists
										.slice(0, 3)
										.map((artist, index) => artist.name)
										.join(", ")} and more`
								: `With ${track.artists
										.slice(0, -1)
										.map((artist) => artist.name)
										.join(", ")} and ${track.artists[track.artists.length - 1].name}`
							: track.artists[0].name}
					</div>
				</div>
			</div>
		</div>
	)
}

export default BoxComponent
