import { createSlice } from "@reduxjs/toolkit"

const uiSlice = createSlice({
	name: "ui",
	initialState: {
		isCollapsed: false,
		isPlayingView: false,
	},
	reducers: {
		toggleCollapse: (state) => {
			state.isCollapsed = !state.isCollapsed
		},
		resetCollapse: (state) => {
			state.isCollapsed = false
		},
		togglePlayingView: (state) => {
			state.isPlayingView = !state.isPlayingView
		},
		resetPlayingView: (state) => {
			state.isPlayingView = false
		},
	},
})

export const { toggleCollapse, resetCollapse, togglePlayingView, resetPlayingView } =
	uiSlice.actions
export default uiSlice.reducer
