import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog"
import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form"

import { z } from "zod"
import toast from "react-hot-toast"
import { RootState } from "@/store/store"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useDispatch, useSelector } from "react-redux"
import { appendPlaylist } from "@/store/slice/playlistSlice"

import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { HttpTransportType, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"

interface AddPlaylistModalProps {
	open: boolean
	setOpen: (open: boolean) => void
}

const formSchema = z.object({
	playlistName: z.string(),
	playlistDescription: z.string(),
})

const AddPlaylistModal = ({ open, setOpen }: AddPlaylistModalProps) => {
	const dispatch = useDispatch()
	const { userToken } = useSelector((state: RootState) => state.auth)

	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			playlistName: "",
			playlistDescription: "",
		},
	})

	const onSubmit = (values: z.infer<typeof formSchema>) => {
		try {
			const connection = new HubConnectionBuilder()
				.withUrl(import.meta.env.VITE_SPOTIFYPOOL_HUB_ADD_TO_PLAYLIST_URL, {
					transport: HttpTransportType.WebSockets, // INFO: set transport ở đây thànhh websockets để sử dụng skipNegotiation
					accessTokenFactory: () => `${userToken?.accessToken}`,
				})
				.configureLogging(LogLevel.Debug) // INFO: set log level ở đây để tắt log -- khôngg cho phép log ra client
				.build()

			connection
				.start()
				.then(() => {
					console.log("Connected to the hub")
					connection.invoke("CreatePlaylistAsync", values.playlistName)
				})
				.catch((err) => console.error(err))

			// NOTE: Khởi tạo 1 playlist mới
			connection.on("CreatePlaylistSuccessfully", (newPlaylist) => {
				dispatch(appendPlaylist(newPlaylist))
				toast.success("Added to Favorite Songs.", {
					position: "bottom-center",
				})

				connection
					.stop()
					.then(() => console.log("Connection stopped by client"))
					.catch((err) => console.error("Error stopping connection", err))
			})

			// NOTE: Khi sự kiện này diễn ra signalR sẽ dừng hoạt động và trả về lỗi
			connection.on("ReceiveException", (message) => {
				toast.error(message, {
					position: "top-right",
					duration: 2000,
				})

				console.log(message)
			})

			connection.onclose((error) => {
				if (error) {
					console.error("Connection closed due to error:", error)
					toast.error("Connection lost. Please try again.")
				} else {
					console.log("Connection closed by the server.")
				}
			})

			form.reset()
			setOpen(false)
		} catch (error) {
			console.error(error)
		}
	}

	return (
		<Dialog open={open} onOpenChange={setOpen}>
			<DialogContent className="sm:max-w-[524px] border-none bg-[#282828]">
				<DialogHeader>
					<DialogTitle className="text-2xl font-bold tracking-wide">Playlist details</DialogTitle>

					<DialogDescription className="hidden">viet lam deo gi</DialogDescription>
				</DialogHeader>
				<Form {...form}>
					<form onSubmit={form.handleSubmit(onSubmit)} className="space-y-2">
						<FormField
							control={form.control}
							name="playlistName"
							render={({ field }) => (
								<FormItem>
									<FormLabel className="text-xl capitalize">Name</FormLabel>
									<FormControl>
										<Input className="rounded-sm" placeholder="Add a name" {...field} />
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						<FormField
							control={form.control}
							name="playlistDescription"
							render={({ field }) => (
								<FormItem>
									<FormLabel className="text-xl capitalize">Description</FormLabel>
									<FormControl>
										<Textarea
											className="rounded-sm"
											placeholder="Add an optional description"
											{...field}
										/>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						<div className="flex justify-end w-full mt-4">
							<Button
								className="rounded-full bg-[#fff] px-8 py-2 text-lg  hover:bg-[f0f0f0] hover:scale-105 font-bold"
								type="submit"
							>
								Save
							</Button>
						</div>
					</form>
				</Form>
				<DialogFooter>
					<p className="text-xs font-bold">
						By proceeding, you agree to give Spotify access to the image you choose to upload.
						Please make sure you have the right to upload the image.
					</p>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	)
}

export default AddPlaylistModal
