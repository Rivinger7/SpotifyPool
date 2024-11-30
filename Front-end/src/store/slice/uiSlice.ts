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
	},
})

export const { toggleCollapse } = uiSlice.actions
export default uiSlice.reducer
