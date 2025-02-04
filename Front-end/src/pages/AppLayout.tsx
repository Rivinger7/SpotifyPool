import { Outlet } from "react-router-dom"
import { useSelector } from "react-redux"
import { RootState } from "@/store/store"

import { useMemo } from "react"

import Preview from "@/features/TrackPreview/Preview"
import MainHeader from "@/features/Layout/MainHeader"
import AudioPlayer from "@/features/Audio/AudioPlayer"
import LeftSideBar from "@/features/Layout/LeftSideBar"
import MainContent from "@/features/Layout/MainContent"
import MusicPreview from "@/features/TrackPreview/MusicPreview"
import NowPlayingView from "@/features/Layout/NowPlayingView"

function AppLayout() {
	const { isAuthenticated } = useSelector((state: RootState) => state.auth)

	const mainHeight = useMemo(() => {
		return isAuthenticated ? "h-[calc(100vh_-_72px_-_80px)]" : "h-[calc(100vh_-_72px_-_76px)]"
	}, [isAuthenticated])

	return (
		<div className={"p-2"}>
			<MainHeader />
			<MainContent mainHeight={mainHeight}>
				<LeftSideBar />
				<div
					id="main-content"
					className="bg-[var(--background-base)] rounded-lg w-full max-h-full overflow-y-auto"
				>
					{/* ==== AUDIO ==== */}
					<AudioPlayer />

					<Outlet />
				</div>
				<NowPlayingView />
			</MainContent>
			{!isAuthenticated ? <Preview /> : <MusicPreview />}
		</div>
	)
}

export default AppLayout
