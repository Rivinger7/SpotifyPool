import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"

// Token structure
interface UserToken {
	accessToken: string
}
// Auth state structure
interface AuthState {
	userToken: UserToken
}
// Root state structure
interface RootState {
	auth: AuthState
}

const baseQuery = fetchBaseQuery({
	baseUrl: import.meta.env.VITE_API_ENDPOINT + "/api",
	prepareHeaders: (headers, { getState }) => {
		headers.set("content-type", "application/json")
		if (!headers.has("Authorization")) {
			const token = (getState() as RootState).auth?.userToken?.accessToken
			if (token) {
				headers.set("Authorization", `Bearer ${token}`)
			}
		}
		return headers
	},
})

export const apiSlice = createApi({
	reducerPath: "api",
	baseQuery: baseQuery,
	tagTypes: ["Auth", "Media"],
	endpoints: () => ({}),
})
