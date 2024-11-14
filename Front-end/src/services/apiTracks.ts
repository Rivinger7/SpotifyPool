import { apiSlice } from "../apis/apiSlice"

export const trackApi = apiSlice.injectEndpoints({
	endpoints: (build) => ({
		getTracks: build.query({
			query: () => ({
				url: "/tracks",
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Media"],
		}),
		uploadImage: build.mutation({
			query: (data) => ({
				url: "/media/upload-image",
				method: "POST",
				body: data,
			}),
			invalidatesTags: ["Media"],
		}),
		uploadTrack: build.mutation({
			query: (data) => ({
				url: "/media/upload-track",
				method: "POST",
				body: data,
			}),
			invalidatesTags: ["Media"],
		}),
	}),
})

export const { useGetTracksQuery, useUploadImageMutation, useUploadTrackMutation } = trackApi
