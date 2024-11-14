import SongName from "./components/SongName"
import SongPlay from "./components/SongPlay"

const MusicPreview = () => {
	return (
		<div className="w-full h-[72px] ">
			<footer className="h-full flex min-w-[620px] select-none">
				<div className="w-full flex items-center justify-between">
					<SongName />

					<SongPlay />

					{/* <SongOptions /> */}
				</div>
			</footer>
		</div>
	)
}

export default MusicPreview
