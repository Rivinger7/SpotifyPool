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
	artists?: Artists[] | []
}

export interface Playlist {
	id: string
	name: string
	images: Images[]
}

export interface Avatar {
	url: string
	height: number
	width: number
}

export interface User {
	id: string
	name: string
	avatar: Avatar[]
}

export interface TrackPlaylist {
	id: string
	name: string
	description: string
	duration: number
	durationFormated: string
	previewURL: string
	addedTime: string
	images: Images[]
	artists: Artists[]
}

export interface PlaylistDetail {
	id: string
	title: string
	images: Images[]
	userId: string
	displayName: string
	avatar: Avatar
	totalTracks: number
	tracks: TrackPlaylist[]
}

export interface TopTracksDetail {
	trackId: string
	streamCount: number
	firstAccessTime: Date
	track: Track
	artists: string[]
}

export interface TopTracks {
	topTrackId: string
	userId: string
	trackInfo: TopTracksDetail[]
}
