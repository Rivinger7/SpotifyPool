export interface Images {
	url: string
	width: number
	height: number
}

export interface Artists {
	name: string
	followers: number
	genreIds: string[]
	images: Images[]
}

export interface Track {
	id: string
	name: string
	description: string
	previewURL: string
	duration: number
	durationFormated: string
	images: Images[]
	artists: Artists[]
}

export interface Playlist {
	id: string
	name: string
	images: Images[]
}
