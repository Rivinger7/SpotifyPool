import { HelmetProvider } from "react-helmet-async"
import { createBrowserRouter, RouterProvider } from "react-router-dom"

import HomeScreen from "./pages/Home/HomeScreen"
import LoginSceen from "./pages/Login/LoginScreen"
import AppLayout from "./pages/AppLayout/AppLayout"
import SignupScreen from "./pages/Signup/SignupScreen"
import SearchScreen from "./pages/Search/SearchScreen"
import ProfileScreen from "./pages/Profile/ProfileScreen"
import ConfirmEmail from "./pages/ConfirmEmail/ConfirmEmail"

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
		element: <ConfirmEmail />,
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
