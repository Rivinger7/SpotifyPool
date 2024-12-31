import { useEffect, useRef, useState } from "react"
import { useDispatch, useSelector } from "react-redux"

import { Pause, Play, Repeat2, Shuffle, SkipBack, SkipForward } from "lucide-react"

import SongOptions from "./TrackOptions"
import { Slider } from "@/components/ui/slider"
import { Button } from "@/components/ui/button"
import CustomTooltip from "@/components/CustomTooltip"

import { HttpTransportType, HubConnectionBuilder } from "@microsoft/signalr"

import { RootState } from "@/store/store"
import { playNext, playPrevious, togglePlay, updateCurrentTime } from "@/store/slice/playerSlice"

const formatTime = (seconds: number) => {
	const minutes = Math.floor(seconds / 60)
	const remainingSeconds = Math.floor(seconds % 60)
	return `${minutes}:${remainingSeconds.toString().padStart(2, "0")}`
}

const TrackPlay = () => {
	const dispatch = useDispatch()

	const {
		currentTrack,
		isPlaying,
		currentTime: savedCurrentTime,
	} = useSelector((state: RootState) => state.play)

	const [volume, setVolume] = useState(1)
	const [duration, setDuration] = useState(0)
	const [currentTime, setCurrentTime] = useState(savedCurrentTime)

	// CHECKPOINT: TIMER LOGIC SIGNALR
	const [playTime, setPlayTime] = useState(0)
	const [playCurrentTime, setPlayCurrentTime] = useState(savedCurrentTime || 0)
	const [hasTriggeredStream, setHasTriggeredStream] = useState(false)

	const timerRef = useRef<NodeJS.Timeout | null>(null)
	const audioRef = useRef<HTMLAudioElement | null>(null)
	const timerCurrentTrackRef = useRef<NodeJS.Timeout | null>(null)

	useEffect(() => {
		audioRef.current = document.querySelector("audio")

		const audio = audioRef.current
		if (!audio) return

		audio.currentTime = savedCurrentTime

		const updateTime = () => setCurrentTime(audio.currentTime)

		const updateDuration = () => setDuration(audio.duration)

		audio.addEventListener("timeupdate", updateTime)
		audio.addEventListener("loadedmetadata", updateDuration)

		const handleEnded = () => {
			dispatch(togglePlay())
		}

		audio.addEventListener("ended", handleEnded)

		return () => {
			audio.removeEventListener("timeupdate", updateTime)
			audio.removeEventListener("loadedmetadata", updateDuration)
			audio.removeEventListener("ended", handleEnded)
		}
	}, [currentTrack, dispatch, savedCurrentTime])

	// Effect for setting currentTime
	// useEffect(() => {
	// 	if (audioRef.current && audioRef.current.currentTime !== savedCurrentTime) {
	// 		audioRef.current.currentTime = savedCurrentTime
	// 	}
	// }, [savedCurrentTime])

	useEffect(() => {
		if (isPlaying) {
			timerCurrentTrackRef.current = setInterval(() => {
				setPlayCurrentTime((prev) => prev + 1)
			}, 1000)
			dispatch(updateCurrentTime(playCurrentTime))
		}

		return () => {
			if (timerCurrentTrackRef.current) {
				clearInterval(timerCurrentTrackRef.current)
			}
		}
	}, [dispatch, playCurrentTime, isPlaying])

	// Effect for tracking play time
	useEffect(() => {
		if (isPlaying && !hasTriggeredStream) {
			timerRef.current = setInterval(() => {
				setPlayTime((prev) => prev + 1)
			}, 1000)
		}

		return () => {
			if (timerRef.current) {
				clearInterval(timerRef.current)
			}
		}
	}, [isPlaying, hasTriggeredStream])

	// Effect for SignalR connection after 10 seconds
	useEffect(() => {
		if (playTime >= 10 && !hasTriggeredStream) {
			const connection = new HubConnectionBuilder()
				.withUrl(import.meta.env.VITE_SPOTIFYPOOL_HUB_COUNT_STREAM_URL, {
					// skipNegotiation: true,
					transport: HttpTransportType.WebSockets, // INFO: set this to websockets to use skipNegotiation
					// transport: HttpTransportType.LongPolling,
				})
				.withAutomaticReconnect()
				.build()

			connection
				.start()
				.then(() => {
					console.log("Connected to the hub")
					connection.invoke("UpdateStreamCountAsync", currentTrack?.id)
					setHasTriggeredStream(true) // Prevent multiple triggers
				})
				.catch((err) => console.error(err))

			// Clear timer after triggering
			if (timerRef.current) {
				clearInterval(timerRef.current)
			}
		}
	}, [playTime, currentTrack?.id, hasTriggeredStream])

	// Reset states when song changes
	useEffect(() => {
		setPlayTime(0)
		setPlayCurrentTime(0)
		setHasTriggeredStream(false)
		dispatch(updateCurrentTime(0))
	}, [currentTrack?.id, dispatch])

	const handleSeek = (value: number[]) => {
		if (audioRef.current) {
			audioRef.current.currentTime = value[0]
			setCurrentTime(value[0])
			dispatch(updateCurrentTime(value[0]))
		}
	}

	return (
		<>
			<div className="flex flex-col items-center justify-center max-w-[722px] w-2/5">
				<div className="w-full mb-2 flex flex-nowrap items-center justify-center gap-x-4">
					<CustomTooltip label="Shuffle">
						<Shuffle className="size-4 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
					</CustomTooltip>

					<CustomTooltip label="Previous">
						<SkipBack
							onClick={() => dispatch(playPrevious())}
							className="size-4 text-[#b3b3b3] fill-current hover:text-white cursor-pointer hover:scale-105 transition-all"
						/>
					</CustomTooltip>

					<CustomTooltip label={`${isPlaying ? "Pause" : "Play"}`}>
						<Button
							className="group"
							variant={"play"}
							size={"iconMd"}
							onClick={() => dispatch(togglePlay())}
						>
							{isPlaying ? (
								<Pause className="size-5 text-[#b3b3b3] fill-black stroke-none group-hover:text-white group-hover:scale-105 transition-all" />
							) : (
								<Play className="size-5 text-[#b3b3b3] fill-black stroke-none group-hover:text-white group-hover:scale-105 transition-all" />
							)}
						</Button>
					</CustomTooltip>

					<CustomTooltip label="Next">
						<SkipForward
							onClick={() => dispatch(playNext())}
							className="size-4 text-[#b3b3b3] fill-current hover:text-white cursor-pointer hover:scale-105 transition-all"
						/>
					</CustomTooltip>

					<CustomTooltip label="Enable repeat">
						<Repeat2 className="size-4 text-[#b3b3b3] hover:text-white cursor-pointer hover:scale-105 transition-all" />
					</CustomTooltip>
				</div>

				{/* ==== SLIDER ====  */}
				<div className="w-full flex justify-between items-center gap-x-2">
					<span>{formatTime(currentTime)}</span>
					<Slider
						value={[currentTime]}
						step={1}
						max={duration || 100}
						onValueChange={handleSeek}
						className="w-full hover:cursor-grab active:cursor-grabbing"
					/>
					<span>{formatTime(duration)}</span>
				</div>
			</div>

			{/* ==== SONG OPTIONS ==== */}
			<SongOptions audioRef={audioRef} volume={volume} setVolume={setVolume} />
		</>
	)
}

export default TrackPlay
