import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form"

import GoogleIcon from "@/assets/icons/GoogleIcon"

import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"

import { z } from "zod"
import toast from "react-hot-toast"
import { useForm } from "react-hook-form"
import { Helmet } from "react-helmet-async"
import { Link, useNavigate } from "react-router-dom"
import { zodResolver } from "@hookform/resolvers/zod"
import { useRegisterMutation } from "@/services/apiAuth"

const formSchema = z.object({
	email: z.string().email({
		message: "Please enter a valid email address.",
	}),
	userName: z.string().min(3, {
		message: "Please enter a valid username.",
	}),
	password: z.string().min(8, {
		message: "Please enter a valid password.",
	}),
	confirmedPassword: z.string().min(8, {
		message: "Please enter a valid password.",
	}),
	displayName: z.string().min(3, {
		message: "Please enter a valid display name.",
	}),
	phoneNumber: z.string().min(10, {
		message: "Please enter a valid phone number.",
	}),
})

const SignupForm = () => {
	const navigate = useNavigate()

	const [registerMutation] = useRegisterMutation()

	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			email: "",
			userName: "",
			password: "",
			confirmedPassword: "",
			displayName: "",
			phoneNumber: "",
		},
	})

	async function onSubmit(values: z.infer<typeof formSchema>) {
		try {
			const response = await registerMutation(values).unwrap()
			toast.success(response.message)
			navigate("/login")
		} catch (error) {
			console.log(error)
			toast.error("An error occurred while creating your account")
		}
	}

	return (
		<div className="flex items-center justify-center w-2/3 lg:w-2/5 min-h-full py-10 m-0 mx-auto">
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
					<form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
						{/* EMAIL */}
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

						{/* USERNAME */}
						<FormField
							control={form.control}
							name="userName"
							render={({ field }) => (
								<FormItem>
									<FormLabel>Username</FormLabel>
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
						/>

						{/* DISPLAY NAME */}
						<FormField
							control={form.control}
							name="displayName"
							render={({ field }) => (
								<FormItem>
									<FormLabel>What should we call you?</FormLabel>
									<FormControl>
										<Input
											className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
											placeholder="Display name"
											{...field}
										/>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						{/* PHONE NUMBER */}
						<FormField
							control={form.control}
							name="phoneNumber"
							render={({ field }) => (
								<FormItem>
									<FormLabel>Phone number</FormLabel>
									<FormControl>
										<Input
											className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
											placeholder="Phone number"
											{...field}
										/>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						{/* PASSWORD */}
						<FormField
							control={form.control}
							name="password"
							render={({ field }) => (
								<FormItem>
									<FormLabel>Password</FormLabel>
									<FormControl>
										<Input
											className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
											placeholder="Password"
											type="password"
											{...field}
										/>
									</FormControl>
									<FormMessage />
								</FormItem>
							)}
						/>

						{/* CONFIRM PASSWORD */}
						<FormField
							control={form.control}
							name="confirmedPassword"
							render={({ field }) => (
								<FormItem>
									<FormLabel>Confirm password</FormLabel>
									<FormControl>
										<Input
											className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
											placeholder="Confirm password"
											type="password"
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
				</div>
			</div>
		</div>
	)
}

export default SignupForm
