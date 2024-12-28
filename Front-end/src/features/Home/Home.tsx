import { useCallback, useEffect, useState } from "react"
import { Helmet } from "react-helmet-async"

import Loader from "@/components/ui/Loader"
import TracksHeader from "@/features/Home/TracksHeader.tsx"
import TracksComponent from "@/features/Home/TracksComponent.tsx"

import { Track } from "@/types"
import { useGetTracksQuery } from "@/services/apiTracks"
import AlertTrackModal from "./components/Modal/AlertTrackModal"
import { Loader2 } from "lucide-react"

function Home() {
	const [open, setOpen] = useState(false)
	const [offset, setOffset] = useState(1)
	const [loadingData, setLoadingData] = useState(false)

	// NOTE: Hiện tại chỉ lấy 20 bài hát đầu tiên -- chỉ cần scroll xuống dưới sẽ tự động để load thêm bài hát
	const { data = [], isLoading } = useGetTracksQuery({ offset, limit: 20 }) as {
		data: Track[]
		isLoading: boolean
	}

	const [tracksData, setTracksData] = useState<Track[]>([])

	useEffect(() => {
		if (data.length > 0) {
			setTracksData((prev) => [...prev, ...data])
		}
		setLoadingData(false)
	}, [data])

	const handleScroll = useCallback(() => {
		const { scrollTop, clientHeight, scrollHeight } = document.getElementById(
			"main-content"
		) as HTMLElement

		if (scrollTop + clientHeight + 1 >= scrollHeight) {
			setLoadingData(true)
			setOffset((prev) => prev + 1)
		}
	}, [])

	useEffect(() => {
		const test = document.getElementById("main-content") as HTMLElement
		test.addEventListener("scroll", handleScroll)
		return () => test.removeEventListener("scroll", handleScroll)
	}, [handleScroll])

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
									<TracksComponent
										key={track.id}
										track={track}
										tracks={tracksData}
										setOpen={setOpen}
									/>
								))}
							</div>
							{loadingData && (
								<div className="w-full flex justify-center mt-4">
									<Loader2 className="size-14 animate-spin" />
								</div>
							)}
						</section>
					</div>
				</section>

				<AlertTrackModal open={open} setOpen={setOpen} />
			</div>
		</div>
	)
}

export default Home
