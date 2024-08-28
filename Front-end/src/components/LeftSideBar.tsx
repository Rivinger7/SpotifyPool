import { Link } from "react-router-dom";
import {
	Tooltip,
	TooltipContent,
	TooltipProvider,
	TooltipTrigger,
} from "@/components/ui/tooltip";
import { useState } from "react";

const LeftSideBar = () => {
	const [createPlaylist, setCreatePlaylist] = useState<boolean>(false);

	function handleCreatePlaylist(): void {
		setCreatePlaylist(!createPlaylist);
	}

	return (
		<div className={"left-sidebar"}>
			{/* ==== NAVBAR ==== */}
			<nav className={"flex flex-col gap-2 h-full"}>
				<div className={"bg-[var(--background-base)]  rounded-lg"}>
					<div className={"flex"}>
						<Link to={"/"} className={"pl-6 pr-6 mt-5"}>
							<img
								src="./Spotify_Logo_RGB_White.png"
								alt={"Spotify Logo White"}
								className={"h-6"}
							/>
						</Link>
					</div>
					<ul className={"p-2 pl-3 pr-3"}>
						<li className={"p-1 pl-3 pr-3"}>
							<Link
								to={"/"}
								className={
									"flex gap-5 h-10 items-center font-bold"
								}
							>
								<svg
									data-encore-id="icon"
									role="img"
									aria-hidden="true"
									className="w-6"
									fill={"currentColor"}
									viewBox="0 0 24 24"
								>
									<path d="M13.5 1.515a3 3 0 0 0-3 0L3 5.845a2 2 0 0 0-1 1.732V21a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1v-6h4v6a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V7.577a2 2 0 0 0-1-1.732l-7.5-4.33z"></path>
								</svg>
								Home
							</Link>
						</li>
						<li className={"p-1 pl-3 pr-3"}>
							<Link
								to={"/"}
								className={
									"flex gap-5 h-10 items-center font-bold text-[#b3b3b3] hover:text-white transition-all"
								}
							>
								<svg
									data-encore-id="icon"
									role="img"
									aria-hidden="true"
									className="w-6"
									fill={"currentColor"}
									viewBox="0 0 24 24"
								>
									<path d="M10.533 1.27893C5.35215 1.27893 1.12598 5.41887 1.12598 10.5579C1.12598 15.697 5.35215 19.8369 10.533 19.8369C12.767 19.8369 14.8235 19.0671 16.4402 17.7794L20.7929 22.132C21.1834 22.5226 21.8166 22.5226 22.2071 22.132C22.5976 21.7415 22.5976 21.1083 22.2071 20.7178L17.8634 16.3741C19.1616 14.7849 19.94 12.7634 19.94 10.5579C19.94 5.41887 15.7138 1.27893 10.533 1.27893ZM3.12598 10.5579C3.12598 6.55226 6.42768 3.27893 10.533 3.27893C14.6383 3.27893 17.94 6.55226 17.94 10.5579C17.94 14.5636 14.6383 17.8369 10.533 17.8369C6.42768 17.8369 3.12598 14.5636 3.12598 10.5579Z"></path>
								</svg>
								Search
							</Link>
						</li>
					</ul>
				</div>
				<div
					className={
						"flex flex-col w-full flex-1 relative bg-[var(--background-base)] rounded-lg"
					}
				>
					{/* ==== YOUR LIBRARY ====  */}
					<div
						className={
							"left-sidebar-library flex flex-col flex-1 w-full"
						}
					>
						<div className="library-header">
							<header className={"p-2 pl-4 pr-4"}>
								<div className={"flex justify-between"}>
									<div className={"flex items-center"}>
										<TooltipProvider>
											<Tooltip>
												<TooltipTrigger>
													<button
														className={
															"flex items-center p-1 pl-2 pr-2 gap-3 h-10 font-bold text-[#b3b3b3] hover:text-white transition-all"
														}
													>
														<svg
															data-encore-id="icon"
															role="img"
															aria-hidden="true"
															className="w-6"
															fill={
																"currentColor"
															}
															viewBox="0 0 24 24"
														>
															<path d="M3 22a1 1 0 0 1-1-1V3a1 1 0 0 1 2 0v18a1 1 0 0 1-1 1zM15.5 2.134A1 1 0 0 0 14 3v18a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V6.464a1 1 0 0 0-.5-.866l-6-3.464zM9 2a1 1 0 0 0-1 1v18a1 1 0 1 0 2 0V3a1 1 0 0 0-1-1z"></path>
														</svg>
														<span>
															Your Library
														</span>
													</button>
												</TooltipTrigger>
												<TooltipContent>
													<p>Collapse your library</p>
												</TooltipContent>
											</Tooltip>
										</TooltipProvider>
									</div>
									<span className={"block relative"}>
										<TooltipProvider>
											<Tooltip>
												<TooltipTrigger
													className={
														"hover:bg-[#1f1f1f] rounded-full p-2 font-bold text-[#b3b3b3] hover:text-white transition-all"
													}
													onClick={
														handleCreatePlaylist
													}
												>
													<svg
														data-encore-id="icon"
														role="img"
														aria-hidden="true"
														viewBox="0 0 16 16"
														className="w-4"
														fill={"currentColor"}
													>
														<path d="M15.25 8a.75.75 0 0 1-.75.75H8.75v5.75a.75.75 0 0 1-1.5 0V8.75H1.5a.75.75 0 0 1 0-1.5h5.75V1.5a.75.75 0 0 1 1.5 0v5.75h5.75a.75.75 0 0 1 .75.75z"></path>
													</svg>
												</TooltipTrigger>
												<TooltipContent>
													<p>
														Create playlist or
														folder
													</p>
												</TooltipContent>
											</Tooltip>
										</TooltipProvider>
										{createPlaylist && (
											<div
												className={
													"absolute right-0 top-10 p-1 bg-[#282828] rounded-sm"
												}
											>
												<div
													onClick={
														handleCreatePlaylist
													}
													className={
														"flex items-center justify-between p-3 cursor-default min-w-[190px] h-10 text-[#b3b3b3] hover:text-white transition-all hover:bg-[hsla(0,0%,100%,.1)]"
													}
												>
													<svg
														data-encore-id="icon"
														role="img"
														aria-hidden="true"
														className="w-4"
														fill={"currentColor"}
														viewBox="0 0 16 16"
													>
														<path d="M2 0v2H0v1.5h2v2h1.5v-2h2V2h-2V0H2zm11.5 2.5H8.244A5.482 5.482 0 0 0 7.966 1H15v11.75A2.75 2.75 0 1 1 12.25 10h1.25V2.5zm0 9h-1.25a1.25 1.25 0 1 0 1.25 1.25V11.5zM4 8.107a5.465 5.465 0 0 0 1.5-.593v5.236A2.75 2.75 0 1 1 2.75 10H4V8.107zM4 11.5H2.75A1.25 1.25 0 1 0 4 12.75V11.5z"></path>
													</svg>
													Create a new playlist
												</div>
											</div>
										)}
									</span>
								</div>
							</header>
							<div
								className={
									"flex items-center gap-2 m-2 ml-4 mr-4"
								}
							></div>
						</div>
						{/*  ==== LIBRARY BODY ==== */}
						<div className="library-body h-full">
							<div className="library-body-container flex flex-col h-full gap-2 min-h-full p-2 pt-0">
								<section className="flex flex-col bg-[#1f1f1f] justify-center gap-5 rounded-lg m-2 ml-0 mr-0 p-4 pl-5 pr-5">
									<div className="flex flex-col gap-2">
										<span className="font-bold">
											Create your first playlist
										</span>
										<span className="text-[14px]">
											It's easy, we'll help you
										</span>
									</div>
									<div className="library-body-btn">
										<button className="bg-transparent border-0 cursor-pointer text-center touch-manipulation transition-all align-middle rounded-full">
											<span className="bg-white text-black flex items-center justify-center rounded-full font-bold p-1 pl-4 pr-4 text-[14px] h-8">
												Create playlist
											</span>
										</button>
									</div>
								</section>
								<section className="flex flex-col bg-[#1f1f1f] justify-center gap-5 rounded-lg m-2 ml-0 mr-0 p-4 pl-5 pr-5">
									<div className="flex flex-col gap-2">
										<span className="font-bold">
											Let's find some podcasts to follow
										</span>
										<span className="text-[14px]">
											We'll keep you updated on new
											episodes
										</span>
									</div>
									<div className="library-body-btn">
										<button className="bg-transparent border-0 cursor-pointer text-center touch-manipulation transition-all align-middle rounded-full">
											<span className="bg-white text-black flex items-center justify-center rounded-full font-bold p-1 pl-4 pr-4 text-[14px] h-8">
												Browse podcasts
											</span>
										</button>
									</div>
								</section>
							</div>
						</div>
					</div>
					{/* ==== SIDEBAR FOOTER ==== */}
					<div className={"left-sidebar-footer"}>
						<div className="left-sidebar-legal-links text-[#b3b3b3] text-[11px] m-8 ml-0 mr-0 p-6 pt-0 pb-0">
							<div className="linklists-container flex flex-wrap">
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/legal/end-user-agreement/"
										}
										className="text-link leading-7 mr-4"
									>
										<span>Legal</span>
									</Link>
								</div>
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/safetyandprivacy"
										}
										className="text-link leading-7 mr-4"
									>
										<span>Safety & Privacy Center</span>
									</Link>
								</div>
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/legal/privacy-policy/"
										}
										className="text-link leading-7 mr-4"
									>
										<span>Privacy Policy</span>
									</Link>
								</div>
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/legal/cookies-policy/"
										}
										className="text-link leading-7 mr-4"
									>
										<span>Cookies</span>
									</Link>
								</div>
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/legal/privacy-policy/#s3"
										}
										className="text-link leading-7 mr-4"
									>
										<span>About Ads</span>
									</Link>
								</div>
								<div className="link-container">
									<Link
										to={
											"https://www.spotify.com/vn-vi/accessibility"
										}
										className="text-link leading-7 mr-4"
									>
										<span>Accessibility</span>
									</Link>
								</div>
							</div>
							<Link
								to={
									"https://www.spotify.com/vn-vi/legal/cookies-policy/"
								}
								className={"hover:underline leading-7"}
								target={"_blank"}
								rel={"noopener"}
							>
								<span>Cookies</span>
							</Link>
						</div>
						<div className="left-sidebar-language mb-8 p-6 pt-0 pb-0">
							<button className="rounded-full border border-[#7c7c7c] p-4 pt-1 pb-1 font-bold cursor-pointer inline-flex items-center justify-center gap-1 text-[14px]">
								<span>
									<svg
										data-encore-id="icon"
										role="img"
										aria-hidden="true"
										viewBox="0 0 16 16"
										className="w-4"
										fill={"currentColor"}
									>
										<path d="M8.152 16H8a8 8 0 1 1 .152 0zm-.41-14.202c-.226.273-.463.713-.677 1.323-.369 1.055-.626 2.496-.687 4.129h3.547c-.06-1.633-.318-3.074-.687-4.129-.213-.61-.45-1.05-.676-1.323-.194-.235-.326-.285-.385-.296h-.044c-.055.007-.19.052-.391.296zM4.877 7.25c.062-1.771.34-3.386.773-4.624.099-.284.208-.554.329-.806a6.507 6.507 0 0 0-4.436 5.43h3.334zm-3.334 1.5a6.507 6.507 0 0 0 4.436 5.43 7.974 7.974 0 0 1-.33-.806c-.433-1.238-.71-2.853-.772-4.624H1.543zm4.835 0c.061 1.633.318 3.074.687 4.129.214.61.451 1.05.677 1.323.202.244.336.29.391.297l.044-.001c.06-.01.19-.061.385-.296.226-.273.463-.713.676-1.323.37-1.055.626-2.496.687-4.129H6.378zm5.048 0c-.061 1.771-.339 3.386-.772 4.624-.082.235-.171.46-.268.674a6.506 6.506 0 0 0 4.071-5.298h-3.03zm3.031-1.5a6.507 6.507 0 0 0-4.071-5.298c.097.214.186.44.268.674.433 1.238.711 2.853.772 4.624h3.031z"></path>
									</svg>
								</span>{" "}
								English
							</button>
						</div>
					</div>
				</div>
			</nav>
		</div>
	);
};

export default LeftSideBar;
