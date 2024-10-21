import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import { jwtDecode } from "jwt-decode"

// Interface for the decoded JWT token
interface DecodedToken {
	"http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string
	"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string
	Avatar: string
}

// Interface for the user data stored in the state
interface UserData {
	fullname: string
	role: string
	avatar: string
}

// Interface for the userToken which contains accessToken and refreshToken
interface UserToken {
	accessToken: string
	refreshToken: string
}

// Interface for the auth state
interface AuthState {
	userData: UserData | null
	userToken: UserToken | null
	isAuthenticated: boolean
	isLoading: boolean
}

// Initial state
const userData = JSON.parse(localStorage.getItem("userData") || "null") as UserData | null
const userToken = JSON.parse(localStorage.getItem("userToken") || "null") as UserToken | null

const initialState: AuthState = {
	userData,
	userToken,
	isAuthenticated: !!userData,
	isLoading: false,
}

const authSlice = createSlice({
	name: "auth",
	initialState,
	reducers: {
		login: (state, action: PayloadAction<{ userToken: UserToken }>) => {
			const { userToken } = action.payload
			const decodedToken = jwtDecode<DecodedToken>(userToken.accessToken)

			state.userData = {
				fullname: decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
				role: decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
				avatar: decodedToken.Avatar,
			}
			state.userToken = userToken
			state.isAuthenticated = true

			// Store in localStorage
			localStorage.setItem("userData", JSON.stringify(state.userData))
			localStorage.setItem("userToken", JSON.stringify(state.userToken))
		},
		logout: (state) => {
			state.isAuthenticated = false
			state.userData = null
			state.userToken = null
			localStorage.removeItem("userData")
			localStorage.removeItem("userToken")
		},
	},
})

// Export actions and reducer
export const { login, logout } = authSlice.actions
export default authSlice.reducer
