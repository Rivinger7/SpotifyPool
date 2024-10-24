import { useNavigate } from "react-router-dom"
import SignupForm from "./Form/SignupForm"
import { useSelector } from "react-redux"
import { RootState } from "@/store/store"
import { useEffect } from "react"

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
