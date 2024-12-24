function formatTimeMiliseconds(miliseconds: number) {
	const minutes = Math.floor(miliseconds / (1000 * 60))
	const seconds = Math.floor((miliseconds % (1000 * 60)) / 1000)

	return `${minutes}:${seconds.toString().padStart(2, "0")}`
}

export default formatTimeMiliseconds
