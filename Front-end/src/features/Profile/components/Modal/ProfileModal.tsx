import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
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
import { z } from "zod"
import { useRef } from "react"
import { Pen } from "lucide-react"
import { useForm } from "react-hook-form"
import { Input } from "@/components/ui/input"
import { zodResolver } from "@hookform/resolvers/zod"

interface ProfileModalProps {
	open: boolean
	setOpen: (open: boolean) => void
}

const formSchema = z.object({
	// username: z.string(),
	imageFile: z.instanceof(File).optional(),
})

function ProfileModal({ open, setOpen }: ProfileModalProps) {
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
	// NOTE: The type of `values` is inferred from the schema.
	// values: z.infer<typeof formSchema>
	function onSubmit() {
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
		<Dialog open={open} onOpenChange={setOpen}>
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
									<Pen className="size-12 stroke-white" />
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
						By proceeding, you agree to give SpotifyPool access to the image you choose to upload.
						Please make sure you have the right to upload the image.
					</p>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	)
}

export default ProfileModal
