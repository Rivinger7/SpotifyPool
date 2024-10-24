import LeftSideBar from "@/pages/AppLayout/LeftSideBar"
import MainContent from "@/pages/AppLayout/MainContent"
import MainHeader from "@/pages/AppLayout/MainHeader"
import Preview from "@/pages/AppLayout/Preview"
import { RootState } from "@/store/store"
import { useSelector } from "react-redux"
// import { useGetGoogleResponseQuery } from "@/services/apiAuth"
import { Outlet } from "react-router-dom"

function AppLayout() {
	const { isAuthenticated } = useSelector((state: RootState) => state.auth)

	return (
		<div className={"grid-templates-container p-2"}>
			<LeftSideBar />
			<MainContent>
				<MainHeader />
				<Outlet />
			</MainContent>
			{!isAuthenticated && <Preview />}
		</div>
	)
}

export default AppLayout
