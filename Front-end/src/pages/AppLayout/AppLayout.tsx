import LeftSideBar from "@/pages/AppLayout/LeftSideBar";
import MainContent from "@/pages/AppLayout/MainContent";
import MainHeader from "@/pages/AppLayout/MainHeader";
import Preview from "@/pages/AppLayout/Preview";
import { Outlet } from "react-router-dom";

function AppLayout() {
	return (
		<div className={"grid-templates-container p-2"}>
			<LeftSideBar />
			<MainContent>
				<MainHeader />
				<Outlet />
			</MainContent>
			<Preview />
		</div>
	);
}

export default AppLayout;
