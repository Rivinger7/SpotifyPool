import { useDispatch } from "react-redux"
import { useEffect } from "react"
import { Helmet } from "react-helmet-async"

import Loader from "@/components/ui/Loader"
import AreaHeader from "@/pages/Home/AreaHeader"
import BoxComponent from "@/pages/Home/BoxComponent"
import AudioPlayer from "@/features/Audio/AudioPlayer"

import { useGetTracksQuery } from "@/services/apiTracks"
import { initializeQueue } from "@/store/slice/playerSlice"

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
	const { data: tracksData = [], isLoading } = useGetTracksQuery({}) as {
		data: Track[]
		isLoading: boolean
	}

	useEffect(() => {
		if (tracksData.length > 0) {
			dispatch(initializeQueue(tracksData))
		}
	}, [dispatch, tracksData])

	if (isLoading) return <Loader />

	return (
		<div>
			<Helmet>
				<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Green.png" />
				<title>SpotifyPool</title>
			</Helmet>

			{/* ==== AUDIO ==== */}
			<AudioPlayer />

			<div>
				<section className="pt-6">
					<div className="flex flex-row flex-wrap pl-6 pr-6 gap-x-6 gap-y-8">
						<section className="relative flex flex-col flex-1 max-w-full min-w-full">
							<AreaHeader>Popular tracks</AreaHeader>
							<div className="grid area-body">
								{tracksData?.map((track) => (
									<BoxComponent key={track.id} track={track} />
								))}
							</div>
						</section>
					</div>
				</section>
			</div>
		</div>
	)
}

export default Home
