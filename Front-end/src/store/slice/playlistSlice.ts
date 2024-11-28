import { Playlist } from "@/types"
import { createSlice, PayloadAction } from "@reduxjs/toolkit"

interface PlaylistState {
	playlists: Playlist[]
}

const initialState: PlaylistState = {
	playlists: [],
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
		resetPlaylist(state) {
			state.playlists = [] // Clear playlists on logout
		},
	},
})

export const { setPlaylist, appendPlaylist, resetPlaylist } = playlistSlice.actions
export default playlistSlice.reducer
