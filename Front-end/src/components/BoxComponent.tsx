import { Link } from "react-router-dom";

interface BoxComponentProps {
	isAvatar?: boolean;
}

const BoxComponent = ({ isAvatar }: BoxComponentProps) => {
	return (
		<div className="box-component group inline-flex flex-col gap-x-2 p-3 rounded-sm hover:bg-[#1f1f1f] transition-all animate-in animate-out cursor-pointer">
			<div className="relative">
				<div>
					<img
						className={`w-full h-full object-cover ${
							isAvatar ? "rounded-full" : "rounded-md"
						}`} // Chỉ dùng rounded-full cho ảnh avatar còn lại dùng rounded-lg
						src="https://i.scdn.co/image/ab676161000051745a79a6ca8c60e4ec1440be53"
						alt=""
					/>
				</div>
				<div className="box-play-btn absolute right-2 bottom-2 transition-all duration-300 opacity-0 transform translate-y-2 group-hover:opacity-100 group-hover:translate-y-0">
					<button className="box-btn cursor-pointer">
						<span className="bg-[#1ed760] rounded-full flex items-center justify-center w-12 h-12 text-black">
							<svg
								data-encore-id="icon"
								role="img"
								aria-hidden="true"
								viewBox="0 0 24 24"
								className="w-6"
								fill="currentColor"
							>
								<path d="m7.05 3.606 13.49 7.788a.7.7 0 0 1 0 1.212L7.05 20.394A.7.7 0 0 1 6 19.788V4.212a.7.7 0 0 1 1.05-.606z"></path>
							</svg>
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
	);
};

export default BoxComponent;
