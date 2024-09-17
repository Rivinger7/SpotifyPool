import "@/styles/loader.scss";

export default function Loader() {
	return (
		<div
			style={{
				width: "100%",
				height: "100vh",
			}}
		>
			<div className="loader">
				<div className="dot"></div>
				<div className="dot"></div>
				<div className="dot"></div>
			</div>
		</div>
	);
}
