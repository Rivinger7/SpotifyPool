import MainHeader from "./MainHeader";
import AreaHeader from "./AreaHeader";
import BoxComponent from "./BoxComponent";

const MainContent = () => {
	return (
		<div
			className={
				"main-content-container relative top-0 left-0 bg-[var(--background-base)] rounded-lg"
			}
		>
			<MainHeader />
			<div className="main-content">
				<div className="main-content-view">
					<section className="pt-6">
						<div className="flex flex-row flex-wrap gap-x-6 gap-y-8 pl-6 pr-6">
							<section className="flex flex-col relative flex-1 min-w-full max-w-full">
								<AreaHeader>Popular artists</AreaHeader>
								<div className="area-body grid">
									<BoxComponent isAvatar={true} />
									<BoxComponent isAvatar={true} />
									<BoxComponent isAvatar={true} />
									<BoxComponent isAvatar={true} />
									<BoxComponent isAvatar={true} />
									<BoxComponent isAvatar={true} />
								</div>
							</section>
						</div>
					</section>
				</div>
			</div>
		</div>
	);
};

export default MainContent;
