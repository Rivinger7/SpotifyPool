import {useDispatch} from "react-redux"
import {useEffect} from "react"
import {Helmet} from "react-helmet-async"

import Loader from "@/components/ui/Loader"
import TracksHeader from "@/features/Home/TracksHeader.tsx"
import BoxComponent from "@/features/Home/TracksComponent.tsx"
import AudioPlayer from "@/features/Audio/AudioPlayer"

import {Track} from "@/types"
import {useGetTracksQuery} from "@/services/apiTracks"
import {initializeQueue} from "@/store/slice/playerSlice"

function Home() {
    const dispatch = useDispatch()
    const {data: tracksData = [], isLoading} = useGetTracksQuery({}) as {
        data: Track[]
        isLoading: boolean
    }

    useEffect(() => {
        if (tracksData.length > 0) {
            dispatch(initializeQueue(tracksData))
        }
    }, [dispatch, tracksData])

    if (isLoading) return <Loader/>

    return (
        <div>
            <Helmet>
                <link rel="icon" type="image/svg+xml" href="/Spotify_Icon_RGB_Green.png"/>
                <title>SpotifyPool</title>
            </Helmet>

            {/* ==== AUDIO ==== */}
            <AudioPlayer/>

            <div>
                <section className="pt-6">
                    <div className="flex flex-row flex-wrap pl-6 pr-6 gap-x-6 gap-y-8">
                        <section className="relative flex flex-col flex-1 max-w-full min-w-full">
                            <TracksHeader>Popular tracks</TracksHeader>
                            <div className="grid grid-cols-6">
                                {tracksData?.map((track) => (
                                    <BoxComponent key={track.id} track={track}/>
                                ))}
                            </div>
                        </section>
                    </div>
                </section>
            </div>
        </div>
    )
}

export default Home