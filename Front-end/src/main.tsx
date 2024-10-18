import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import { Toaster } from "react-hot-toast"
import App from "./App.tsx"
import "./index.css"

import store from "./store/store.ts"
import { Provider } from "react-redux"

createRoot(document.getElementById("root")!).render(
	<StrictMode>
		<Provider store={store}>
			<Toaster />
			<App />
		</Provider>
	</StrictMode>
)
