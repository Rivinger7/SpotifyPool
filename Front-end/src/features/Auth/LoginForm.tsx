import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form"

import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"

import { z } from "zod"
import toast from "react-hot-toast"
import { Link } from "react-router-dom"
import { useDispatch } from "react-redux"
import { useForm } from "react-hook-form"
import { Helmet } from "react-helmet-async"
import { useNavigate } from "react-router-dom"
import { login } from "@/store/slice/authSlice"
import { zodResolver } from "@hookform/resolvers/zod"
import { useLoginByGoogleMutation, useLoginMutation } from "@/services/apiAuth"

import { GoogleLogin } from "@react-oauth/google"

const formSchema = z.object({
	username: z.string(),
	password: z.string().min(3, {
		message: "Your password must be at least 3 characters long.",
	}),
	remember: z.boolean(),
})

const LoginForm = () => {
	const navigate = useNavigate()
	const dispatch = useDispatch()

	const [loginMutation] = useLoginMutation()
	const [loginByGoogleMutation] = useLoginByGoogleMutation()

	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			username: "",
			password: "",
			remember: false,
		},
	})

	function onSubmit(values: z.infer<typeof formSchema>) {
		loginMutation({ username: values.username, password: values.password })
			.unwrap()
			.then((data) => {
				console.log(data)
				dispatch(
					login({
						userToken: data.authenticatedResponseModel,
						isGoogle: false,
					})
				)
				navigate("/")
				toast.success("Login successful")
			})
			.catch((error) => {
				console.error(error)
			})
	}

	return (
		<div className="min-h-full flex items-center justify-center bg-gradient-to-b from-zinc-700 from-0% to-black to-100%">
			<div className="flex items-center justify-center w-4/5 md:w-2/3 lg:w-1/2 h-full m-0 mx-auto">
				<div className="bg-[#121212] py-10 px-14 rounded-md">
					<Helmet>
						<link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Black.png" />
						<title>Login - Spotify</title>
					</Helmet>
					<header className="flex flex-col items-center justify-center mb-3">
						<img src="/Spotify_Icon_RGB_White.png" alt="spotify logo black" className="w-10 h-10" />
						<h1 className="text-2xl md:text-3xl lg:text-5xl leading-[62px] text-center font-bold text-white">
							Log in to Spotify
						</h1>
					</header>
					<Form {...form}>
						<form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
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
					<div className="mt-3 text-center">
						<Link to={"/"} className="underline hover:text-[#1ed760] transition-all duration-300">
							Forgot your password?
						</Link>
					</div>
					<div className="flex justify-center items-center mt-3 relative before:absolute before:left-0 before:right-0 before:block before:top-1/2 before:h-[1px] before:content-[''] before:w-full before:border-[1px] before:border-solid before:border-[#727272]">
						<span className="relative bg-[#121212] pl-3 pr-3 text-sm leading-5 text-[rgb(107 114 128 / 1)]">
							or
						</span>
					</div>
					<div
						className="mt-3"
						// className="rounded-full bg-transparent transition-all duration-300 p-2 pl-8 pr-8 w-full mt-3 border-[1px] border-solid border-[#727272] hover:bg-transparent hover:border-[#fff] text-white font-bold"
						// type="submit"
					>
						<GoogleLogin
							shape="pill"
							size="large"
							onSuccess={(credentialResponse) => {
								loginByGoogleMutation({ googleToken: credentialResponse.credential })
									.unwrap()
									.then((data) => {
										dispatch(login({ userToken: data.token, isGoogle: true }))
										navigate("/")
										toast.success("Login successful")
									})
									.catch((error) => {
										console.error(error)
									})
							}}
							onError={() => {
								console.log("Login Failed")
							}}
						/>
					</div>

					<div className="text-center mt-3 w-full text-[#a7a7a7]">
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

export default LoginForm
