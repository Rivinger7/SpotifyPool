import { createSlice } from "@reduxjs/toolkit"
import jwtDecode from "jwt-decode"

const userData = JSON.parse(localStorage.getItem("userData") || "null") || null
const userToken = JSON.parse(localStorage.getItem("userToken") || "null") || null

const initialState = {
	userData,
	userToken,
	isAuthenticated: !!userData,
	isLoading: false,
}

const authSlice = createSlice({
	name: "auth",
	initialState,
	reducers: {
		login: (state, action) => {
			const { userData, userToken } = action.payload
			// const decodedToken = jwtDecode(userToken)

			state.userData = userData
			state.userToken = userToken

			localStorage.setItem("userData", JSON.stringify(userData))
			localStorage.setItem("userToken", JSON.stringify(userToken))
			state.isAuthenticated = true
		},
		logout: (state) => {
			localStorage.removeItem("userData")
			localStorage.removeItem("userToken")
			state.isAuthenticated = false
		},
	},
})

export const { login, logout } = authSlice.actions

export default authSlice.reducer
