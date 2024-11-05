import toast from "react-hot-toast"
import { Radio } from "lucide-react"
import { Button } from "@/components/ui/button"
import { useEmailConfirmMutation } from "@/services/apiAuth"
import { useNavigate, useSearchParams } from "react-router-dom"

const ConfirmEmail = () => {
	const [searchParams] = useSearchParams()
	const navigate = useNavigate()
	const token = searchParams.get("token") ?? null

	// Confirm email
	const [confirmEmail] = useEmailConfirmMutation()

	const handleConfirmEmail = async () => {
		try {
			await confirmEmail(token).unwrap()
			toast.success("Email confirmed successfully")
			navigate("/login", { replace: true })
		} catch (error) {
			console.error(error)
			toast.error("Failed to confirm email")
		}
	}

	return (
		<div className="flex flex-col items-center justify-center w-full h-full">
			<div className="flex items-center justify-center ">
				<div className="w-60 h-60 relative">
					<iframe
						src="https://giphy.com/embed/StK13Ad9lHtdo1IqFA"
						width="100%"
						style={{ position: "absolute", border: 0 }}
						className="giphy-embed"
						allowFullScreen
					/>
				</div>
				<p>
					<a href="https://giphy.com/gifs/SWR-Kindernetz-cat-spin-katze-StK13Ad9lHtdo1IqFA">
						via GIPHY
					</a>
				</p>
			</div>
			<Button
				onClick={handleConfirmEmail}
				className="mt-4 flex items-center gap-2 bg-foreground text-background"
			>
				<Radio className="size-4" />
				Confirm your email
				<Radio className="size-4" />
			</Button>
		</div>
	)
}

export default ConfirmEmail

// NOTE: MAY BE USED FOR LATER
// <div
// 	style={{
// 		width: "100%",
// 		height: "100%",
// 		display: "flex",
// 		alignItems: "center",
// 		justifyContent: "center",
// 	}}
// >
// 	<div className="p-6 min-w-80 max-w-lg bg-foreground rounded-md">
// 		<Avatar>
// 			<AvatarImage src="/Spotify_Icon_RGB_Black.png" />
// 			<AvatarFallback>SP</AvatarFallback>
// 		</Avatar>
// 		<h1 className="text-xl font-bold text-background mt-4">Confirm your email address</h1>
// 		<p className="text-background text-sm mt-2">
// 			Please click the button below to confirm that hello@SmilesDavis.yeah is the correct email
// 			address to receive my newsletter.
// 		</p>
// 		<Button variant="transparent" className="mt-4 w-full text-center">
// 			Confirm your email
// 		</Button>
// 		<p className="text-xs text-[#919191] mt-2">
// 			Lorem ipsum dolor sit amet, consectetur adipisicing elit. In, cupiditate.
// 		</p>
// 	</div>
// </div>
