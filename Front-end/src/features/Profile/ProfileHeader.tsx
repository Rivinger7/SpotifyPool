import { User } from "@/types"
import { Loader, Pen } from "lucide-react"
import { useGetUserProfileQuery } from "@/services/apiUser.ts"

import useGetUserId from "./hooks/useGetUserId"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"

interface ProfileHeaderProps {
	setOpen: (open: boolean) => void
}

export default function ProfileHeader({ setOpen }: ProfileHeaderProps) {
	const userId = useGetUserId()

	const { data: user, isLoading } = useGetUserProfileQuery(userId) as {
		data: User
		isLoading: boolean
	}

	if (isLoading) {
		return <Loader className={"animate-spin size-10"} />
	}

	return (
		<div className="profile">
			<div className="bg-style" style={{ backgroundColor: "rgb(136, 64, 56)" }}></div>
			<div className="bg-style gradient"></div>
			<div className="info">
				<div className="user-image">
					<div className="style relative">
						<div className="w-full h-full">
							<Avatar className="bg-[#1f1f1f] items-center justify-center cursor-pointer transition-all w-full h-full">
								<AvatarImage
									referrerPolicy="no-referrer"
									src={user?.avatar[0].url}
									// className="object-cover rounded-full w-8 h-8"
									className="rounded-full object-cover"
								/>

								<AvatarFallback className="bg-green-500 text-sky-100 font-bold text-7xl">
									{user?.name.charAt(0).toUpperCase()}
								</AvatarFallback>
							</Avatar>
						</div>

						<div className="cta-btn">
							<div className="cover">
								<button type="button" className="edit-image-button" onClick={() => setOpen(true)}>
									<div className="icon">
										<Pen className="size-12" />
										<span>Choose photo</span>
									</div>
								</button>
							</div>
						</div>
					</div>
				</div>
				<div className="user-name">
					<span className="text-sm">Profile</span>
					<span>
						<button type="button" onClick={() => setOpen(true)}>
							<h1 className="font-bold tracking-tight text-8xl">{user?.name}</h1>
						</button>
					</span>
					<span className="mt-4">1 Public playlist</span>
				</div>
			</div>
		</div>
	)
}
