import { useState } from "react"
import { Link } from "react-router-dom"

import { Globe, Music4, Plus, SquareLibrary } from "lucide-react"
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import CustomTooltip from "@/components/CustomTooltip"

const LeftSideBar = () => {
	const [createPlaylist, setCreatePlaylist] = useState<boolean>(false)

	function handleCreatePlaylist(): void {
		setCreatePlaylist(!createPlaylist)
	}

	return (
		<div className="w-[420px] shrink-0">
			{/* ==== NAVBAR ==== */}
			<nav className={"flex flex-col gap-2 h-full"}>
				<div
					className={"flex flex-col w-full flex-1 relative bg-[var(--background-base)] rounded-lg"}
				>
					{/* ==== YOUR LIBRARY ====  */}
					<div className={"left-sidebar-library flex flex-col flex-1 w-full"}>
						<div className="library-header">
							<header className={"p-2 pl-4 pr-4"}>
								<div className={"flex justify-between"}>
									<div className={"flex items-center"}>
										<CustomTooltip label="Collapse your library" side="bottom">
											<div
												className={
													"flex items-center p-1 pl-2 pr-2 gap-3 h-10 font-bold text-[#b3b3b3] hover:text-white transition-all cursor-pointer"
												}
											>
												<SquareLibrary className="size-6" />
												<span>Your Library</span>
											</div>
										</CustomTooltip>
									</div>
									<span className={"block relative"}>
										<DropdownMenu>
											<DropdownMenuTrigger>
												<CustomTooltip label="Create playlist or folder" side="bottom">
													<div
														className={
															"hover:bg-[#1f1f1f] rounded-full p-2 font-bold text-[#b3b3b3] hover:text-white transition-all"
														}
														onClick={handleCreatePlaylist}
													>
														<Plus className="size-6 fill-current" />
													</div>
												</CustomTooltip>
											</DropdownMenuTrigger>

											<DropdownMenuContent align="end" className="border-none bg-[#282828] w-52">
												<div
													onClick={handleCreatePlaylist}
													className={
														"flex items-center justify-between p-3 cursor-default min-w-[190px] h-10 text-[#b3b3b3] hover:text-white transition-all hover:bg-[hsla(0,0%,100%,0.1)]"
													}
												>
													<Music4 className="size-4" />
													Create a new playlist
												</div>
											</DropdownMenuContent>
										</DropdownMenu>
									</span>
								</div>
							</header>
							<div className={"flex items-center gap-2 m-2 ml-4 mr-4"}></div>
						</div>

						{/*  ==== LIBRARY BODY ==== */}
						<div className="h-full library-body">
							<div className="flex flex-col h-full min-h-full gap-2 p-2 pt-0 library-body-container">
								{/* CREATE PLAYLIST */}
								<section className="flex flex-col bg-[#1f1f1f] justify-center gap-5 rounded-lg m-2 ml-0 mr-0 p-4 pl-5 pr-5">
									<div className="flex flex-col gap-2">
										<span className="font-bold">Create your first playlist</span>
										<span className="text-[14px]">It's easy, we'll help you</span>
									</div>
									<div className="library-body-btn">
										<button className="text-center align-middle transition-all bg-transparent border-0 rounded-full cursor-pointer touch-manipulation hover:scale-105">
											<span className="bg-white text-black flex items-center justify-center rounded-full font-bold p-1 pl-4 pr-4 text-[14px] h-8">
												Create playlist
											</span>
										</button>
									</div>
								</section>

								{/* BROWSE PODCASTS */}
								<section className="flex flex-col bg-[#1f1f1f] justify-center gap-5 rounded-lg m-2 ml-0 mr-0 p-4 pl-5 pr-5">
									<div className="flex flex-col gap-2">
										<span className="font-bold">Let's find some podcasts to follow</span>
										<span className="text-[14px]">We'll keep you updated on new episodes</span>
									</div>
									<div className="library-body-btn">
										<button className="text-center align-middle transition-all bg-transparent border-0 rounded-full cursor-pointer touch-manipulation hover:scale-105">
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
							<div className="flex flex-wrap linklists-container">
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/legal/end-user-agreement/"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">Legal</span>
									</Link>
								</div>
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/safetyandprivacy"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">Safety & Privacy Center</span>
									</Link>
								</div>
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/legal/privacy-policy/"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">Privacy Policy</span>
									</Link>
								</div>
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/legal/cookies-policy/"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">Cookies</span>
									</Link>
								</div>
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/legal/privacy-policy/#s3"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">About Ads</span>
									</Link>
								</div>
								<div className="group">
									<Link
										to={"https://www.spotify.com/vn-vi/accessibility"}
										className="mr-4 leading-7"
									>
										<span className="group-hover:text-white">Accessibility</span>
									</Link>
								</div>
							</div>
						</div>
						<div className="p-6 pt-0 pb-0 mb-8 left-sidebar-language">
							<button className="rounded-full border border-[#7c7c7c] p-4 pt-1 pb-1 font-bold cursor-pointer inline-flex items-center justify-center gap-1 text-[14px] hover:border-white hover:scale-105">
								<span>
									<Globe className="size-4" />
								</span>{" "}
								English
							</button>
						</div>
					</div>
				</div>
			</nav>
		</div>
	)
}

export default LeftSideBar
