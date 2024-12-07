import {User} from "@/types";

import EditIcon from "@/assets/icons/EditIcon"
import useGetUserId from "./hooks/useGetUserId"
import ProfileModal from "./components/Modal/ProfileModal"
import {Avatar, AvatarFallback, AvatarImage} from "@/components/ui/avatar"

import {useGetUserProfileQuery} from "@/services/apiUser.ts";
import {Loader} from "lucide-react";


export default function ProfileHeader() {
    const userId = useGetUserId()

    const {data: user, isLoading} = useGetUserProfileQuery(userId) as {
        data: User,
        isLoading: boolean
    }

    if (isLoading) {
        return <Loader className={"animate-spin size-10"}/>
    }

    return (
        <div className="profile">
            <div className="bg-style" style={{backgroundColor: "rgb(136, 64, 56)"}}></div>
            <div className="bg-style gradient"></div>
            <div className="info">
                <div className="user-image">
                    <div className="style">
                        <div className="image">
                            <Avatar
                                className="bg-[#1f1f1f] items-center justify-center cursor-pointer hover:scale-110 transition-all w-full h-full">
                                <AvatarImage
                                    referrerPolicy="no-referrer"
                                    src={user?.avatar[0].url}
                                    // className="object-cover rounded-full w-8 h-8"
                                    className="rounded-full"
                                />

                                <AvatarFallback
                                    className="bg-green-500 text-sky-100 font-bold text-7xl">
                                    {user?.name.charAt(0).toUpperCase()}
                                </AvatarFallback>
                            </Avatar>
                        </div>
                        <div className="cta-btn">
                            <div className="cover">
                                <ProfileModal>
                                    <button type="button" className="edit-image-button">
                                        <div className="icon">
                                            <EditIcon/>
                                            <span
                                                className="encore-text encore-text-body-medium jN7ZUHc7IxpwvWsjb4jo"
                                                data-encore-id="text"
                                            >
												Choose photo
											</span>
                                        </div>
                                    </button>
                                </ProfileModal>
                            </div>
                        </div>
                    </div>
                </div>
                <div className="user-name">
                    <span className="text-sm">Profile</span>
                    <span>
						<ProfileModal>
							<button type="button">
								<h1 className="font-bold tracking-tight text-8xl">{user?.name}</h1>
							</button>
						</ProfileModal>
					</span>
                    <span className="mt-4">1 Public playlist</span>
                </div>
            </div>
        </div>
    )
}
