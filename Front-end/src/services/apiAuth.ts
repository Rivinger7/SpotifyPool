import { apiSlice } from "../apis/apiSlice"

export const authApi = apiSlice.injectEndpoints({
	endpoints: (build) => ({
		login: build.mutation({
			query: (data) => ({
				url: "/authentication/login",
				method: "POST",
				body: data,
			}),
			invalidatesTags: ["Auth"],
		}),
		register: build.mutation({
			query: (data) => ({
				url: "/authentication/register",
				method: "POST",
				body: data,
			}),
			invalidatesTags: ["Auth"],
		}),
		emailConfirm: build.mutation({
			query: (token) => ({
				url: "/authentication/confirm-email",
				method: "POST",
				body: JSON.stringify(token),
			}),
			invalidatesTags: ["Auth"],
		}),
		getGoogleResponse: build.query({
			query: () => ({
				url: "/authentication/google-response",
				method: "GET",
			}),
			transformResponse: (res) => res,
			providesTags: ["Auth"],
		}),
	}),
})

export const {
	useLoginMutation,
	useRegisterMutation,
	useEmailConfirmMutation,
	useGetGoogleResponseQuery,
} = authApi
