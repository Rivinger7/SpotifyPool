import { apiSlice } from "../apis/apiSlice"

export const trackApi = apiSlice.injectEndpoints({
	endpoints: (build) => ({
		getTracks: build.query({
			query: (params) => ({
				url: "/tracks",
				method: "GET",
				params,
			}),
			transformResponse: (response) => response,
			providesTags: ["Track"],
		}),
		getTracksById: build.query({
			query: () => ({
				url: "/tracks/:trackId",
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Track"],
		}),
		searchTracks: build.query({
			query: () => ({
				url: "/tracks/search",
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Track"],
		}),
		getTopTracks: build.query({
			query: () => ({
				url: "/tracks/top-track",
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Track"],
		}),
	}),
})

export const {
	useGetTracksQuery,
	useGetTracksByIdQuery,
	useSearchTracksQuery,
	useGetTopTracksQuery,
} = trackApi
