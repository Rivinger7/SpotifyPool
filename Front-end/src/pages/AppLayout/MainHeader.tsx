import { Link } from "react-router-dom";

const MainHeader = () => {
	return (
		<>
			<header className="main-content-header sticky w-full flex items-center bg-[rgba(0,0,0,.5)] h-[64px]">
				<div className="main-content-header-action w-full flex items-center justify-between pl-6 mr-6">
					<div className="prev-next-btn flex gap-2 text-white">
						<button className="w-8 h-8 inline-flex items-center justify-center cursor-not-allowed opacity-60 bg-black bg-opacity-70 rounded-full">
							<svg
								role="img"
								aria-hidden="true"
								viewBox="0 0 16 16"
								className="w-4"
								fill={"currentColor"}
							>
								<path d="M11.03.47a.75.75 0 0 1 0 1.06L4.56 8l6.47 6.47a.75.75 0 1 1-1.06 1.06L2.44 8 9.97.47a.75.75 0 0 1 1.06 0z"></path>
							</svg>
						</button>
						<button className="w-8 h-8 inline-flex items-center justify-center cursor-not-allowed opacity-60 bg-black bg-opacity-70 rounded-full">
							<svg
								data-encore-id="icon"
								role="img"
								aria-hidden="true"
								viewBox="0 0 16 16"
								className="w-4"
								fill={"currentColor"}
							>
								<path d="M4.97.47a.75.75 0 0 0 0 1.06L11.44 8l-6.47 6.47a.75.75 0 1 0 1.06 1.06L13.56 8 6.03.47a.75.75 0 0 0-1.06 0z"></path>
							</svg>
						</button>
					</div>
					<div className="auth-btn mr-2">
						<div>
							<Link to={"/signup"}>
								<button className="inline-flex items-center justify-center text-[#b3b3b3] p-2 pr-8 font-bold hover:text-white hover:scale-x-105">
									Sign up
								</button>
							</Link>
							<Link to={"/login"}>
								<button className="signin">
									<span className="bg-white text-black flex items-center justify-center transition-all p-2 pl-8 pr-8 rounded-full font-bold min-h-12 ">
										Log in
									</span>
								</button>
							</Link>
						</div>
					</div>
				</div>
			</header>
		</>
	);
};

export default MainHeader;
