import { useEffect, useState } from "react"
import { TrackPlaylist } from "@/types"
import { useParams } from "react-router-dom"
import PlaylistTableHeader from "./PlaylistTableHeader"
import { Table, TableBody, TableCell, TableRow } from "@/components/ui/table"

import { Play } from "lucide-react"
import { useGetTracksByPlaylistQuery } from "@/services/apiPlaylist"
import { useDispatch, useSelector } from "react-redux"
import { RootState } from "@/store/store"
import { setPlaylistTracks } from "@/store/slice/playlistSlice"

const PlaylistTable = () => {
	const dispatch = useDispatch()
	const { playlistId } = useParams()
	const [hoveredRow, setHoveredRow] = useState<number | null>(null)

	const { playlistTracks } = useSelector((state: RootState) => state.playlist)

	const { data: tracks, isLoading } = useGetTracksByPlaylistQuery(playlistId) as {
		data: TrackPlaylist[]
		isLoading: boolean
	}

	useEffect(() => {
		if (tracks) {
			dispatch(setPlaylistTracks(tracks))
		}
	}, [dispatch, playlistTracks, tracks])

	if (isLoading) {
		return <div>Loading...</div>
	}

	return (
		<Table>
			<PlaylistTableHeader />

			{playlistTracks.map((track, index) => (
				<TableBody key={track.id}>
					<TableRow
						className="group"
						key={track.id}
						onMouseEnter={() => setHoveredRow(index)}
						onMouseLeave={() => setHoveredRow(null)}
					>
						<TableCell className="relative">
							{hoveredRow === index ? (
								<Play className="size-4 fill-white absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2" />
							) : (
								index + 1
							)}
						</TableCell>
						<TableCell>
							<div className="flex gap-2">
								<div className="shrink-0 w-10 h-10">
									<img
										src={track.images[2].url}
										className="w-full h-full object-cover rounded-md"
									/>
								</div>
								<div className="flex flex-col">
									<div>{track.name}</div>
									<div>{track?.artists?.[0].name || ""}</div>
								</div>
							</div>
						</TableCell>
						<TableCell>{track.addedTime}</TableCell>
						<TableCell className="text-right">{track.durationFormated}</TableCell>
					</TableRow>
				</TableBody>
			))}
		</Table>
	)
}

export default PlaylistTable
