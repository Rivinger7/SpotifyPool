import { createSlice } from "@reduxjs/toolkit"

const uiSlice = createSlice({
	name: "ui",
	initialState: {
		isCollapsed: false, // default state
	},
	reducers: {
		toggleCollapse: (state) => {
			state.isCollapsed = !state.isCollapsed
		},
		resetCollapse: (state) => {
			state.isCollapsed = false
		},
	},
})

export const { toggleCollapse, resetCollapse } = uiSlice.actions
export default uiSlice.reducer
