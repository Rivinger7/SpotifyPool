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

export interface Song {
	id: string
	name: string
	description: string
	previewURL: string
	duration: number
	images: Images[]
	artists: Artists[]
}

export interface Playlist {
	id: string
	name: string
	images: Images[]
}
