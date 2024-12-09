import { Track, TrackPlaylist } from "@/types"
import { createSlice, PayloadAction } from "@reduxjs/toolkit"

// Define interface for the payload
interface PlayPlaylistPayload {
	tracks: TrackPlaylist[]
	startIndex?: number // Optional with default 0
}

interface PlayerStore {
	currentTrack: Track | null
	isPlaying: boolean
	queue: Track[] | TrackPlaylist[]
	currentIndex: number
}

const initialState: PlayerStore = {
	currentTrack: null,
	isPlaying: false,
	queue: [],
	currentIndex: -1,
}

const PlayerSlice = createSlice({
	name: "player",
	initialState,
	reducers: {
		initializeQueue: (state, action) => {
			const tracks: Track[] = action.payload

			state.queue = tracks
			state.currentTrack = state.currentTrack || tracks[0]
			state.currentIndex = state.currentIndex === -1 ? 0 : state.currentIndex
		},
		setCurrentTrack: (state, action) => {
			const track: Track | null = action.payload

			if (!track) return

			const songIndex = state.queue.findIndex((tr) => tr.id === track.id)

			state.isPlaying = true
			state.currentTrack = track
			state.currentIndex = songIndex !== -1 ? songIndex : state.currentIndex
		},
		togglePlay: (state) => {
			state.isPlaying = !state.isPlaying
		},
		playNext: (state) => {
			if (state.currentIndex < state.queue.length - 1) {
				state.currentIndex++
				state.currentTrack = state.queue[state.currentIndex]
			} else {
				state.currentIndex = 0
				state.currentTrack = state.queue[state.currentIndex]
			}
		},
		playPrevious: (state) => {
			if (state.currentIndex > 0) {
				state.currentIndex--
				state.currentTrack = state.queue[state.currentIndex]
			} else {
				state.currentIndex = state.queue.length - 1
				state.currentTrack = state.queue[state.currentIndex]
			}
		},
		playPlaylist: (state, action: PayloadAction<PlayPlaylistPayload>) => {
			if (!action.payload.tracks || action.payload.tracks.length === 0) return

			const { tracks, startIndex } = action.payload

			const track = tracks[startIndex || 0]

			state.queue = tracks
			state.currentTrack = track
			state.currentIndex = startIndex || 0
			state.isPlaying = true
		},
	},
})

export const {
	initializeQueue,
	setCurrentTrack,
	togglePlay,
	playNext,
	playPrevious,
	playPlaylist,
} = PlayerSlice.actions
export default PlayerSlice.reducer
