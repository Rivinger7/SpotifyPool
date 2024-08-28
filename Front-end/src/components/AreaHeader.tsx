import React from "react";
import { Link } from "react-router-dom";

interface AreaHeaderProps {
	children: React.ReactNode;
}

const AreaHeader = ({ children }: AreaHeaderProps) => {
	return (
		<div className="area-headers">
			// NOTE: The children prop is used to display the title of the area
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
