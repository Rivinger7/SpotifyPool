import { Pause, Play } from "lucide-react"
import { RootState } from "@/store/store"
import { useDispatch, useSelector } from "react-redux"
import { playPlaylist, togglePlay } from "@/store/slice/playerSlice"

const PlaylistOption = () => {
	const dispatch = useDispatch()

	const { playlistDetail } = useSelector((state: RootState) => state.playlist)
	const { currentTrack, isPlaying } = useSelector((state: RootState) => state.play)

	// INFO: Play the playlist from the first track or pause/play if the current track is in the playlist
	const handlePlayPlaylist = () => {
		if (!playlistDetail) return

		const iscurrentTrackInPlaylist = playlistDetail?.tracks.some(
			(track) => track.id === currentTrack?.id
		)
		if (iscurrentTrackInPlaylist) {
			dispatch(togglePlay())
			return
		}

		dispatch(playPlaylist({ tracks: playlistDetail.tracks, startIndex: 0 }))
	}

	return (
		<div className="px-6 py-4">
			<button className="cursor-pointer group" onClick={handlePlayPlaylist}>
				<span className="bg-[#1ed760] group-hover:scale-105 group-hover:bg-[#3be477] rounded-full flex items-center justify-center w-14 h-14 text-black">
					{isPlaying && playlistDetail?.tracks.some((track) => track.id === currentTrack?.id) ? (
						<Pause className="size-6 fill-current" />
					) : (
						<Play className="size-6 fill-current" />
					)}
				</span>
			</button>
		</div>
	)
}

export default PlaylistOption
