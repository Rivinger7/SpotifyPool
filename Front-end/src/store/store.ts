import {configureStore} from "@reduxjs/toolkit"
import {apiSlice} from "@/apis/apiSlice"
import authReducer from "./slice/authSlice"
import playerReducer from "./slice/playerSlice"
import playlistReducer from "./slice/playlistSlice"

const rootReducer = {
    auth: authReducer,
    play: playerReducer,
    playlist: playlistReducer
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
