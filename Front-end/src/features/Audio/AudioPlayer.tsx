import { useEffect, useRef } from "react"
import { useDispatch, useSelector } from "react-redux"
import { RootState } from "@/store/store"
import { playNext } from "@/store/slice/playerSlice"

const AudioPlayer = () => {
	const audioRef = useRef<HTMLAudioElement | null>(null)
	const prevSongRef = useRef<string | null>(null)
	const dispatch = useDispatch()

	const { currentSong, isPlaying } = useSelector((state: RootState) => state.play)

	// NOTE: handle play/pause logic
	useEffect(() => {
		if (isPlaying) audioRef.current?.play()
		else audioRef.current?.pause()
	}, [isPlaying])

	// NOTE: handle song ends -- when this song ends, play the next song
	useEffect(() => {
		const audio = audioRef.current

		const handleEnded = () => {
			dispatch(playNext())
		}

		audio?.addEventListener("ended", handleEnded)

		return () => audio?.removeEventListener("ended", handleEnded)
	}, [dispatch])

	// NOTE: handle song changes
	useEffect(() => {
		if (!audioRef.current || !currentSong) return

		const audio = audioRef.current

		// NOTE: check if this is actually a new song
		const isSongChange = prevSongRef.current !== currentSong?.previewURL
		if (isSongChange) {
			audio.src = currentSong?.previewURL
			// reset the playback position
			audio.currentTime = 0

			prevSongRef.current = currentSong?.previewURL

			if (isPlaying) audio.play()
		}
	}, [currentSong, isPlaying])

	return <audio ref={audioRef} />
}

export default AudioPlayer
