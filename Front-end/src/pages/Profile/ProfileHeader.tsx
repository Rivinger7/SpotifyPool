import EditIcon from "@/assets/icons/EditIcon";
import ProfileModal from "./ProfileModal";

export default function ProfileHeader() {
	return (
		<div className="profile">
			<div className="bg-style" style={{ backgroundColor: "rgb(136, 64, 56)" }}></div>
			<div className="bg-style gradient"></div>
			<div className="info">
				<div className="user-image">
					<div className="style">
						<div className="image">
							<img src="/avatar-formal.jpg" alt="" />
						</div>
						<div className="cta-btn">
							<div className="cover">
								<ProfileModal>
									<button type="button" className="edit-image-button">
										<div className="icon">
											<EditIcon />
											<span
												className="encore-text encore-text-body-medium jN7ZUHc7IxpwvWsjb4jo"
												data-encore-id="text"
											>
												Choose photo
											</span>
										</div>
									</button>
								</ProfileModal>
							</div>
						</div>
					</div>
				</div>
				<div className="user-name">
					<span className="text-sm">Profile</span>
					<span>
						<ProfileModal>
							<button type="button">
								<h1 className="font-bold tracking-tight text-8xl">Phuc Hoa</h1>
							</button>
						</ProfileModal>
					</span>
					<span className="mt-4">1 Public playlist</span>
				</div>
			</div>
		</div>
	);
}
