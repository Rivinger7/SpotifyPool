import CustomTooltip from "@/components/CustomTooltip"
import { togglePlayingView } from "@/store/slice/uiSlice"
import { RootState } from "@/store/store"
import { PlusCircle, X } from "lucide-react"
import { useDispatch, useSelector } from "react-redux"

// NOTE: might be used for later
/* // Selector to get the next track
const getNextTrack = (state: RootState) => {
	const { queue, currentIndex } = state.play
	return currentIndex < queue.length - 1 ? queue[currentIndex + 1] : queue[0]
} */

const NowPlayingView = () => {
	const dispatch = useDispatch()
	const { isPlayingView } = useSelector((state: RootState) => state.ui)
	const { currentTrack, nextTrack } = useSelector((state: RootState) => state.play)

	return (
		<div
			className={`${
				isPlayingView ? "w-[400px]" : "w-0"
			} shrink-0 max-h-full relative overflow-y-auto`}
		>
			<div className="px-3 py-4 space-y-2 w-full h-full bg-[var(--background-base)]">
				{/* ==== Playing View Header ==== */}
				<div className="flex items-center justify-between">
					<h3 className="font-semibold text-base hover:underline cursor-pointer underline-offset-1">
						Favorite Songs
					</h3>

					<CustomTooltip label="Close" side="top">
						<button
							onClick={() => dispatch(togglePlayingView())}
							className="size-8 p-2 group hover:bg-white/10 rounded-full transition-colors"
						>
							<X className="size-4 text-[#b3b3b3] group-hover:text-white transition-colors" />
						</button>
					</CustomTooltip>
				</div>

				{/* ==== Track thumbnail ==== */}
				<div className="w-full rounded-lg bg-[#1f1f1f] p-4">
					<img
						src={currentTrack?.images[0].url}
						alt="random test song"
						className="w-full h-auto rounded-lg object-cover object-center"
					/>

					<div className="flex items-center justify-between mt-3 gap-3">
						<div>
							<h1 className="text-2xl font-bold tracking-tight uppercase line-clamp-1 text-white hover:underline cursor-pointer underline-offset-1">
								{currentTrack?.name}
							</h1>
							<p className="text-base text-[#b3b3b3] line-clamp-1">Artist name</p>
						</div>

						<CustomTooltip label="Add to Favorite Songs" side="top" align="end">
							<button>
								<PlusCircle className="size-6 text-white hover:scale-105 transition-colors" />
							</button>
						</CustomTooltip>
					</div>
				</div>

				{/* ==== Artist ==== */}
				<div className="w-full rounded-lg bg-[#1f1f1f] p-4">
					<h3 className="font-semibold text-base">About the artist</h3>
					<div className="my-5">
						<img
							src={`${currentTrack?.artists?.[0]?.images?.[0]?.url}` || "https://placehold.co/80"}
							alt="test avatar"
							className="rounded-full object-cover object-center shrink-0 size-20"
						/>
					</div>
					<h3 className="font-semibold text-base text-white hover:underline cursor-pointer underline-offset-1">
						{currentTrack?.artists?.[0]?.name || "Artist name"}
					</h3>
				</div>

				{/* ==== Next track in queue ==== */}
				<div className="w-full rounded-lg bg-[#1f1f1f] p-4">
					<div className="flex items-center justify-between">
						<h3 className="font-semibold text-base">Next in queue</h3>
						<button className="font-medium text-[#ffffffb2] hover:scale-105 hover:text-white hover:underline decoration-white">
							Open queue
						</button>
					</div>
					<div className="mt-3 rounded-lg hover:bg-white/10 p-2 transition-colors cursor-pointer">
						<div className="flex items-center gap-3">
							<img
								src={`${nextTrack?.images?.[0]?.url || "https://placehold.co/48"}`}
								alt="test image"
								className="rounded-sm shrink-0 size-12 object-cover object-center"
							/>
							<div>
								<h4 className="font-semibold text-base line-clamp-1">{nextTrack?.name}</h4>
								<p className="text-[#b3b3b3] text-sm font-medium line-clamp-1">Artist name</p>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	)
}

export default NowPlayingView
