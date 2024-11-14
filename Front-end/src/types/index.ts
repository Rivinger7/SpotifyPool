interface Images {
	url: string
	width: number
	height: number
}

interface Artists {
	name: string
	followers: number
	genreIds: string[]
	images: Images[]
}

export interface Song {
	id: string
	name: string
	description: string
	previewURL: string
	duration: number
	images: Images[]
	artists: Artists[]
}
