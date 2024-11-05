import { useEffect } from "react"
import { useSelector } from "react-redux"
import { RootState } from "@/store/store"
import { useNavigate } from "react-router-dom"
import LoginForm from "@/features/Auth/LoginForm"

const LoginSceen = () => {
	const navigate = useNavigate()
	const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated)

	useEffect(() => {
		if (isAuthenticated) {
			navigate("/", { replace: true })
		}
	}, [navigate, isAuthenticated])

	return <LoginForm />
}

export default LoginSceen
