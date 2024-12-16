import TopTracksTable from "./components/Table/TopTracksTable"

const ProfileTopTracks = () => {
	return (
		<>
			<div className="flex flex-col px-6">
				<h3 className="font-bold text-2xl">Top tracks this month</h3>
				<div className="flex justify-between">
					<span className="text-[#b3b3b3]">Only visible to you</span>
					<span className="text-[#b3b3b3] font-bold hover:underline cursor-pointer transition-all">
						Show all
					</span>
				</div>
			</div>

			<TopTracksTable />
		</>
	)
}

export default ProfileTopTracks
