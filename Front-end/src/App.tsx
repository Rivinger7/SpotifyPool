import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import AppLayout from './pages/AppLayout/AppLayout'
import { HelmetProvider } from 'react-helmet-async'
import HomeScreen from './pages/Home/HomeScreen'
import LoginSceen from './pages/Login/LoginScreen'
import ProfileScreen from './pages/Profile/ProfileScreen'
import SearchScreen from './pages/Search/SearchScreen'
import SignupScreen from './pages/Signup/SignupScreen'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'

const queryClient = new QueryClient({
	defaultOptions: {
		queries: {
			staleTime: 0,
		},
	},
})

const router = createBrowserRouter([
	{
		element: <AppLayout />,
		children: [
			{
				path: '/',
				element: <HomeScreen />,
			},
			{
				path: '/search',
				element: <SearchScreen />,
			},
			{
				path: '/user',
				element: <ProfileScreen />,
			},
		],
	},
	{
		path: '/login',
		element: <LoginSceen />,
	},
	{
		path: '/signup',
		element: <SignupScreen />,
	},
])

function App() {
	return (
		<QueryClientProvider client={queryClient}>
			<HelmetProvider>
				<RouterProvider router={router} />
				<ReactQueryDevtools initialIsOpen={false} buttonPosition='bottom-left' />
			</HelmetProvider>
		</QueryClientProvider>
	)
}

export default App
