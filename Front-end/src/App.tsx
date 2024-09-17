import { createBrowserRouter, RouterProvider } from "react-router-dom";
import Home from "@/pages/Home/Home";
import Login from "./pages/Login/Login";
import Signup from "./pages/Signup/Signup";
import AppLayout from "./pages/AppLayout/AppLayout";
import { HelmetProvider } from "react-helmet-async";
import SearchPage from "./pages/Search/SearchPage";
import Profile from "./pages/Profile/Profile";

const router = createBrowserRouter([
	{
		element: <AppLayout />,
		children: [
			{
				path: "/",
				element: <Home />,
			},
			{
				path: "/search",
				element: <SearchPage />,
			},
			{
				path: "/user",
				element: <Profile />,
			},
		],
	},
	{
		path: "/login",
		element: <Login />,
	},
	{
		path: "/signup",
		element: <Signup />,
	},
]);

function App() {
	return (
		<HelmetProvider>
			<RouterProvider router={router} />
		</HelmetProvider>
	);
}

export default App;
