import { useSelector } from "react-redux"
import { RootState } from "@/store/store.ts"
import PlayListsItem from "@/features/Playlist/PlayListsItem.tsx"

const PlayListsSidebar = () => {
	const { playlists } = useSelector((state: RootState) => state.playlist)

	return (
		<>
			{playlists?.map((playlist) => (
				<PlayListsItem key={playlist.id} playlist={playlist} />
			))}
		</>
	)
}
export default PlayListsSidebar
