import { Track, TrackPlaylist } from "@/types"
import { createSlice, PayloadAction } from "@reduxjs/toolkit"

// Define interface for the payload
interface PlayPlaylistPayload {
	tracks: TrackPlaylist[]
	startIndex?: number // Optional with default 0
	playlistId?: string
}

interface SetCurrentTrackPayload {
	track: Track
	tracks?: Track[]
}

interface PlayerStore {
	currentTrack: Track | null
	isPlaying: boolean
	queue: Track[] | TrackPlaylist[]
	currentIndex: number
	currentTime: number
	playlistId?: string
}

const currentLocalTrackString = localStorage.getItem("spotifypool-current-track")
const currentLocalTrack =
	currentLocalTrackString && currentLocalTrackString !== "undefined"
		? (JSON.parse(currentLocalTrackString) as Track)
		: null

const currentLocalTime = localStorage.getItem("spotifypool-current-time")
	? localStorage.getItem("spotifypool-current-time")
	: 0

const currentLocalQueueString = localStorage.getItem("spotifypool-current-queue")
const currentLocalQueue =
	currentLocalQueueString && currentLocalQueueString !== "undefined"
		? (JSON.parse(currentLocalQueueString) as Track[])
		: []

const currentLocalIndex = localStorage.getItem("spotifypool-current-index")
const currentLocalPlaylistId = JSON.parse(
	localStorage.getItem("spotifypool-current-playlist") as string
)

const initialState: PlayerStore = {
	currentTrack: currentLocalTrack ?? null,
	isPlaying: false,
	queue: currentLocalQueue ?? [],
	currentIndex: Number(currentLocalIndex) || -1,
	currentTime: Number(currentLocalTime) || 0,
	playlistId: currentLocalPlaylistId || undefined,
}

const PlayerSlice = createSlice({
	name: "player",
	initialState,
	reducers: {
		/* initializeQueue: (state, action) => {
			const tracks: Track[] = action.payload
			const currentLocalTrackString = localStorage.getItem("spotifypool-current-track")
			const currentLocalTrack =
				currentLocalTrackString && currentLocalTrackString !== "undefined"
					? (JSON.parse(currentLocalTrackString) as Track)
					: null
			const currentLocalTime = localStorage.getItem("spotifypool-current-time")
				? localStorage.getItem("spotifypool-current-time")
				: 0
			const currentLocalQueueString = localStorage.getItem("spotifypool-current-queue")
			const currentLocalQueue =
				currentLocalQueueString && currentLocalQueueString !== "undefined"
					? (JSON.parse(currentLocalQueueString) as Track[])
					: []

			state.queue = currentLocalQueue || tracks
			state.currentTime = Number(currentLocalTime)
			state.currentTrack = currentLocalTrack || state.currentTrack || tracks[0]
			state.currentIndex = state.currentIndex === -1 ? 0 : state.currentIndex
		}, */
		setCurrentTrack: (state, action: PayloadAction<SetCurrentTrackPayload>) => {
			const { track, tracks } = action.payload

			if (!track) return

			const songIndex = state.queue.findIndex((tr) => tr.id === track.id)

			state.isPlaying = true
			state.currentTrack = track
			state.currentIndex = songIndex !== -1 ? songIndex : state.currentIndex

			localStorage.setItem("spotifypool-current-time", "0")
			localStorage.removeItem("spotifypool-current-playlist")
			localStorage.setItem("spotifypool-current-track", JSON.stringify(track))
			localStorage.setItem("spotifypool-current-queue", JSON.stringify(tracks))
			localStorage.setItem("spotifypool-current-index", state.currentIndex.toString())
			state.currentTime = 0
			state.playlistId = undefined
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

			localStorage.setItem("spotifypool-current-track", JSON.stringify(state.currentTrack))
			localStorage.setItem("spotifypool-current-time", "0")
			state.currentTime = 0
		},
		playPrevious: (state) => {
			if (state.currentIndex > 0) {
				state.currentIndex--
				state.currentTrack = state.queue[state.currentIndex]
			} else {
				state.currentIndex = state.queue.length - 1
				state.currentTrack = state.queue[state.currentIndex]
			}

			localStorage.setItem("spotifypool-current-track", JSON.stringify(state.currentTrack))
			localStorage.setItem("spotifypool-current-time", "0")
			state.currentTime = 0
		},
		playPlaylist: (state, action: PayloadAction<PlayPlaylistPayload>) => {
			if (!action.payload.tracks || action.payload.tracks.length === 0) return

			const { tracks, startIndex, playlistId } = action.payload

			const track = tracks[startIndex || 0]

			state.queue = tracks
			state.isPlaying = true
			state.currentTrack = track
			state.currentIndex = startIndex || 0
			state.playlistId = playlistId || undefined

			localStorage.setItem("spotifypool-current-playlist", JSON.stringify(playlistId))
			localStorage.setItem("spotifypool-current-queue", JSON.stringify(tracks))
			localStorage.setItem("spotifypool-current-track", JSON.stringify(track))
			localStorage.setItem("spotifypool-current-time", "0")
			state.currentTime = 0
		},
		updateCurrentTime: (state, action: PayloadAction<number>) => {
			// state.currentTime = action.payload
			localStorage.setItem("spotifypool-current-time", Math.round(action.payload).toString())
		},
	},
})

export const {
	// initializeQueue,
	setCurrentTrack,
	togglePlay,
	playNext,
	playPrevious,
	playPlaylist,
	updateCurrentTime,
} = PlayerSlice.actions
export default PlayerSlice.reducer
