import { forwardRef } from "react"
import { Link } from "react-router-dom"

interface AlertCreatePlaylistProps {
	setOpen: (open: boolean) => void
}

const AlertCreatePlaylist = forwardRef<HTMLDivElement, AlertCreatePlaylistProps>(
	({ setOpen }, ref) => {
		return (
			<div
				ref={ref}
				className="absolute -right-96 top-20 z-50 w-96 p-4 border-none bg-[#69bfff] rounded-lg animate-in transition-all duration-200"
			>
				<div className="relative">
					<div className="absolute size-3 bg-[#69bfff] rotate-45 top-1/2 -translate-y-1/2 -left-5" />
					<div className="font-bold text-black">Create a playlist</div>
					<div className="font-medium text-black/80">Log in to create and share playlists.</div>

					<div className="flex justify-end items-center gap-3 mt-4">
						<div
							className="text-black font-bold hover:scale-105 cursor-pointer text-sm transition-transform"
							onClick={() => setOpen(false)}
						>
							Not Now
						</div>
						<Link to={"/login"}>
							<div className="flex-shrink-0 py-1 px-3 text-black bg-white hover:scale-105 text-sm transition-transform rounded-full font-bold cursor-pointer">
								Log in
							</div>
						</Link>
					</div>
				</div>
			</div>
		)
	}
)

export default AlertCreatePlaylist
