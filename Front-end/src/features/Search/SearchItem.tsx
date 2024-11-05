const SearchItem = () => {
	return (
		<div className="h-56 p-3">
			<a href="#" className="relative">
				<div className="w-full h-full relative overflow-hidden rounded-lg bg-[#006450]">
					<img
						src="https://i.scdn.co/image/ab6765630000ba8a81f07e1ead0317ee3c285bfa"
						alt=""
						className="absolute right-[-24px] object-cover object-center bottom-[-8px] rotate-[25deg] w-[45%] shadow-[0_2px_4px_0_rgba(0,0,0,0.2)] rounded-sm translate-[18%_-2%]"
					/>
					<span className="absolute p-4 text-2xl font-bold">Podcasts</span>
				</div>
			</a>
		</div>
	)
}

export default SearchItem
