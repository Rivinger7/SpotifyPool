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
import { Switch } from "@/components/ui/switch"
import { useNavigate } from "react-router-dom"
import GoogleIcon from "@/assets/icons/GoogleIcon"

import { login } from "@/store/slice/authSlice"
import { useLoginMutation } from "@/services/apiAuth"
import { useDispatch } from "react-redux"
import toast from "react-hot-toast"

const formSchema = z.object({
	username: z.string(),
	password: z.string().min(3, {
		message: "Your password must be at least 3 characters long.",
	}),
	remember: z.boolean(),
})

export default function LoginForm() {
	const navigate = useNavigate()
	const dispatch = useDispatch()

	const [loginMutation] = useLoginMutation()

	// 1. Define your form.
	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			username: "",
			password: "",
			remember: false,
		},
	})

	// 2. Define a submit handler.
	function onSubmit(values: z.infer<typeof formSchema>) {
		loginMutation({ username: values.username, password: values.password })
			.unwrap()
			.then((data) => {
				console.log(data)
				dispatch(
					login({
						userData: { username: values.username },
						userToken: data.authenticatedResponseModel,
					})
				)
				navigate("/")
				toast.success("Login successful")
			})
			.catch((error) => {
				console.error(error)
			})
		// console.log({
		// 	userData: { username: values.username, password: values.password, remem: values.remember },
		// })
	}

	return (
		<div className="h-full bg-gradient-to-b from-zinc-700 from-0% to-black to-100%">
			<div className="flex items-center justify-center w-1/2 h-full m-0 mx-auto">
				<div className="bg-[#121212] p-10 pl-[10vw] pr-[10vw] rounded-md">
					<Helmet>
						<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Black.png" />
						<title>Login - Spotify</title>
					</Helmet>
					<header className="flex flex-col items-center justify-center mb-8">
						<img src="/Spotify_Icon_RGB_White.png" alt="spotify logo black" className="w-10 h-10" />
						<h1 className="text-5xl leading-[62px] text-center font-bold text-white">
							Log in to Spotify
						</h1>
					</header>
					<Form {...form}>
						<form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
							<FormField
								control={form.control}
								name="username"
								render={({ field }) => (
									<FormItem>
										<FormLabel> username</FormLabel>
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
							<FormField
								control={form.control}
								name="password"
								render={({ field }) => (
									<FormItem>
										<FormLabel>Password</FormLabel>
										<FormControl>
											<Input
												type="password"
												className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
												placeholder="Password"
												{...field}
											/>
										</FormControl>
										<FormMessage />
									</FormItem>
								)}
							/>
							<FormField
								control={form.control}
								name="remember"
								render={({ field }) => (
									<FormItem>
										<div className="flex items-center space-x-2">
											<FormControl>
												<Switch
													id="remember"
													className="checked:bg-[#1ed760]"
													checked={field.value}
													onCheckedChange={(checked) => field.onChange(checked)}
												/>
											</FormControl>
											<FormLabel htmlFor="airplane-mode">Remember me</FormLabel>
										</div>
									</FormItem>
								)}
							/>
							<Button
								className="rounded-full bg-[#1ed760] w-full hover:bg-[#1fdf64] font-bold"
								type="submit"
							>
								Log In
							</Button>
						</form>
					</Form>
					<div className="mt-8 text-center">
						<Link to={"/"} className="underline hover:text-[#1ed760] transition-all duration-300">
							Forgot your password?
						</Link>
					</div>
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
						Don't have an account?{" "}
						<Link
							to={"/signup"}
							className="underline hover:text-[#1ed760] transition-all duration-300"
						>
							Sign up for Spotify.
						</Link>
					</div>
				</div>
			</div>
		</div>
	)
}
