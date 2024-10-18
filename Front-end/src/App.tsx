import { createBrowserRouter, RouterProvider } from "react-router-dom"
import AppLayout from "./pages/AppLayout/AppLayout"
import { HelmetProvider } from "react-helmet-async"
import HomeScreen from "./pages/Home/HomeScreen"
import LoginSceen from "./pages/Login/LoginScreen"
import ProfileScreen from "./pages/Profile/ProfileScreen"
import SearchScreen from "./pages/Search/SearchScreen"
import SignupScreen from "./pages/Signup/SignupScreen"

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
])

function App() {
	return (
		<HelmetProvider>
			<RouterProvider router={router} />
		</HelmetProvider>
	)
}

export default App
