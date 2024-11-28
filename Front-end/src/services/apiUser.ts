import {apiSlice} from "@/apis/apiSlice.ts";

export const userApi = apiSlice.injectEndpoints({
    endpoints: (build) => ({
        getUserProfile: build.query({
            query: (id) => ({
                url: `/users/${id}`,
                method: "GET"
            }),
            transformResponse: (response) => response,
            providesTags: ["User"]
        }),
        updateUserProfile: build.mutation({
            query: () => ({
                url: "/users/edit-profile",
                method: "PUT"
            }),
            invalidatesTags: ["User"]
        })
    })
})

export const {useGetUserProfileQuery, useUpdateUserProfileMutation} = userApi