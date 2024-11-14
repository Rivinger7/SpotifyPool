import AudioPlayer from "@/features/Audio/AudioPlayer"
import AreaHeader from "@/pages/Home/AreaHeader"
import BoxComponent from "@/pages/Home/BoxComponent"
import { useGetTracksQuery } from "@/services/apiTracks"
import { initializeQueue } from "@/store/slice/playerSlice"
import { RootState } from "@/store/store"
import { useEffect, useMemo } from "react"
import { Helmet } from "react-helmet-async"
import { useDispatch, useSelector } from "react-redux"

interface Image {
	url: string
	height: number
	width: number
}
interface Artist {
	name: string
	followers: number
	genreIds: string[]
	images: Image[]
}
interface Track {
	id: string
	name: string
	description: string | null
	previewURL: string
	duration: number
	images: Image[]
	artists: Artist[]
}

function Home() {
	const dispatch = useDispatch()
	const { data: tracksData = [] } = useGetTracksQuery({}) as { data: Track[] }

	// Memoize limitedTracks to prevent unnecessary recalculations
	const limitedTracks = useMemo(() => tracksData.slice(0, 10), [tracksData])

	const { currentSong } = useSelector((state: RootState) => state.play)

	useEffect(() => {
		if (limitedTracks.length > 0) {
			dispatch(initializeQueue(limitedTracks))
		}
	}, [dispatch, limitedTracks])

	return (
		console.log(currentSong?.previewURL),
		(
			<div>
				<Helmet>
					<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Green.png" />
					<title>Spotify</title>
				</Helmet>

				{/* ==== AUDIO ==== */}
				<AudioPlayer />

				<div className="main-content">
					<div className="main-content-view">
						<section className="pt-6">
							<div className="flex flex-row flex-wrap pl-6 pr-6 gap-x-6 gap-y-8">
								<section className="relative flex flex-col flex-1 max-w-full min-w-full">
									<AreaHeader>Popular artists</AreaHeader>
									<div className="grid area-body">
										{limitedTracks?.map((track) => (
											<BoxComponent key={track.id} track={track} />
										))}
									</div>
								</section>
							</div>
						</section>
					</div>
				</div>
			</div>
		)
	)
}

export default Home
