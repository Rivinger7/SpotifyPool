import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Input } from "@/components/ui/input"
import { useRef } from "react"

interface ProfileModalProps {
	children: React.ReactNode
}

const EditIcon = (
	<svg viewBox="0 0 24 24" className="w-12 h-12" fill="currentcolor">
		<path d="M17.318 1.975a3.329 3.329 0 1 1 4.707 4.707L8.451 20.256c-.49.49-1.082.867-1.735 1.103L2.34 22.94a1 1 0 0 1-1.28-1.28l1.581-4.376a4.726 4.726 0 0 1 1.103-1.735L17.318 1.975zm3.293 1.414a1.329 1.329 0 0 0-1.88 0L5.159 16.963c-.283.283-.5.624-.636 1l-.857 2.372 2.371-.857a2.726 2.726 0 0 0 1.001-.636L20.611 5.268a1.329 1.329 0 0 0 0-1.879z" />
	</svg>
)

const formSchema = z.object({
	// username: z.string(),
	imageFile: z.instanceof(File).optional(),
})

function ProfileModal({ children }: ProfileModalProps) {
	const fileInputRef = useRef<HTMLInputElement | null>(null)

	const handleClick = () => {
		if (fileInputRef.current) {
			fileInputRef.current.click()
		}
	}

	// 1. Define your form.
	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			// username: "",
			imageFile: undefined,
		},
	})

	// 2. Define a submit handler.
	function onSubmit(values: z.infer<typeof formSchema>) {
		const formData = new FormData()

		// Check if the image file exists
		const file = fileInputRef.current?.files ? fileInputRef.current.files[0] : null

		console.log("file", file)

		// If there is a file, add the file to FormData, otherwise add the URL
		if (file) {
			formData.append("imageFile", file) // Add file to FormData
		}

		// formData.append("username", values.username); // Add other fields

		// Send data to the API
		// axios
		// 	.post("https://localhost:7018/api/media/upload-video", formData, {
		// 		headers: {
		// 			"Content-Type": "multipart/form-data",
		// 		},
		// 	})
		// 	.then((response) => {
		// 		toast.success("Image uploaded successfully");
		// 	})
		// 	.catch((error) => {
		// 		console.error("Error uploading:", error);
		// 	});
	}

	return (
		<Dialog>
			<DialogTrigger asChild>{children}</DialogTrigger>
			<DialogContent className="sm:max-w-[524px] border-none bg-[#282828]">
				<DialogHeader>
					<DialogTitle className="text-2xl font-bold tracking-wide">Profile details</DialogTitle>

					<DialogDescription className="hidden">viet lam deo gi</DialogDescription>
				</DialogHeader>
				<Form {...form}>
					<form className="grid grid-cols-[180px_1fr] gap-4" onSubmit={form.handleSubmit(onSubmit)}>
						<div className="h-[180px] w-[180px] relative">
							<div className="flex w-full h-full cursor-pointer group" onClick={handleClick}>
								<div className="w-full h-full">
									<img
										src="/avatar-formal.jpg"
										alt=""
										className="object-cover object-center rounded-full w-full h-full shadow-[0_4px_60px_rgba(0,0,0, .5)]"
									/>
								</div>
								<div className="absolute top-0 bottom-0 left-0 right-0 z-10 flex flex-col items-center justify-center opacity-0 group-hover:opacity-100 group-hover:bg-[rgba(0,0,0,.7)] group-hover:rounded-full transition-all duration-300 ">
									{EditIcon}
									<span className="mt-2 text-base font-semibold">Choose photo</span>
								</div>
							</div>
						</div>
						<div>
							<FormField
								control={form.control}
								name="imageFile"
								render={({ field }) => (
									<FormItem className="hidden">
										<FormLabel>Image URL</FormLabel>
										<FormControl>
											<Input
												type="file"
												accept=".mp3"
												className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
												onChange={(e) => {
													const file = e.target.files ? e.target.files[0] : null
													field.onChange(file)
												}}
												ref={fileInputRef} // Attach the ref to the file input
											/>
										</FormControl>
										<FormMessage />
									</FormItem>
								)}
							/>
							{/* <FormField
								control={form.control}
								name="username"
								render={({ field }) => (
									<FormItem>
										<FormLabel className="text-xl capitalize"> username</FormLabel>
										<FormControl>
											<Input
												className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
												placeholder="Username"
												{...field}
											/>
										</FormControl>
										<FormMessage />
									</FormItem>
								)}
							/> */}
						</div>
						{/* // FIXME: check again why this not trigger the onSubmit */}
						<span className="flex justify-end w-full mt-4">
							<Button
								className="rounded-full bg-[#fff] px-8 py-2 text-lg  hover:bg-[f0f0f0] hover:scale-105 font-bold"
								type="submit"
							>
								Save
							</Button>
						</span>
					</form>
				</Form>
				<DialogFooter>
					<p className="text-xs">
						By proceeding, you agree to give Spotify access to the image you choose to upload.
						Please make sure you have the right to upload the image.
					</p>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	)
}

export default ProfileModal
