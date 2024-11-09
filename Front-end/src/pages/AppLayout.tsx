import { Outlet } from "react-router-dom"
import { useSelector } from "react-redux"
import { RootState } from "@/store/store"

import Preview from "@/features/common/Preview"
import MainHeader from "@/features/Layout/MainHeader"
import LeftSideBar from "@/features/Layout/LeftSideBar"
import MainContent from "@/features/Layout/MainContent"
import MusicPreview from "@/features/common/MusicPreview"
import { useMemo } from "react"

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
				<div className="bg-[var(--background-base)] rounded-lg w-full max-h-full overflow-y-auto">
					<Outlet />
				</div>
			</MainContent>
			{!isAuthenticated ? <Preview /> : <MusicPreview />}
		</div>
	)
}

export default AppLayout
