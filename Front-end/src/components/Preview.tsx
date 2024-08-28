const Preview = () => {
	return (
		<div className={"now-playing-bar-container"}>
			<footer>
				<div className="now-playing-bar-style">
					<div>
						<p className="text-[14px] font-bold">
							Preview of Spotify
						</p>
						<p className="font-medium">
							Sign up to get unlimited songs and podcasts with
							occasional ads. No credit card needed.
						</p>
					</div>
					<button className="bg-transparent border-0 cursor-pointer text-center touch-manipulation transition-all align-middle rounded-full">
						<span className="bg-white text-black flex items-center justify-center rounded-full font-bold p-2 pl-8 pr-8 min-h-12">
							Sign up free
						</span>
					</button>
				</div>
			</footer>
		</div>
	);
};

export default Preview;
