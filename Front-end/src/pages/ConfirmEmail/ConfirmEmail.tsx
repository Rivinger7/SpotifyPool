import { useEffect, useMemo } from "react"

const ConfirmEmail = () => {
	// Memoize the audio object
	const funnySound = useMemo(
		() => new Audio("/sound/Gregorius Boss Orchestra (Blue Archive).mp3"),
		[]
	)

	useEffect(() => {
		// Set the volume
		funnySound.volume = 0.05

		// Play the sound when the component is mounted
		const playSound = async () => {
			try {
				await funnySound.play()
			} catch (err) {
				console.error("Failed to play sound:", err)
			}
		}

		playSound()
	}, [funnySound])

	return (
		// TODO: MAY BE USED FOR LATER
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

		<div className="w-full h-full flex items-center justify-center ">
			<div className="w-60 h-60  relative">
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
	)
}

export default ConfirmEmail
