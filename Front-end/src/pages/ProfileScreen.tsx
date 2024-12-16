import "@/styles/profile.scss"
import { useState } from "react"

import ProfileBulk from "@/features/Profile/ProfileBulk"
import ProfileHeader from "@/features/Profile/ProfileHeader"
import ProfileModal from "@/features/Profile/components/Modal/ProfileModal"
import ProfileTopTracks from "@/features/Profile/ProfileTopTracks"

export default function ProfileScreen() {
	const [openProfileModal, setOpenProfileModal] = useState(false)

	return (
		<div>
			<ProfileModal open={openProfileModal} setOpen={setOpenProfileModal} />

			<ProfileHeader setOpen={setOpenProfileModal} />

			<ProfileBulk setOpen={setOpenProfileModal} />

			<ProfileTopTracks />
		</div>
	)
}
