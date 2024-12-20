import { useEffect } from "react"
import { RootState } from "@/store/store.ts"
import { useDispatch, useSelector } from "react-redux"
import { toggleCollapse } from "@/store/slice/uiSlice"

import { useNavigate } from "react-router-dom"

import { Folder, Loader, Music4, Plus, SquareLibrary } from "lucide-react"
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import CustomTooltip from "@/components/CustomTooltip"

import { Playlist } from "@/types"
import { setPlaylist } from "@/store/slice/playlistSlice.ts"
import SidebarFooter from "@/features/Layout/SidebarFooter.tsx"
import { useGetAllPlaylistsQuery } from "@/services/apiPlaylist.ts"
import PlayListsSidebar from "@/features/Playlist/PlayListsSidebar.tsx"

const LeftSideBar = () => {
	const navigate = useNavigate()
	const dispatch = useDispatch()
	const { playlists } = useSelector((state: RootState) => state.playlist)
	const { isAuthenticated } = useSelector((state: RootState) => state.auth)
	const { isCollapsed } = useSelector((state: RootState) => state.ui)

	const handleCollapse = () => {
		if (isAuthenticated) {
			dispatch(toggleCollapse())
		}
	}

	// Fetch playlists
	const { data: playlistsData, isLoading: isLoadingPlaylist } = useGetAllPlaylistsQuery(
		{},
		{
			skip: !isAuthenticated,
		}
	) as {
		data: Playlist[]
		isLoading: boolean
	}

	useEffect(() => {
		dispatch(setPlaylist(playlistsData))
	}, [dispatch, playlistsData])

	const hasPlaylists = playlists && playlists.length > 0
	// Render logic based on playlists and authentication
	const shouldShowLibraryBody = !isAuthenticated || (isAuthenticated && !hasPlaylists)
	const shouldShowPlaylistsSidebar = isAuthenticated && hasPlaylists

	return (
		<div className={`${isCollapsed ? "w-[72px]" : "w-[420px]"} shrink-0 max-h-full`}>
			{/* ==== NAVBAR ==== */}
			<nav className={"flex flex-col gap-2 h-full"}>
				<div
					className={
						"flex flex-col w-full h-full flex-1 relative bg-[var(--background-base)] rounded-lg"
					}
				>
					{/* ==== YOUR LIBRARY ====  */}
					<div className={"left-sidebar-library flex flex-col flex-1 w-full overflow-x-hidden"}>
						{/*  ==== LIBRARY HEADER ==== */}
						<div className="library-header">
							<header className={"p-2 pl-4 pr-4"}>
								<div className={`flex justify-between ${isCollapsed ? "flex-col" : "flex-row"}`}>
									<CustomTooltip
										label={`${isCollapsed ? "Expand" : "Collapse"} your library`}
										side={isCollapsed ? "right" : "bottom"}
									>
										<div
											className={`flex items-center p-1 pl-2 pr-2 gap-3 h-10 font-bold text-[#b3b3b3] hover:text-white transition-all cursor-pointer`}
											onClick={handleCollapse}
										>
											<SquareLibrary className="size-6" />
											{!isCollapsed && <span>Your Library</span>}
										</div>
									</CustomTooltip>
									<span className={"block relative"}>
										<DropdownMenu>
											<DropdownMenuTrigger>
												<CustomTooltip
													label="Create playlist or folder"
													side={isCollapsed ? "right" : "bottom"}
												>
													<div
														className={
															"hover:bg-[#1f1f1f] rounded-full p-2 font-bold text-[#b3b3b3] hover:text-white transition-all"
														}
													>
														<Plus className="size-6 fill-current" />
													</div>
												</CustomTooltip>
											</DropdownMenuTrigger>

											<DropdownMenuContent
												align={`${isCollapsed ? "start" : "end"}`}
												className="border-none bg-[#282828]"
											>
												{/*<div*/}
												{/*	className={*/}
												{/*		"flex items-center justify-between p-3 cursor-default min-w-[190px] h-10 text-[#b3b3b3] hover:text-white transition-all hover:bg-[hsla(0,0%,100%,0.1)]"*/}
												{/*	}*/}
												{/*>*/}
												{/*	<Music4 className="size-4" />*/}
												{/*	<span>Create a new playlist</span>*/}
												{/*</div>*/}
												<DropdownMenuItem>
													<Music4 className="size-4" />
													<span>Create a new playlist</span>
												</DropdownMenuItem>

												<DropdownMenuItem>
													<Folder className="size-4" />
													<span>Create a playlist folder</span>
												</DropdownMenuItem>
											</DropdownMenuContent>
										</DropdownMenu>
									</span>
								</div>
							</header>
						</div>

						{isLoadingPlaylist && (
							<div className={"flex items-center justify-center"}>
								<Loader className="size-12 animate-spin" />
							</div>
						)}

						{/*  ==== LIBRARY BODY ==== */}
						{shouldShowLibraryBody && (
							<div className={`h-full overflow-y-auto ${isCollapsed ? "hidden" : ""}`}>
								<div className="flex flex-col max-h-full overflow-y-auto gap-2 p-2 pt-0 library-body-container">
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
											<button
												onClick={() => navigate("/search")}
												className="text-center align-middle transition-all bg-transparent border-0 rounded-full cursor-pointer touch-manipulation hover:scale-105"
											>
												<span className="bg-white text-black flex items-center justify-center rounded-full font-bold p-1 pl-4 pr-4 text-[14px] h-8">
													Browse podcasts
												</span>
											</button>
										</div>
									</section>
								</div>
							</div>
						)}

						{shouldShowPlaylistsSidebar && <PlayListsSidebar />}
					</div>

					{/* ==== SIDEBAR FOOTER ==== */}
					{!isAuthenticated && <SidebarFooter />}
				</div>
			</nav>
		</div>
	)
}

export default LeftSideBar
