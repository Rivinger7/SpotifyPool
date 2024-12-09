import { apiSlice } from "@/apis/apiSlice.ts"

export const playlistApi = apiSlice.injectEndpoints({
	endpoints: (build) => ({
		getAllPlaylists: build.query({
			query: () => ({
				url: "/playlists",
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Playlist"],
		}),
		getPlaylist: build.query({
			query: (id) => ({
				url: `/playlists/${id}`,
				method: "GET",
			}),
			transformResponse: (response) => response,
			providesTags: ["Playlist"],
		}),
		createPlaylist: build.mutation({
			query: (playlistName) => ({
				url: `/playlists/${playlistName}`,
				method: "POST",
			}),
			invalidatesTags: ["Playlist"],
		}),
		addTrackToPlaylist: build.mutation({
			query: (playlistId) => ({
				url: `/playlists/${playlistId}/add-track`,
				method: "POST",
			}),
			invalidatesTags: ["Playlist"],
		}),
		deleteTrackFromPlaylist: build.mutation({
			query: ({ playlistID, trackID }) => ({
				url: `/playlists/${playlistID}`,
				method: "DELETE",
				params: { trackID },
			}),
			invalidatesTags: ["Playlist"],
		}),
	}),
})

export const {
	useGetAllPlaylistsQuery,
	useGetPlaylistQuery,
	useCreatePlaylistMutation,
	useAddTrackToPlaylistMutation,
	useDeleteTrackFromPlaylistMutation,
} = playlistApi
