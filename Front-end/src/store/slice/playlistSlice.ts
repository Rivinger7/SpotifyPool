import { Playlist, TrackPlaylist } from "@/types"
import { createSlice, PayloadAction } from "@reduxjs/toolkit"

interface PlaylistState {
	playlists: Playlist[]
	playlistTracks: TrackPlaylist[]
}

const initialState: PlaylistState = {
	playlists: [],
	playlistTracks: [],
}

const playlistSlice = createSlice({
	name: "playlists",
	initialState,
	reducers: {
		setPlaylist: (state, action: PayloadAction<Playlist[]>) => {
			state.playlists = action.payload
		},
		appendPlaylist: (state, action: PayloadAction<Playlist>) => {
			state.playlists.push(action.payload)
		},
		deletePlaylist: (state, action: PayloadAction<string>) => {
			state.playlists = state.playlists.filter((playlist) => playlist.id !== action.payload)
		},
		resetPlaylist(state) {
			state.playlists = [] // Clear playlists on logout
		},
		setPlaylistTracks: (state, action: PayloadAction<TrackPlaylist[]>) => {
			state.playlistTracks = action.payload
		},
		appendPlaylistTracks: (state, action: PayloadAction<TrackPlaylist>) => {
			state.playlistTracks.push(action.payload)
		},
	},
})

export const {
	setPlaylist,
	appendPlaylist,
	deletePlaylist,
	resetPlaylist,
	setPlaylistTracks,
	appendPlaylistTracks,
} = playlistSlice.actions
export default playlistSlice.reducer
