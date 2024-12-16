import { Playlist } from "@/types"
import toast from "react-hot-toast"
import { Link } from "react-router-dom"
import { RootState } from "@/store/store"
import { CircleMinus, Play } from "lucide-react"
import { useDispatch, useSelector } from "react-redux"
import {
	ContextMenu,
	ContextMenuContent,
	ContextMenuItem,
	ContextMenuShortcut,
	ContextMenuTrigger,
} from "@/components/ui/context-menu"
import CustomTooltip from "@/components/CustomTooltip"
import { deletePlaylist } from "@/store/slice/playlistSlice"
import { HttpTransportType, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"

interface PlayListsItemProps {
	playlist: Playlist
}

const PlayListsItem = ({ playlist }: PlayListsItemProps) => {
	const dispatch = useDispatch()

	const { isCollapsed } = useSelector((state: RootState) => state.ui)
	const { userToken } = useSelector((state: RootState) => state.auth)

	const handleDeletePlaylist = () => {
		const connection = new HubConnectionBuilder()
			.withUrl(import.meta.env.VITE_SPOTIFYPOOL_HUB_ADD_TO_PLAYLIST_URL, {
				// skipNegotiation: true,
				transport: HttpTransportType.WebSockets, // INFO: set transport ở đây thànhh websockets để sử dụng skipNegotiation
				// transport: HttpTransportType.LongPolling,
				accessTokenFactory: () => `${userToken?.accessToken}`,
				// headers: {
				// 	Authorization: `ROCEEaMgL6TEDqZlwxvm3ELwCBTc8MVC`,
				// },
			})
			.withAutomaticReconnect()
			.configureLogging(LogLevel.None) // INFO: set log level ở đây để tắt log -- khôngg cho phép log ra client
			.build()

		connection
			.start()
			.then(() => {
				console.log("Connected to the hub")
				connection.invoke("DeletePlaylistAsync", playlist.id)
				// connection.invoke("AddToPlaylistAsync", currentSong?.id, null, "Favorites Songs")
			})
			.catch((err) => console.error(err))

		// NOTE: Tạo mới playlist Favorites songs khi playlist này chưa tồn tại
		connection.on("DeletePlaylistSuccessfully", (playlistId) => {
			dispatch(deletePlaylist(playlistId))
			toast.success("Removed from Your Library", {
				position: "bottom-center",
			})
		})

		// NOTE: Khi sự kiện này diễn ra signalR sẽ dừng hoạt động và trả về lỗi
		connection.on("ReceiveException", (message) => {
			toast.error(message, {
				position: "top-right",
				duration: 2000,
			})

			console.log(message)
		})

		connection.onclose(() => {
			console.log("Connection closed")
		})
	}

	return (
		<ContextMenu>
			<ContextMenuTrigger>
				<CustomTooltip label={playlist.name} side="right" isHidden={!isCollapsed}>
					<Link to={`/playlist/${playlist.id}`}>
						<div className="w-full rounded-md p-2 flex gap-3 items-center group hover:bg-stone-800/80 cursor-pointer">
							<div className={`relative ${isCollapsed ? "w-full" : "w-12 h-12"} shrink-0`}>
								<img
									className="w-full h-full rounded-md"
									src={playlist.images[2]?.url}
									alt="playlist"
								/>
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
					</Link>
				</CustomTooltip>
			</ContextMenuTrigger>

			<ContextMenuContent className="w-40 border-none">
				<ContextMenuItem className="text-lg" inset onSelect={handleDeletePlaylist}>
					Delete
					<ContextMenuShortcut>
						<CircleMinus />
					</ContextMenuShortcut>
				</ContextMenuItem>
			</ContextMenuContent>
		</ContextMenu>
	)
}
export default PlayListsItem
