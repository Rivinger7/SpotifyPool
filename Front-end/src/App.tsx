import { createBrowserRouter, RouterProvider } from "react-router-dom"

import AppLayout from "@/pages/AppLayout"
import HomeScreen from "@/pages/HomeScreen"
import LoginScreen from "@/pages/LoginScreen"
import SignupScreen from "@/pages/SignupScreen"
import SearchScreen from "@/pages/SearchScreen"
import ProfileScreen from "@/pages/ProfileScreen"
import PlaylistScreen from "@/pages/PlaylistScreen"
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
			{
				path: "/playlist/:playlistId",
				element: <PlaylistScreen />,
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
