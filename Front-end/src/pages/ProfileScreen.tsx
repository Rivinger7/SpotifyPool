import ProfileBulk from "@/features/Profile/ProfileBulk"
import ProfileHeader from "@/features/Profile/ProfileHeader"
import "@/styles/profile.scss"

export default function ProfileScreen() {
	return (
		<div>
			<ProfileHeader />

			<ProfileBulk />
		</div>
	)
}
