import { Helmet } from "react-helmet";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import {
	Form,
	FormControl,
	FormField,
	FormItem,
	FormLabel,
	FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Link } from "react-router-dom";
import { Switch } from "@/components/ui/switch";
import axios from "axios";
import { useNavigate } from "react-router-dom";

const formSchema = z.object({
	username: z.string(),
	password: z.string().min(3, {
		message: "Your password must be at least 3 characters long.",
	}),
});

const Login = () => {
	const navigate = useNavigate();
	// 1. Define your form.
	const form = useForm<z.infer<typeof formSchema>>({
		resolver: zodResolver(formSchema),
		defaultValues: {
			username: "",
			password: "",
		},
	});

	// 2. Define a submit handler.
	function onSubmit(values: z.infer<typeof formSchema>) {
		// Do something with the form values.
		// ✅ This will be type-safe and validated.
		axios
			.post(
				"https://localhost:7018/api/authentication/login",
				{
					username: values.username,
					password: values.password,
				},
				{
					headers: {
						"Content-Type": "application/json",
					},
				}
			)
			.then((res) => {
				console.log(res.data);
				navigate("/");
			})
			.catch((err) => {
				console.log(err);
			});
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
						<img
							src="/Spotify_Icon_RGB_White.png"
							alt="spotify logo black"
							className="w-10 h-10"
						/>
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
												className="border-[#727272] rounded-sm transition-all duration-300 hover:border-[#fff]"
												placeholder="Password"
												{...field}
											/>
										</FormControl>
										<FormMessage />
									</FormItem>
								)}
							/>
							<div className="flex items-center space-x-2">
								<FormControl>
									<Switch id="airplane-mode" className="checked:bg-[#1ed760]" />
								</FormControl>
								<FormLabel htmlFor="airplane-mode">Remember me</FormLabel>
							</div>
							<Button
								className="rounded-full bg-[#1ed760] w-full hover:bg-[#1fdf64] font-bold"
								type="submit"
							>
								Log In
							</Button>
						</form>
					</Form>
					<div className="mt-8 text-center">
						<Link
							to={"/"}
							className="underline hover:text-[#1ed760] transition-all duration-300"
						>
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
						<svg
							width={25}
							height={24}
							viewBox="0 0 25 24"
							fill="none"
							xmlns="http://www.w3.org/2000/svg"
							className="mr-6"
						>
							<path
								fillRule="evenodd"
								clipRule="evenodd"
								d="M22.1 12.2272C22.1 11.5182 22.0364 10.8363 21.9182 10.1818H12.5V14.05H17.8818C17.65 15.3 16.9455 16.3591 15.8864 17.0682V19.5772H19.1182C21.0091 17.8363 22.1 15.2727 22.1 12.2272Z"
								fill="#4285F4"
							/>
							<path
								fillRule="evenodd"
								clipRule="evenodd"
								d="M12.4998 21.9999C15.1998 21.9999 17.4635 21.1045 19.118 19.5772L15.8862 17.0681C14.9907 17.6681 13.8453 18.0227 12.4998 18.0227C9.89529 18.0227 7.69075 16.2636 6.90439 13.8999H3.56348V16.4908C5.20893 19.759 8.59075 21.9999 12.4998 21.9999Z"
								fill="#34A853"
							/>
							<path
								fillRule="evenodd"
								clipRule="evenodd"
								d="M6.90455 13.9C6.70455 13.3 6.59091 12.6591 6.59091 12C6.59091 11.3409 6.70455 10.7 6.90455 10.1V7.50909H3.56364C2.88636 8.85909 2.5 10.3864 2.5 12C2.5 13.6136 2.88636 15.1409 3.56364 16.4909L6.90455 13.9Z"
								fill="#FBBC05"
							/>
							<path
								fillRule="evenodd"
								clipRule="evenodd"
								d="M12.4998 5.97727C13.968 5.97727 15.2862 6.48182 16.3226 7.47273L19.1907 4.60455C17.4589 2.99091 15.1953 2 12.4998 2C8.59075 2 5.20893 4.24091 3.56348 7.50909L6.90439 10.1C7.69075 7.73636 9.89529 5.97727 12.4998 5.97727Z"
								fill="#EA4335"
							/>
						</svg>
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
	);
};

export default Login;