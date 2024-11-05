import { useEffect } from "react"
import { RootState } from "@/store/store"
import { useSelector } from "react-redux"
import { useNavigate } from "react-router-dom"
import SignupForm from "@/features/Auth/SignupForm"

const SignupScreen = () => {
	const navigate = useNavigate()
	const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated)

	useEffect(() => {
		if (isAuthenticated) {
			navigate("/", { replace: true })
		}
	}, [navigate, isAuthenticated])

	return <SignupForm />
}

export default SignupScreen
