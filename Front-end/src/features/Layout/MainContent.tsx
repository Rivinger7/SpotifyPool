import React from "react"

interface MainContentProps {
	children: React.ReactNode
	mainHeight: string
}

const MainContent = ({ children, mainHeight }: MainContentProps) => {
	return <div className={`relative rounded-lg flex gap-2 my-2 ${mainHeight}`}>{children}</div>
}

export default MainContent
