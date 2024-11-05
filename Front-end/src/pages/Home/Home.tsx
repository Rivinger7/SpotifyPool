import AreaHeader from "@/pages/Home/AreaHeader"
import BoxComponent from "@/pages/Home/BoxComponent"
import { useGetTracksQuery } from "@/services/apiTracks"
import { Helmet } from "react-helmet-async"

function Home() {
	const { data: tracksData = [] as any[], isLoading } = useGetTracksQuery({})

	return (
		<div>
			<Helmet>
				<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Green.png" />
			</Helmet>

			<div className="main-content">
				<div className="main-content-view">
					<section className="pt-6">
						<div className="flex flex-row flex-wrap pl-6 pr-6 gap-x-6 gap-y-8">
							<section className="relative flex flex-col flex-1 max-w-full min-w-full">
								<AreaHeader>Popular artists</AreaHeader>
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
		</div>
	)
}

export default Home
