import { RootState } from "@/store/store"
import { Playlist } from "@/types"
import { Play } from "lucide-react"
import { useSelector } from "react-redux"

interface PlayListsItemProps {
	playlist: Playlist
}

const PlayListsItem = ({ playlist }: PlayListsItemProps) => {
	const { isCollapsed } = useSelector((state: RootState) => state.ui)

	return (
		<div className="w-full rounded-md p-2 flex gap-3 items-center group hover:bg-stone-800/80 cursor-pointer">
			<div className="relative w-12 h-12 shrink-0">
				<img className="w-full h-full rounded-md" src={playlist.images[2]?.url} alt="playlist" />
				{!isCollapsed && (
					<div className="absolute inset-0 items-center justify-center hidden group-hover:flex bg-black/50 rounded-md group-hover:bg-black/70 transition-all duration-300/1000">
						<Play className="size-5 fill-white" />
					</div>
				)}
			</div>

			{!isCollapsed && (
				<div>
					<h1 className="text-white font-bold">{playlist.name}</h1>
					<p className="text-gray-400">Playlist•{playlist.name}</p>
				</div>
			)}
		</div>
	)
}
export default PlayListsItem
