import LeftSideBar from "@/components/LeftSideBar.tsx";
import MainContent from "@/components/MainContent.tsx";
import Preview from "@/components/Preview.tsx";
import { Helmet } from "react-helmet";

function Home() {
	return (
		<div className={"grid-templates-container p-2"}>
			<Helmet>
				<link
					rel="icon"
					type="image/svg+xml"
					href="/Spotify_Icon_RGB_Green.png"
				/>
			</Helmet>
			<LeftSideBar />
			<MainContent />
			<Preview />
		</div>
	);
}

export default Home;
