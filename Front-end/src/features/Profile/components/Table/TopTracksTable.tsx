import { useState } from "react"
import { TopTracks } from "@/types"
import { Play } from "lucide-react"
import { useGetTopTracksQuery } from "@/services/apiTracks"
import formatTimeMiliseconds from "@/utils/formatTimeMiliseconds"
import { Table, TableBody, TableCell, TableRow } from "@/components/ui/table"

const TopTracksTable = () => {
	const [hoveredRow, setHoveredRow] = useState<number | null>(null)

	const { data, isLoading } = useGetTopTracksQuery({}) as {
		data: TopTracks
		isLoading: boolean
	}

	if (isLoading) return <p>Loading...</p>

	return (
		<Table>
			{data?.trackInfo?.map((track, index) => (
				<TableBody key={index}>
					<TableRow
						className="group"
						key={track.trackId}
						onMouseEnter={() => setHoveredRow(index)}
						onMouseLeave={() => setHoveredRow(null)}
					>
						<TableCell className="relative w-10">
							<div onClick={() => {}}>
								{/* {hoveredRow === index ? (
									isCurrentTrack && isPlaying ? (
										<Pause className="size-4 fill-white stroke-white absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2" />
									) : (
										<Play className="size-4 fill-white stroke-white absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2" />
									)
								) : isCurrentTrack && isPlaying ? (
									<Music className="size-4 absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 text-green-500" />
								) : (
									index + 1
								)} */}
								{hoveredRow === index ? (
									<Play className="size-4 fill-white stroke-white absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2" />
								) : (
									index + 1
								)}
							</div>
						</TableCell>
						<TableCell className="min-w-[200px]">
							<div className="flex gap-2">
								<div className="shrink-0 w-10 h-10">
									<img
										src={track.track?.images?.[2].url}
										className="w-full h-full object-cover rounded-md"
									/>
								</div>
								<div className="flex flex-col">
									<div>{track.track?.name}</div>
									<div>{track.track?.artists?.[0].name || ""}</div>
								</div>
							</div>
						</TableCell>
						<TableCell className="text-right min-w-[150px]">
							{formatTimeMiliseconds(track.track?.duration)}
						</TableCell>
					</TableRow>
				</TableBody>
			))}
		</Table>
	)
}

export default TopTracksTable
