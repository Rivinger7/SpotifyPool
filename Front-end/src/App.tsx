import { createBrowserRouter, RouterProvider } from "react-router-dom"

import AppLayout from "@/pages/AppLayout"
import LoginScreen from "@/pages/LoginScreen"
import SignupScreen from "@/pages/SignupScreen"
import SearchScreen from "@/pages/SearchScreen"
import HomeScreen from "@/pages/HomeScreen"
import ProfileScreen from "@/pages/ProfileScreen"
import ConfirmEmailScreen from "@/pages/ConfirmEmailScreen"

const router = createBrowserRouter([
	{
		element: <AppLayout />,
		children: [
			{
				path: "/",
				element: <HomeScreen />,
			},
			{
				path: "/search",
				element: <SearchScreen />,
			},
			{
				path: "/user/:userId",
				element: <ProfileScreen />,
			},
		],
	},
	{
		path: "/login",
		element: <LoginScreen />,
	},
	{
		path: "/signup",
		element: <SignupScreen />,
	},
	{
		path: "/spotifypool/confirm-email",
		element: <ConfirmEmailScreen />,
	},
])

function App() {
	return <RouterProvider router={router} />
}

export default App
