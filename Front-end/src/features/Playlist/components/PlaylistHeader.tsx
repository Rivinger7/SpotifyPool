import { RootState } from "@/store/store"
import { useSelector } from "react-redux"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
// import { useParams } from "react-router-dom"

const PlaylistHeader = () => {
	// const { playlistId } = useParams()
	const { userData } = useSelector((state: RootState) => state.auth)

	return (
		<div className="flex items-end gap-4 p-4">
			<div className="shrink-0"></div>
			<div className="flex flex-col gap-3">
				<div>Playlist</div>

				<div className="text-3xl font-bold">Favorite Songs</div>

				<div className="flex gap-1 items-center">
					{/* AVATAR IMAGE */}
					<Avatar className="bg-[#1f1f1f] items-center justify-center cursor-pointer hover:scale-110 transition-all w-12 h-12">
						<AvatarImage
							referrerPolicy="no-referrer"
							src={userData?.avatar}
							className="object-cover rounded-full w-8 h-8"
						/>

						<AvatarFallback className="bg-green-500 text-sky-100 font-bold w-8 h-8">
							{userData?.displayName.charAt(0).toUpperCase()}
						</AvatarFallback>
					</Avatar>

					{/* DISPLAY NAME */}
					<div className="font-bold">{userData?.displayName}</div>
					<span>•</span>
					<div>2 songs</div>
				</div>
			</div>
		</div>
	)
}

export default PlaylistHeader
