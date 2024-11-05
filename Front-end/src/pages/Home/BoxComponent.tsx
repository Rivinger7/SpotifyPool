import { Play } from "lucide-react"
import { Link } from "react-router-dom"

interface BoxComponentProps {
	isAvatar?: boolean
}

const BoxComponent = ({ isAvatar }: BoxComponentProps) => {
	return (
		<div className="group inline-flex flex-col gap-x-2 p-3 rounded-sm hover:bg-[#1f1f1f] transition-all animate-in animate-out cursor-pointer">
			<div className="relative">
				<div>
					<img
						className={`w-full h-full object-cover ${isAvatar ? "rounded-full" : "rounded-md"}`} // Chỉ dùng rounded-full cho ảnh avatar còn lại dùng rounded-lg
						src="https://i.scdn.co/image/ab676161000051745a79a6ca8c60e4ec1440be53"
						alt=""
					/>
				</div>
				<div className="absolute transition-all duration-300 transform translate-y-2 opacity-0 box-play-btn right-2 bottom-2 group-hover:opacity-100 group-hover:translate-y-0">
					<button className="cursor-pointer group">
						<span className="bg-[#1ed760] group-hover:scale-105 group-hover:bg-[#3be477] rounded-full flex items-center justify-center w-12 h-12 text-black">
							<Play className="w-6 fill-current" />
						</span>
					</button>
				</div>
			</div>
			<div>
				<div className="flex flex-col">
					<Link to={"/"} className="font-medium">
						Son Tung MTP
					</Link>
					<div className="text-[#b3b3b3]">Artist</div>
				</div>
			</div>
		</div>
	)
}

export default BoxComponent
