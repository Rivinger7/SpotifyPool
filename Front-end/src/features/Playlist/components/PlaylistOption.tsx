import { Play } from "lucide-react"

const PlaylistOption = () => {
	return (
		<div className="p-4">
			<button className="cursor-pointer group" onClick={() => {}}>
				<span className="bg-[#1ed760] group-hover:scale-105 group-hover:bg-[#3be477] rounded-full flex items-center justify-center w-14 h-14 text-black">
					<Play className="w-6 fill-current" />
				</span>
			</button>
		</div>
	)
}

export default PlaylistOption
