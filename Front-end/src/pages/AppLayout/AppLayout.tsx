import LeftSideBar from "@/pages/AppLayout/LeftSideBar"
import MainContent from "@/pages/AppLayout/MainContent"
import MainHeader from "@/pages/AppLayout/MainHeader"
import Preview from "@/pages/AppLayout/Preview"
import { RootState } from "@/store/store"
import { useEffect } from "react"
import { useSelector } from "react-redux"
import { Outlet, useLocation, useNavigate } from "react-router-dom"

function AppLayout() {
	const nav = useNavigate()
	const location = useLocation()

	const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated)

	useEffect(() => {
		const excludedRoutes = ["/spotifypool/confirm-email"]

		if (!isAuthenticated && !excludedRoutes.includes(location.pathname)) {
			nav("/login")
		}
	}, [nav, isAuthenticated, location.pathname])

	return (
		<div className={"grid-templates-container p-2"}>
			<LeftSideBar />
			<MainContent>
				<MainHeader />
				<Outlet />
			</MainContent>
			<Preview />
		</div>
	)
}

export default AppLayout
