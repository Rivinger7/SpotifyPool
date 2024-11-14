import { configureStore } from "@reduxjs/toolkit"
import authReducer from "./slice/authSlice"
import { apiSlice } from "@/apis/apiSlice"
import playerReducer from "./slice/playerSlice"

const rootReducer = {
	auth: authReducer,
	play: playerReducer,
}

const store = configureStore({
	reducer: {
		[apiSlice.reducerPath]: apiSlice.reducer,

		...rootReducer,
	},

	middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(apiSlice.middleware),
})

// Define RootState type
export type RootState = ReturnType<typeof store.getState>
export default store
