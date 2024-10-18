import { configureStore } from "@reduxjs/toolkit"
import authReducer from "./slice/authSlice"
import { apiSlice } from "@/apis/apiSlice"

const rootReducer = {
	auth: authReducer,
}

const store = configureStore({
	reducer: {
		[apiSlice.reducerPath]: apiSlice.reducer,

		...rootReducer,
	},

	middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(apiSlice.middleware),
})

export default store
