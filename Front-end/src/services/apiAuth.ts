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
	}),
})

export const { useLoginMutation, useRegisterMutation } = authApi
