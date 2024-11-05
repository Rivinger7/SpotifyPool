import { HelmetProvider } from "react-helmet-async"
import { createBrowserRouter, RouterProvider } from "react-router-dom"

import AppLayout from "./pages/AppLayout"
import LoginSceen from "./pages/LoginScreen"
import SignupScreen from "./pages/SignupScreen"
import SearchScreen from "./pages/SearchScreen"
import HomeScreen from "./pages/Home/HomeScreen"
import ProfileScreen from "./pages/Profile/ProfileScreen"
import ConfirmEmailScreen from "./pages/ConfirmEmailScreen"

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
				path: "/user",
				element: <ProfileScreen />,
			},
		],
	},
	{
		path: "/login",
		element: <LoginSceen />,
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
	return (
		<HelmetProvider>
			<RouterProvider router={router} />
		</HelmetProvider>
	)
}

export default App
