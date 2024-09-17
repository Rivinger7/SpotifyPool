import React from "react";
import { Link } from "react-router-dom";

interface AreaHeaderProps {
	children: React.ReactNode;
}

const AreaHeader = ({ children }: AreaHeaderProps) => {
	return (
		<div className="area-headers">
			{/* Display title */}
			<div className="text-2xl font-bold">{children}</div>
			<div>
				<Link to={"/"} className="hover:underline text-[#b3b3b3]">
					Show all
				</Link>
			</div>
		</div>
	);
};
export default AreaHeader;
