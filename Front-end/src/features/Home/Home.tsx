// import { useEffect } from "react"
// import { useDispatch } from "react-redux"
import { Helmet } from "react-helmet-async"

import Loader from "@/components/ui/Loader"
import TracksHeader from "@/features/Home/TracksHeader.tsx"
import TracksComponent from "@/features/Home/TracksComponent.tsx"

import { Track } from "@/types"
import { useGetTracksQuery } from "@/services/apiTracks"
// import { initializeQueue } from "@/store/slice/playerSlice"

function Home() {
	// const dispatch = useDispatch()

	// NOTE: Hiện tại chỉ lấy 6 bài hát đầu tiên
	const { data: tracksData = [], isLoading } = useGetTracksQuery({ limit: 30 }) as {
		data: Track[]
		isLoading: boolean
	}

	// useEffect(() => {
	// 	if (tracksData.length > 0) {
	// 		dispatch(initializeQueue(tracksData))
	// 	}
	// }, [dispatch, tracksData])

	if (isLoading) return <Loader />

	return (
		<div>
			<Helmet>
				<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Green.png" />
				<title>SpotifyPool</title>
			</Helmet>

			<div>
				<section className="pt-6">
					<div className="flex flex-row flex-wrap pl-6 pr-6 gap-x-6 gap-y-8">
						<section className="relative flex flex-col flex-1 max-w-full min-w-full">
							<TracksHeader>Popular tracks</TracksHeader>
							<div className="grid grid-cols-5">
								{tracksData?.map((track) => (
									<TracksComponent key={track.id} track={track} tracks={tracksData} />
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
