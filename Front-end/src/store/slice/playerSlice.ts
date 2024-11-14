import { createSlice } from "@reduxjs/toolkit"
import { Song } from "@/types"

interface PlayerStore {
	currentSong: Song | null
	isPlaying: boolean
	queue: Song[]
	currentIndex: number
}

const initialState: PlayerStore = {
	currentSong: null,
	isPlaying: false,
	queue: [],
	currentIndex: -1,
}

const PlayerSlice = createSlice({
	name: "player",
	initialState,
	reducers: {
		initializeQueue: (state, action) => {
			const songs: Song[] = action.payload

			state.queue = songs
			state.currentSong = state.currentSong || songs[0]
			state.currentIndex = state.currentIndex === -1 ? 0 : state.currentIndex
		},
		setCurrentSong: (state, action) => {
			const song: Song | null = action.payload

			if (!song) return

			const songIndex = state.queue.findIndex((s) => s.id === song.id)

			state.isPlaying = true
			state.currentSong = song
			state.currentIndex = songIndex !== -1 ? songIndex : state.currentIndex
		},
		togglePlay: (state) => {
			state.isPlaying = !state.isPlaying
		},
		playNext: (state) => {
			if (state.currentIndex < state.queue.length - 1) {
				state.currentIndex++
				state.currentSong = state.queue[state.currentIndex]
			} else {
				state.currentIndex = 0
				state.currentSong = state.queue[state.currentIndex]
			}
		},
		playPrevious: (state) => {
			if (state.currentIndex > 0) {
				state.currentIndex--
				state.currentSong = state.queue[state.currentIndex]
			} else {
				state.currentIndex = state.queue.length - 1
				state.currentSong = state.queue[state.currentIndex]
			}
		},
	},
})

export const { initializeQueue, setCurrentSong, togglePlay, playNext, playPrevious } =
	PlayerSlice.actions
export default PlayerSlice.reducer
