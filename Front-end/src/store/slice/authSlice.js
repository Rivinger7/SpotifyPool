const { createSlice } = require("@reduxjs/toolkit");

const userData = JSON.parse(localStorage.getItem("userData")) || null;
const userToken = JSON.parse(localStorage.getItem("userToken")) || null;

initialState = {
	userData,
	userToken,
	isAuth: false,
	isLoading: false,
};

const authSlice = createSlice({
	name: "auth",
	initialState,
	reducers: {
		login: (state, action) => {
			state.userData = action.payload.userData;
			state.userToken = action.payload.userToken;
			state.isAuth = true;
		},
		logout: (state) => {
			state.userData = {};
			state.userToken = "";
			state.isAuth = false;
		},
	},
});

export const { login, logout } = authSlice.actions;

export default authSlice.reducer;
