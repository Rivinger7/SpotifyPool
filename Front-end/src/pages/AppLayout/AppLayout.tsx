import LeftSideBar from "@/pages/AppLayout/LeftSideBar"
import MainContent from "@/pages/AppLayout/MainContent"
import MainHeader from "@/pages/AppLayout/MainHeader"
import Preview from "@/pages/AppLayout/Preview"
// import { useGetGoogleResponseQuery } from "@/services/apiAuth"
import { Outlet } from "react-router-dom"

function AppLayout() {
	// const { data: responseData, status, error } = useGetGoogleResponseQuery({})

	// console.log(responseData, status, error)

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
