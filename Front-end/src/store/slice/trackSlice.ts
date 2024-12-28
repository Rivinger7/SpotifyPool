import { Track } from "@/types"
import { createSlice, PayloadAction } from "@reduxjs/toolkit"

interface TrackState {
	track: Track | null
}

const initialState: TrackState = {
	track: null,
}

const trackSlice = createSlice({
	name: "track",
	initialState,
	reducers: {
		setTrack: (state, action: PayloadAction<TrackState>) => {
			state.track = action.payload.track
		},
	},
})

export const { setTrack } = trackSlice.actions
export default trackSlice.reducer
