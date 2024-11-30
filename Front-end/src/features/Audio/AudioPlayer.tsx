import { useEffect, useRef } from "react"
import { useDispatch, useSelector } from "react-redux"
import { RootState } from "@/store/store"
import { playNext } from "@/store/slice/playerSlice"

const AudioPlayer = () => {
	const audioRef = useRef<HTMLAudioElement | null>(null)
	const prevSongRef = useRef<string | null>(null)
	const dispatch = useDispatch()

	const { currentTrack, isPlaying } = useSelector((state: RootState) => state.play)

	// NOTE: handle play/pause logic
	useEffect(() => {
		if (isPlaying) audioRef.current?.play()
		else audioRef.current?.pause()
	}, [isPlaying])

	// NOTE: handle track ends -- when this track ends, play the next track
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
		if (!audioRef.current || !currentTrack) return

		const audio = audioRef.current

		// NOTE: check if this is actually a new song
		const isSongChange = prevSongRef.current !== currentTrack?.previewURL
		if (isSongChange) {
			audio.src = currentTrack?.previewURL
			// reset the playback position
			audio.currentTime = 0

			prevSongRef.current = currentTrack?.previewURL

			if (isPlaying) audio.play()
		}
	}, [currentTrack, isPlaying])

	return <audio ref={audioRef} />
}

export default AudioPlayer
