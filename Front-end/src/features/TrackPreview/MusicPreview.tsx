import TrackName from "./components/TrackName"
import TrackPlay from "./components/TrackPlay"

const MusicPreview = () => {
	return (
		<div className="w-full h-[72px] ">
			<footer className="h-full flex min-w-[620px] select-none">
				<div className="w-full flex items-center justify-between">
					<TrackName />
					<TrackPlay />
				</div>
			</footer>
		</div>
	)
}

export default MusicPreview
