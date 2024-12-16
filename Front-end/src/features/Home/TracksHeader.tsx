import React from "react";
import {Link} from "react-router-dom";

interface TracksHeaderProps {
    children: React.ReactNode;
}

const TracksHeader = ({children}: TracksHeaderProps) => {
    return (
        <div className="area-headers flex items-center justify-between mb-1">
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
export default TracksHeader;
