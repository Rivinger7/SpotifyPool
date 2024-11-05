import {
	DropdownMenu,
	DropdownMenuItem,
	DropdownMenuLabel,
	DropdownMenuContent,
	DropdownMenuTrigger,
	DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu"

import toast from "react-hot-toast"
import { Button } from "@/components/ui/button"
import { House, Package, Search } from "lucide-react"
import CustomTooltip from "@/components/CustomTooltip"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"

import { RootState } from "@/store/store"
import { logout } from "@/store/slice/authSlice"
import { useDispatch, useSelector } from "react-redux"
import { Link, useLocation, useNavigate } from "react-router-dom"

const MainHeader = () => {
	const navigate = useNavigate()
	const dispatch = useDispatch()
	const location = useLocation()

	const pathname = location.pathname

	const { userData, isAuthenticated } = useSelector((state: RootState) => state.auth)

	const handleLogout = () => {
		dispatch(logout())
		toast.success("Logout successful")
	}

	return (
		<header className="w-full flex items-center relative bg-[rgba(0,0,0,.5)] p-2 -m-2 h-16">
			<div className="w-full flex items-center justify-between pl-6 h-12">
				{/* LOGO */}
				<div className="pointer-events-auto z-20">
					<Link to={"/"}>
						<CustomTooltip label="Spotify" side="bottom" align="center">
							<img
								src="Spotify_Icon_RGB_White.png"
								alt="Spotify Logo white RGB"
								className="size-8"
							/>
						</CustomTooltip>
					</Link>
				</div>

				{/* <div className="flex-grow" /> */}

				{/* HOME AND SEARCH */}
				<div className="absolute flex items-center z-10 justify-center left-0 right-0 w-full pointer-events-auto">
					<div className="flex items-center gap-2 min-w-[350px] max-w-[546px] w-1/2">
						<div className="shrink-0">
							<CustomTooltip label="Home" side="bottom" align="center">
								<Button
									variant={"normal"}
									size={"iconLarge"}
									className="group"
									onClick={() => navigate("/")}
								>
									<House
										className={`text-[#b3b3b3] size-6 group-hover:text-white ${
											pathname === "/" ? "text-white" : ""
										}`}
									/>
								</Button>
							</CustomTooltip>
						</div>

						<form className="relative group w-full h-full" onClick={() => navigate("/search")}>
							<CustomTooltip label="Search" side="bottom">
								<Search className="text-[#b3b3b3] group-hover:text-white cursor-pointer size-6 absolute left-3 top-1/2 -translate-y-1/2" />
							</CustomTooltip>

							<input
								type="text"
								placeholder="What do you want to play?"
								className="bg-[#1f1f1f] text-[#b3b3b3] cursor-pointer p-3 pl-12 pr-16 rounded-full w-full focus:outline-none focus:ring-2 focus:ring-primary focus:ring-opacity-50 truncate min-h-12"
							/>

							<div className="absolute right-3 top-1/2 -translate-y-1/2 pl-3 border-l bordeer-solid border-[#7c7c7c]">
								<CustomTooltip label="Browse" side="bottom">
									<Package
										className={`text-[#b3b3b3] hover:text-white cursor-pointer size-6 ${
											pathname === "/search" ? "text-white" : ""
										}`}
									/>
								</CustomTooltip>
							</div>
						</form>
					</div>
				</div>

				{/* <div className="flex-grow" /> */}

				{/* AVATAR OR AUTH ACTION */}
				{!isAuthenticated ? (
					<div className="pointer-events-auto z-20">
						<Link to={"/signup"}>
							<button className="inline-flex items-center justify-center text-[#b3b3b3] p-2 pr-8 font-bold hover:text-white hover:scale-x-105">
								Sign up
							</button>
						</Link>

						<Link to={"/login"}>
							<button className="group">
								<span className="bg-white text-black flex items-center justify-center transition-all p-2 pl-8 pr-8 rounded-full font-bold min-h-12 group-hover:scale-105 group-hover:bg-[#f0f0f0]">
									Log in
								</span>
							</button>
						</Link>
					</div>
				) : (
					<div className="pointer-events-auto z-20">
						<DropdownMenu>
							<DropdownMenuTrigger>
								<CustomTooltip label={userData?.displayName} side="bottom" align="center">
									{/* AVATAR IMAGE */}
									<Avatar className="bg-[#1f1f1f] items-center justify-center cursor-pointer hover:scale-110 transition-all w-12 h-12">
										<AvatarImage
											referrerPolicy="no-referrer"
											src={userData?.avatar}
											className="object-cover rounded-full w-8 h-8"
										/>

										<AvatarFallback className="bg-green-500 text-sky-100 font-bold w-8 h-8">
											{userData?.displayName.charAt(0).toUpperCase()}
										</AvatarFallback>
									</Avatar>
								</CustomTooltip>
							</DropdownMenuTrigger>

							<DropdownMenuContent className="border-none bg-[#282828] w-52">
								<DropdownMenuLabel className="w-full text-lg font-bold">
									{userData?.displayName}
								</DropdownMenuLabel>

								<DropdownMenuSeparator />

								{/* PROFILE BUTTON */}
								<DropdownMenuItem className="p-3 pr-2">
									<Link to={"/user"} className="w-full text-lg">
										Profile
									</Link>
								</DropdownMenuItem>

								<DropdownMenuItem className="p-3 pr-2">
									<Link to={"/settings"} className="w-full text-lg">
										Settings
									</Link>
								</DropdownMenuItem>

								<DropdownMenuSeparator />

								{/* LOGOUT BUTTON */}
								<DropdownMenuItem className="p-3 pr-2">
									<div onClick={handleLogout} className="w-full text-lg">
										Logout
									</div>
								</DropdownMenuItem>
							</DropdownMenuContent>
						</DropdownMenu>
					</div>
				)}
			</div>
		</header>
	)
}

export default MainHeader
