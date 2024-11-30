import { createSlice } from "@reduxjs/toolkit"
import { Track } from "@/types"

interface PlayerStore {
	currentTrack: Track | null
	isPlaying: boolean
	queue: Track[]
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
			const songs: Track[] = action.payload

			state.queue = songs
			state.currentTrack = state.currentTrack || songs[0]
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
	},
})

export const { initializeQueue, setCurrentTrack, togglePlay, playNext, playPrevious } =
	PlayerSlice.actions
export default PlayerSlice.reducer
