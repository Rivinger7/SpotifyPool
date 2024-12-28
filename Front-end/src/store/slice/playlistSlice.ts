import {Playlist, PlaylistDetail, TrackPlaylist} from "@/types"
import {createSlice, PayloadAction} from "@reduxjs/toolkit"

interface PlaylistState {
    playlists: Playlist[]
    playlistDetail: PlaylistDetail | null
    currentPlaylist: Playlist | null
}

const initialState: PlaylistState = {
    playlists: [],
    playlistDetail: null,
    currentPlaylist: null,
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
            state.playlists = []
        },
        setPlaylistDetail: (state, action: PayloadAction<PlaylistDetail>) => {
            state.playlistDetail = action.payload
        },
        appendPlaylistTracks: (state, action: PayloadAction<TrackPlaylist>) => {
            state.playlistDetail?.tracks.push(action.payload)
        },
    },
})

export const {
    setPlaylist,
    appendPlaylist,
    deletePlaylist,
    resetPlaylist,
    setPlaylistDetail,
    appendPlaylistTracks,
} = playlistSlice.actions
export default playlistSlice.reducer
