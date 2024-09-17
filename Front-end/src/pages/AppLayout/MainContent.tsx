import React from "react";

interface MainContentProps {
	children: React.ReactNode;
}

const MainContent = ({ children }: MainContentProps) => {
	return (
		<div
			className={
				"main-content-container relative top-0 left-0 bg-[var(--background-base)] rounded-lg"
			}
		>
			{children}
		</div>
	);
};

export default MainContent;
