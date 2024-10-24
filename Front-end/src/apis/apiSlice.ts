import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"

const baseQuery = fetchBaseQuery({
	baseUrl: import.meta.env.VITE_API_ENDPOINT + "/api",
	prepareHeaders: (headers, { getState }) => {
		headers.set("content-type", "application/json")
		if (!headers.has("Authorization")) {
			const token = getState().auth?.userToken?.accessToken
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
