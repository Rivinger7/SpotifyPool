import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import CustomTooltip from "@/components/CustomTooltip"

import { Copy, Ellipsis, Pen } from "lucide-react"
import toast from "react-hot-toast"
import ProfileModal from "./components/Modal/ProfileModal"

const ProfileBulk = () => {
	const handleCopy = () => {
		navigator.clipboard.writeText(window.location.href)

		toast.success("Link copied to clipboard", {
			position: "bottom-right",
		})
	}

	return (
		<div className="w-full p-6">
			{/* // TODO: Change the name of user here */}
			<DropdownMenu>
				<DropdownMenuTrigger>
					<CustomTooltip label={`More options for {username}`} side="right">
						<Ellipsis className="size-6 text-[#b3b3b3] hover:text-white cursor-pointer" />
					</CustomTooltip>
				</DropdownMenuTrigger>

				<DropdownMenuContent align="start" className="border-none bg-[#282828] min-w-40">
					<DropdownMenuItem
						className={
							"flex items-center justify-start gap-2 p-3 cursor-default h-10 text-[#b3b3b3] hover:text-white transition-all hover:bg-[hsla(0,0%,100%,0.1)] text-lg"
						}
					>
						<ProfileModal>
							<>
								<Pen className="size-5" />
								Edit profile
							</>
						</ProfileModal>
					</DropdownMenuItem>

					<DropdownMenuItem
						onClick={handleCopy}
						className={
							"flex items-center justify-start gap-2 p-3 cursor-default h-10 text-[#b3b3b3] hover:text-white transition-all hover:bg-[hsla(0,0%,100%,0.1)] text-lg"
						}
					>
						<Copy className="size-5 rotate-90" />
						Copy link to profile
					</DropdownMenuItem>
				</DropdownMenuContent>
			</DropdownMenu>
		</div>
	)
}

export default ProfileBulk
