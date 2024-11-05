import React from "react"

interface MainContentProps {
	children: React.ReactNode
}

const MainContent = ({ children }: MainContentProps) => {
	return (
		<div
			className={
				"relative rounded-lg flex gap-2 my-2 h-[calc(100vh_-_72px_-_76px)] overflow-y-auto"
			}
		>
			{children}
		</div>
	)
}

export default MainContent
