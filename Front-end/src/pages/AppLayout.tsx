import { Outlet } from "react-router-dom"
import { useSelector } from "react-redux"
import { RootState } from "@/store/store"

import Preview from "@/features/common/Preview"
import MainHeader from "@/features/Layout/MainHeader"
import LeftSideBar from "@/features/Layout/LeftSideBar"
import MainContent from "@/features/Layout/MainContent"

function AppLayout() {
	const { isAuthenticated } = useSelector((state: RootState) => state.auth)

	return (
		<div className={"p-2"}>
			<MainHeader />
			<MainContent>
				<LeftSideBar />
				<div className="bg-[var(--background-base)] rounded-lg w-full max-h-full overflow-y-auto">
					<Outlet />
				</div>
			</MainContent>
			{!isAuthenticated && <Preview />}
		</div>
	)
}

export default AppLayout
