import { RootState } from "@/store/store"
import { useSelector } from "react-redux"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Link } from "react-router-dom"

const PlaylistHeader = () => {
	const { playlistDetail } = useSelector((state: RootState) => state.playlist)
	const { isCollapsed } = useSelector((state: RootState) => state.ui)

	return (
		<div className="flex items-end gap-4 p-6">
			<div className={`shrink-0 ${isCollapsed ? "w-60 h-60" : "w-48 h-48"}`}>
				<img
					src={playlistDetail?.images[1].url}
					alt={playlistDetail?.title + " playlist"}
					className="w-full h-full object-cover rounded-md"
				/>
			</div>

			<div className="flex flex-col gap-3">
				<div>Playlist</div>

				<div className="text-8xl font-bold tracking-tighter">{playlistDetail?.title}</div>

				<div className="flex gap-1 items-center">
					{/* AVATAR IMAGE */}
					<Avatar className="bg-[#1f1f1f] items-center justify-center cursor-pointer hover:scale-110 transition-all w-12 h-12">
						<AvatarImage
							referrerPolicy="no-referrer"
							src={playlistDetail?.avatar.url}
							className="object-cover rounded-full w-8 h-8"
						/>

						<AvatarFallback className="bg-green-500 text-sky-100 font-bold w-8 h-8">
							{playlistDetail?.displayName.charAt(0).toUpperCase()}
						</AvatarFallback>
					</Avatar>

					{/* DISPLAY NAME */}
					<div className="font-bold hover:underline transition-all">
						<Link to={`/user/${playlistDetail?.userId}`}>{playlistDetail?.displayName}</Link>
					</div>
					<span>•</span>
					<div>{playlistDetail?.totalTracks} songs</div>
				</div>
			</div>
		</div>
	)
}

export default PlaylistHeader
