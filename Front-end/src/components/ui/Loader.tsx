import "@/styles/loader.scss"

export default function Loader() {
	return (
		<div
			style={{
				width: "100%",
				height: "100%",
				display: "flex",
				justifyContent: "center",
				alignItems: "center",
			}}
		>
			{/* <div className="loader">
				<div className="dot"></div>
				<div className="dot"></div>
				<div className="dot"></div>
			</div> */}

			<div className="loader"></div>
		</div>
	)
}
