import { Helmet } from "react-helmet-async"
import { z } from "zod"
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { Button } from "@/components/ui/button"
import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { Link } from "react-router-dom"
import GoogleIcon from "@/assets/icons/GoogleIcon"

const formSchema = z.object({
	email: z.string().email({
		message: "Please enter a valid email address.",
	}),
})

const SignupForm = () => {
	// 1. Define your form.
	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			email: "",
		},
	})

	// 2. Define a submit handler.
	function onSubmit(values: z.infer<typeof formSchema>) {
		// Do something with the form values.
		// ✅ This will be type-safe and validated.
		console.log(values)
	}

	return (
		<div className="flex items-center w-1/5 h-full m-0 mx-auto">
			<div>
				<Helmet>
					<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Black.png" />
					<title>Sign up - Spotify</title>
				</Helmet>
				<header className="flex flex-col items-center justify-center mb-8">
					<img src="/Spotify_Icon_RGB_White.png" alt="spotify logo black" className="w-10 h-10" />
					<h1 className="text-5xl leading-[62px]  text-center font-bold text-white">
						Sign up to start listening
					</h1>
				</header>
				<Form {...form}>
					<form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
						<FormField
							control={form.control}
							name="email"
							render={({ field }) => (
								<FormItem>
									<FormLabel>Email address</FormLabel>
									<FormControl>
										<Input
											className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
											placeholder="name@domain.com"
											{...field}
										/>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>
						<Button
							className="rounded-full bg-[#1ed760] w-full hover:bg-[#1fdf64] font-bold"
							type="submit"
						>
							Next
						</Button>
					</form>
				</Form>
				<div className="flex justify-center items-center mt-8 relative before:absolute before:left-0 before:right-0 before:block before:top-1/2 before:h-[1px] before:content-[''] before:w-full before:border-[1px] before:border-solid before:border-[#727272]">
					<span className="relative bg-[#121212] pl-3 pr-3 text-sm leading-5 text-[rgb(107 114 128 / 1)]">
						or
					</span>
				</div>
				<Button
					className="rounded-full bg-transparent transition-all duration-300 p-2 pl-8 pr-8 w-full mt-8 border-[1px] border-solid border-[#727272] hover:bg-transparent hover:border-[#fff] text-white font-bold"
					type="submit"
				>
					<GoogleIcon />
					Sign up with Google
				</Button>
				<div className="h-[1px] bg-[#292929] w-full mt-8 mb-8"></div>
				<div className="text-center w-full text-[#a7a7a7]">
					Already have an account?{" "}
					<Link
						to={"/login"}
						className="underline hover:text-[#1ed760] transition-all duration-300"
					>
						Log in here
					</Link>
					.
				</div>
			</div>
		</div>
	)
}

export default SignupForm
